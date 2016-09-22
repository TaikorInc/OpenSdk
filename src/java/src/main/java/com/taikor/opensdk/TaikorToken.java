package com.taikor.opensdk;

/**
 * Created by Carey on 2016/7/13.
 *
*/
class TaikorToken {
    /**
    * Access Token
    */
    public String AccessToken;
    /**
    * 错误信息
    */
    public String Error;
    /**
    * 过期时间
    */
    public long ExpiresIn;
    /**
    * Http错误信息
    */
    public String HttpErrorReason;
    /**
    * Http错误状态码
    */
    public int HttpErrorStatusCode;
    /**
    * 是否错误
    */
    public boolean IsError;
    /**
    * 是否Http错误
    */
    public boolean IsHttpError;
    /**
    * Refresh Token
    */
    public String RefreshToken;
    /**
    * Token Type
    */
    public String TokenType;
}
