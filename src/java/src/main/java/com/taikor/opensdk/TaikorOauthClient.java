package com.taikor.opensdk;

import com.alibaba.fastjson.JSON;

import javax.net.ssl.*;
import java.io.*;
import java.net.URL;
import java.net.URLConnection;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Created by Carey on 2016/7/13.
 */
public class TaikorOauthClient {

    String userId;
    String appSecret;
    String baseApiUrl;
    boolean isAccessTokenSet;
    long tokenExpiresTime ;
    String tokenType;
    String accessToken;

    public TaikorOauthClient(String uId,String appS){
        this.userId = uId;
        this.appSecret = appS;
        this.baseApiUrl = "https://api.palaspom.com/";
        this.isAccessTokenSet = false;
        this.tokenExpiresTime = (long) 0;
        this.tokenType = "";
        this.accessToken = "";
        if(! this.requestToken()){
            System.out.println("Init the client error, Please check UserId and AppSecert setting. Or you can retry it.");
        }
    }

    public boolean isAuthorized() {
        if (this.isAccessTokenSet && this.accessToken != "" && this.tokenType != "" && ((this.tokenExpiresTime - new Date().getTime()) > 10 * 60 * 1000)) {
            return true;
        } else {
            return false;
        }
    }


    public boolean requestToken(){

        if(this.userId == null && this.appSecret == null){
            System.out.println("Please set the UserId and AppSecert before you request." +
                    " You can get this from https://daas.palaspom.com/Login/Authority.");
            return  false;
        }

        Map<String, Object> parameters = new HashMap<String, Object>();
        parameters.put("userId",this.userId);
        parameters.put("appSecert", this.appSecret);
        this.isAccessTokenSet = false;
        String api = "Oauth2/Authorize";

        String httpContent = this.httpGet(api,parameters,false);
        TaikorToken result = JSON.parseObject(httpContent, TaikorToken.class);

        if(result != null && !result.IsError && !result.IsHttpError && result.AccessToken != null && result.AccessToken != "")
        {
            this.tokenExpiresTime = new Date().getTime() + result.ExpiresIn * 1000;
            this.accessToken = result.AccessToken;
            this.tokenType = result.TokenType;
            this.isAccessTokenSet = true;
            return true;
        }else {
            return false;
        }
    }

    public  String httpGet(String api, Map<String,Object> parameters)
    {
        return this.httpGet(api, parameters, true);
    }

    public String httpGet(String api, Map<String,Object> parameters, boolean needAuthorized)
    {
        if(!needAuthorized || this.isAuthorized()|| this.requestToken())
        {
            try {
                String urlstr = api;
                if(!api.startsWith("http")){
                    urlstr = this.baseApiUrl + urlstr;
                }

                if(parameters != null && parameters.size()>0){
                    if(urlstr.indexOf("?")<0){
                        urlstr = urlstr + "?";
                    }else{
                        urlstr = urlstr + "&";
                    }
                    for(String key: parameters.keySet()){
                        urlstr = urlstr + key + "=" + parameters.get(key) + "&";
                    }
                    urlstr = urlstr.substring(0,urlstr.length()-1);//去掉末尾的“&"
                }

                // 创建SSLContext对象，并使用我们指定的信任管理器初始化
                TrustManager[] tm = { new TaikorX509TrustManager() };
                SSLContext sslContext = SSLContext.getInstance("SSL", "SunJSSE");
                sslContext.init(null, tm, new java.security.SecureRandom());
                // 从上述SSLContext对象中得到SSLSocketFactory对象
                SSLSocketFactory ssf = sslContext.getSocketFactory();
                URL realUrl = new URL(urlstr);
                // 打开和URL之间的连接
                HttpsURLConnection connection = (HttpsURLConnection) realUrl.openConnection();
                connection.setSSLSocketFactory(ssf);
                connection.setHostnameVerifier(DO_NOT_VERIFY);

                // 设置通用的请求属性
                connection.setRequestProperty("accept", "*/*");
                connection.setRequestProperty("connection", "Keep-Alive");
                connection.setRequestProperty("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.84 Safari/537.36");
                connection.setRequestProperty("Accept-Charset", "UTF-8");
                connection.setRequestProperty("Content-Type", "application/json");

                if(needAuthorized){
                    connection.setRequestProperty("Authorization", this.tokenType + ' ' + this.accessToken);
                }

                // 建立实际的连接
                connection.connect();
                // 获取所有响应头字段
                int responseCode = connection.getResponseCode();
                if(responseCode == 401 || responseCode == 403)
                {
                    this.isAccessTokenSet = false;
                    this.requestToken();
                }
                else if(responseCode == 200)
                {
                   return getResponseAsString(connection);
                }

            } catch (Exception e) {
                return null;
            }
        }
        return  null;
    }

    public  String httpPost(String api, Map<String,Object> parameters)
    {
        return this.httpPost(api, parameters, true);
    }

    public String httpPost(String api, Map<String,Object> parameters, boolean needAuthorized)
    {
        PrintWriter out = null;
        if((!needAuthorized)|| this.isAuthorized()|| this.requestToken())//if
        {
            try {

                String urlstr = api;
                if(!api.startsWith("http")){
                    urlstr = urlstr + this.baseApiUrl;
                }

                // 创建SSLContext对象，并使用我们指定的信任管理器初始化
                TrustManager[] tm = { new TaikorX509TrustManager() };
                SSLContext sslContext = SSLContext.getInstance("SSL", "SunJSSE");
                sslContext.init(null, tm, new java.security.SecureRandom());
                // 从上述SSLContext对象中得到SSLSocketFactory对象
                SSLSocketFactory ssf = sslContext.getSocketFactory();
                URL realUrl = new URL(urlstr);
                // 打开和URL之间的连接
                HttpsURLConnection connection = (HttpsURLConnection) realUrl.openConnection();
                connection.setSSLSocketFactory(ssf);
                connection.setHostnameVerifier(DO_NOT_VERIFY);

                // 设置通用的请求属性
                connection.setRequestProperty("accept", "*/*");
                connection.setRequestProperty("connection", "Keep-Alive");
                connection.setRequestProperty("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.84 Safari/537.36");
                connection.setRequestProperty("Accept-Charset", "UTF-8");
                connection.setRequestProperty("Content-Type", "application/json");

                if(needAuthorized){
                    connection.setRequestProperty("Authorization", this.tokenType + ' ' + this.accessToken);
                }

                // 发送POST请求必须设置如下两行
                connection.setDoOutput(true);
                connection.setDoInput(true);

                if(parameters != null && parameters.size() > 0 ){
                    // 获取URLConnection对象对应的输出流
                    out = new PrintWriter(connection.getOutputStream());
                    // 发送请求参数
                    out.print(parameters);
                    // flush输出流的缓冲
                    out.flush();
                }

                // 建立实际的连接
                connection.connect();
                // 获取所有响应头字段
                int responseCode = connection.getResponseCode();
                if(responseCode == 401 || responseCode == 403)
                {
                    this.isAccessTokenSet = false;
                    this.requestToken();
                }
                else if(responseCode == 200)
                {
                    return getResponseAsString(connection);
                }
            }
            catch (Exception e) {
                return null;
            }
        }
        return  null;
    }

    protected static String getResponseAsString(HttpsURLConnection conn) throws IOException {
        InputStream es = conn.getErrorStream();
        if (es == null) {
            return getStreamAsString(conn.getInputStream(), "utf-8");
        } else {
            String msg = getStreamAsString(es, "utf-8");
            if (msg == null || msg == "") {
                throw new IOException(conn.getResponseCode() + ":" + conn.getResponseMessage());
            } else {
                throw new IOException(msg);
            }
        }
    }

    private static String getStreamAsString(InputStream stream, String charset) throws IOException {
        try {
            BufferedReader reader = new BufferedReader(new InputStreamReader(stream, charset));
            StringWriter writer = new StringWriter();

            char[] chars = new char[256];
            int count = 0;
            while ((count = reader.read(chars)) > 0) {
                writer.write(chars, 0, count);
            }

            return writer.toString();
        } finally {
            if (stream != null) {
                stream.close();
            }
        }
    }

    final static HostnameVerifier DO_NOT_VERIFY = new HostnameVerifier() {

        public boolean verify(String hostname, SSLSession session) {
            return true;
        }
    };
}