using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taikor.Opensdk
{
    /// <summary>
    /// Taikor 用于返回的Token类
    /// </summary>
    public class TaikorToken
    {
        /// <summary>
        /// Access Token, 
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public long ExpiresIn { get; set; }
        /// <summary>
        /// Http错误信息
        /// </summary>
        public string HttpErrorReason { get; set; }
        /// <summary>
        /// Http错误状态码
        /// </summary>
        public int HttpErrorStatusCode { get; set; }
        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// 是否Http错误
        /// </summary>
        public bool IsHttpError { get; set; }
        /// <summary>
        /// Refresh Token
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// Token Type
        /// </summary>
        public string TokenType { get; set; }
    }
}
