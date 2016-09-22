package com.taikor.opensdk;

import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import javax.net.ssl.X509TrustManager;

/**
 * Created by Carey on 2016/7/13.
 */
public class TaikorX509TrustManager implements X509TrustManager {
    /*
     * The default X509TrustManager returned by SunX509.  We'll delegate
     * decisions to it, and fall back to the logic in this class if the
     * default X509TrustManager doesn't trust it.
     */
    X509TrustManager sunJSSEX509TrustManager;

    TaikorX509TrustManager() throws Exception {
    }
    /*
     * Delegate to the default trust manager.
     */
    public void checkClientTrusted(X509Certificate[] chain, String authType)
            throws CertificateException {

    }
    /*
     * Delegate to the default trust manager.
     */
    public void checkServerTrusted(X509Certificate[] chain, String authType)
            throws CertificateException {

    }
    /*
     * Merely pass this through.
     */
    public X509Certificate[] getAcceptedIssuers() {
       return null;
    }
}