#/usr/bin/python2
#encoding=utf-8  
import sys, urllib, urllib2, json, ssl
from datetime import datetime,timedelta

ssl._create_default_https_context = ssl._create_unverified_context
class TaikorOauthClient:
    def __init__(self, userId, appSecert):
        self.userId = userId
        self.appSecert = appSecert
        self.baseApiUrl = "https://api.palaspom.com/"
        self.isAccessTokenSet = False
        self.tokenExpiresTime = datetime.min
        self.tokenType = ""
        self.accessToken = ""
        if (not self.RequestToken()):
	        raise TypeError("Init the client error, Please check UserId and AppSecert setting. Or you can retry it.");

    def IsAuthorized(self):
        if(self.isAccessTokenSet and self.accessToken and self.tokenType and self.tokenExpiresTime > (datetime.now() + timedelta(minutes = 10))):
            return True
        else:
            return False

    def httpGet(self, api, parameters, needAuthorized = True):
        if((not needAuthorized) or self.IsAuthorized() or self.RequestToken()):
            if(not api.startswith('http')):
                api = self.baseApiUrl + api
            if(parameters):
                if(api.find('?') < 0):
                    api = api + "?"
                else:
                    api = api + "&"
                for (d,x) in parameters.items():
                    api = api + d + "=" + str(x) + "&"                
                api = api.rstrip('&')
            req = urllib2.Request(api)  
            req.add_header('Content-Type', 'application/json') 
            if(needAuthorized):
                req.add_header('Authorization', self.tokenType + ' ' + self.accessToken)
            try:
                resp = urllib2.urlopen(req)
                content = resp.read()  
                if(content):
                    result = json.loads(content)
                    return result
            except urllib2.HTTPError as e:
                if(e.code == 401 or e.code == 403):
                    self.RequestToken()
                    req.add_header('Authorization', self.tokenType + ' ' + self.accessToken)
                    resp = urllib2.urlopen(req)
                    content = resp.read()  
                    if(content):
                        result = json.loads(content)
                        return result
        else:
            raise TypeError("Error, Has refused to authorize this request.")

    def httpPost(self, api, parameters, needAuthorized = True):
        if((not needAuthorized) or self.IsAuthorized() or self.RequestToken()):
            if(not api.startswith('http')):
                api = self.baseApiUrl + api
            params = urllib.urlencode(parameters)
            req = urllib2.Request(api, params)  
            req.add_header('Content-Type', 'application/json') 
            if(needAuthorized):
                req.add_header('Authorization', self.tokenType + ' ' + self.accessToken)
            try:
                resp = urllib2.urlopen(req)
                content = resp.read()  
                if(content):
                    result = json.loads(content)
                    return result
            except urllib2.HTTPError as e:
                if(e.code == 401 or e.code == 403):
                    self.RequestToken()
                    req.add_header('Authorization', self.tokenType + ' ' + self.accessToken)
                    resp = urllib2.urlopen(req)
                    content = resp.read()  
                    if(content):
                        result = json.loads(content)
                        return result
        else:
            raise TypeError("Error, Has refused to authorize this request.")

    def RequestToken(self):
        if(not self.userId and not self.appSecert):
            raise TypeError("Please set the UserId and AppSecert before you request. You can get this from https://daas.palaspom.com/Login/Authority.")
        paramsData = {'userId': self.userId, 'appSecert': self.appSecert}   
        self.isAccessTokenSet = False
        api = "Oauth2/Authorize";
        result = self.httpGet(api,paramsData,False)
        print(result)
        if(result):
            if(result and (not result['IsError']) and (not result['IsHttpError']) and result['AccessToken']):
                self.tokenExpiresTime = datetime.now() + timedelta(seconds = int(result['ExpiresIn']))
                self.accessToken = result['AccessToken']
                self.tokenType = result['TokenType']
                self.isAccessTokenSet = True
                return True
        else:
            return False 