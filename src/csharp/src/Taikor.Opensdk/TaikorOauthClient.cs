using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Taikor.Opensdk
{
    /// <summary>
    /// Taikor Open api Oauth client.
    /// </summary>
    public class TaikorOauthClient
    {
        public string UserId { get; protected set; }
        public string AppSecret { get; protected set; }
        public string AccessToken { get; set; }
        public DateTime TokenExpiresTime { set; get; }
        public string TokenType { set; get; }

        public bool IsAuthorized
        {
            get { return isAccessTokenSet && !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(TokenType) && TokenExpiresTime > DateTime.Now.AddMinutes(10); }
        }

        protected string BaseApiUrl {
            get {
                return "https://api.palaspom.com/";
            }
        }

        protected bool isAccessTokenSet = false;

        protected HttpClient http;

        public HttpResponseMessage HttpGet(string api, object parameters, bool needAuthorized = true)
        {
            HttpResponseMessage result = HttpGetAsync(api, parameters, needAuthorized).Result;

            if (result != null && (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.Forbidden))
            {
                isAccessTokenSet = false;
                result = HttpGetAsync(api, parameters, needAuthorized).Result;
            }

            return result;
        }

        private Task<HttpResponseMessage> HttpGetAsync(string api, object parameters, bool needAuthorized = true)
        {
            var paramType = parameters.GetType();
            if (!(paramType.Name.Contains("AnonymousType") && paramType.Namespace == null))
            {
                throw new ArgumentException("Only anonymous type parameters are supported.");
            }

            var dict = paramType.GetProperties().ToDictionary(k => k.Name, v => string.Format("{0}", v.GetValue(parameters, null)));

            return HttpGetAsync(api, dict, needAuthorized);
        }


        public HttpResponseMessage HttpGet(string api, Dictionary<string, object> parameters = null, bool needAuthorized = true)
        {
            HttpResponseMessage result = HttpGetAsync(api, parameters, needAuthorized).Result;

            if (result != null && (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.Forbidden))
            {
                isAccessTokenSet = false;
                result = HttpGetAsync(api, parameters, needAuthorized).Result;
            }

            return result;
        }

        private Task<HttpResponseMessage> HttpGetAsync(string api, Dictionary<string, object> parameters = null, bool needAuthorized = true)
        {
            if (!needAuthorized || IsAuthorized || RequestToken())
            {
                if (parameters == null)
                    parameters = new Dictionary<string, object>();

                var queryString = string.Join("&", parameters.Select(p => string.Format("{0}={1}", Uri.EscapeDataString(p.Key), Uri.EscapeDataString(string.Format("{0}", p.Value)))));

                if (api.IndexOf("?") < 0)
                {
                    api = string.Format("{0}?{1}", api, queryString);
                }
                else
                {
                    api = string.Format("{0}&{1}", api, queryString);
                }

                api = api.Trim('&', '?');

                //If the result is 
                return http.GetAsync(api);
            }
            else
                throw new Exception("Error, Has refused to authorize this request.");
        }

        public HttpResponseMessage HttpPost(string api, object parameters, bool needAuthorized = true)
        {
            HttpResponseMessage result = HttpPostAsync(api, parameters, needAuthorized).Result;

            if (result != null && (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.Forbidden))
            {
                isAccessTokenSet = false;
                result = HttpPostAsync(api, parameters, needAuthorized).Result;
            }

            return result;
        }

        private Task<HttpResponseMessage> HttpPostAsync(string api, object parameters, bool needAuthorized = true)
        {
            var paramType = parameters.GetType();
            if (!(paramType.Name.Contains("AnonymousType") && paramType.Namespace == null))
            {
                throw new ArgumentException("Only anonymous type parameters are supported.");
            }

            var dict = paramType.GetProperties().ToDictionary(k => k.Name, v => v.GetValue(parameters, null));

            return HttpPostAsync(api, dict, needAuthorized);
        }

        public HttpResponseMessage HttpPost(string api, Dictionary<string, object> parameters, bool needAuthorized = true)
        {
            HttpResponseMessage result = HttpPostAsync(api, parameters, needAuthorized).Result;

            if (result != null && (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.Forbidden))
            {
                isAccessTokenSet = false;
                result = HttpPostAsync(api, parameters, needAuthorized).Result;
            }

            return result;
        }

        private Task<HttpResponseMessage> HttpPostAsync(string api, Dictionary<string, object> parameters, bool needAuthorized = true)
        {
            if (!needAuthorized || IsAuthorized || RequestToken())
            {
                if (parameters == null)
                    parameters = new Dictionary<string, object>();

                var dict = new Dictionary<string, object>(parameters.ToDictionary(k => k.Key, v => v.Value));

                HttpContent httpContent = null;

                if (dict.Count(p => p.Value.GetType() == typeof(byte[]) || p.Value.GetType() == typeof(System.IO.FileInfo)) > 0)
                {
                    var content = new MultipartFormDataContent();

                    foreach (var param in dict)
                    {
                        var dataType = param.Value.GetType();
                        if (dataType == typeof(byte[])) //byte[]
                        {
                            content.Add(new ByteArrayContent((byte[])param.Value), param.Key, GetNonceString());
                        }
                        else if (dataType == typeof(System.IO.FileInfo))
                        {
                            var file = (System.IO.FileInfo)param.Value;
                            content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(file.FullName)), param.Key, file.Name);
                        }
                        else
                        {
                            content.Add(new StringContent(string.Format("{0}", param.Value)), param.Key);
                        }
                    }

                    httpContent = content;
                }
                else
                {
                    var content = new FormUrlEncodedContent(dict.ToDictionary(k => k.Key, v => string.Format("{0}", v.Value)));
                    httpContent = content;
                }

                return http.PostAsync(api, httpContent);
            }
            else
                throw new Exception("Error, Has refused to authorize this request.");
        }

        /// <summary>
        /// Init method
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="appSecret">AppSecret</param>
        public TaikorOauthClient(string userId, string appSecret)
        {
            this.UserId = userId;
            this.AppSecret = appSecret;

            var handler = new HttpClientHandler()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip                
            };

            http = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseApiUrl),
                Timeout = new TimeSpan(0, 0, 30)
            };

            if (!RequestToken())
            {
                throw new Exception("Init the client error, Please check UserId and AppSecert setting. Or you can retry it.");
            }
        }

        /// <summary>
        /// Get the Nonce String
        /// </summary>
        /// <param name="length">length</param>
        /// <returns></returns>
        private string GetNonceString(int length = 8)
        {
            var sb = new StringBuilder();

            var rnd = new Random();
            for (var i = 0; i < length; i++)
            {
                sb.Append((char)rnd.Next(97, 123));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Request the Access Token
        /// </summary>
        /// <returns></returns>
        public bool RequestToken()
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(AppSecret))
                throw new ArgumentException("Please set the UserId and AppSecert before you request. You can get this from https://daas.palaspom.com/Login/Authority.");

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", UserId);
            parameters.Add("appSecert", AppSecret);

            isAccessTokenSet = false;
            http.DefaultRequestHeaders.Remove("Authorization");

            HttpResponseMessage result = HttpGet("Oauth2/Authorize", parameters, false);
            if (result != null && result.IsSuccessStatusCode && result.Content != null)
            {
                string tokenResult = result.Content.ReadAsStringAsync().Result;
                TaikorToken token = JsonConvert.DeserializeObject<TaikorToken>(tokenResult);

                if (token != null && !token.IsError && !token.IsHttpError && !string.IsNullOrEmpty(token.AccessToken))
                {
                    TokenExpiresTime = DateTime.Now.AddSeconds(token.ExpiresIn);
                    AccessToken = token.AccessToken;
                    TokenType = token.TokenType;
                    isAccessTokenSet = true;

                    http.DefaultRequestHeaders.Add("Authorization", TokenType + " " + AccessToken);
                    return true;
                }
            }

            return false;
        }
    }
}
