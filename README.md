# OpenSdk

## Overview

sdk for taikor daas api.

## Getting started

### csharp

    public static void Main(string[] args)
    {
        try
        {
            TaikorOauthClient client = new TaikorOauthClient("userId", "appSecert");
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["aaa"] = "bbb";
            string test = client.HttpGet("Controller/action", parameters).Content.ReadAsStringAsync().Result;
        }
        catch(Exception ex) { }
    }

### Java

    public static void main(String[] argv){
        TaikorOauthClient toc = new TaikorOauthClient("userId","appSecert");

        Map<String,Object> parameters = new HashMap<String,Object>();
        parameters.put("aaa","bbb");

        Strings=toc.httpGet("Controller/action",parameters);
    }

### python

    Client = TaikorOauthClient('userId','appSecert')
    parameters = {'aaa':'bbb'}
    Client.httpGet('Controller/action',parameters)
