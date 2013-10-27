import os
import webapp2
import cgi
import json
import jinja2
import time
import hashlib

from uuid import uuid4
from google.appengine.ext import ndb

JINJA_ENVIRONMENT = jinja2.Environment(
    loader=jinja2.FileSystemLoader(os.path.dirname(__file__)),
    extensions=['jinja2.ext.autoescape'])


defaultUserName="admin"
defaultPassword="8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918"

"""---------Functions--------------"""
def isAdmin(userName,password):
    users=User.query()
    for user in users:
        if user.userName==userName and user.password==password and user.isAdmin=="true":
            return True
    if defaultUserName==userName and defaultPassword==password:
        return True
    return False


def validToken(theTokenString):
    cleanToken()
    tokens=Token.query()
    for token in tokens:
        if token.tokenString == theTokenString:
            token.key.delete()
            return True
    return False

def cleanToken():
    tokens=Token.query()
    for token in tokens:
        if float(token.timeStamp)+300<time.time():
            token.key.delete()
    return

def getToken():
    newToken=Token()
    newToken.timeStamp=str(time.time())
    key=newToken.put()
    newToken.tokenString=str(key.id())
    newToken.put()
    return newToken.tokenString

"""----------------------------------"""

""" --------Objects---------------"""
class User(ndb.Model):
    userName = ndb.StringProperty()
    password = ndb.StringProperty()
    isAdmin=ndb.StringProperty()

class Token(ndb.Model):
    tokenString=ndb.StringProperty()
    timeStamp=ndb.StringProperty()

class Field(ndb.Model):
    farmName=ndb.StringProperty()
    fieldName=ndb.StringProperty()
    acre=ndb.StringProperty()
    note=ndb.StringProperty()
    timeStamp=ndb.StringProperty()


class Bin(ndb.Model):
    binID=ndb.StringProperty()
    binSize=ndb.StringProperty()
    bushel=ndb.StringProperty()
    crop=ndb.StringProperty()
    moister=ndb.StringProperty()
    timeStamp=ndb.StringProperty()


class RainGuage(ndb.Model):
    farmName=ndb.StringProperty()
    theDate=ndb.StringProperty()
    rain=ndb.StringProperty()
    timeStamp=ndb.StringProperty()

      #--------------------------------
class recieveRainGuage(webapp2.RequestHandler):
    def get(self):
        rainGuages_query=RainGuage.query()
        rainGuages=rainGuages_query.fetch()
        hasRainGuage=False
        response=[]
    
        #when there's data in server
        for rainGuage in rainGuages:
            if rainGuage.farmName==self.request.get('farmName') and rainGuage.theDate==self.request.get('theDate'):
                hasRainGuage=True
                #when client data is newer
                if float(rainGuage.timeStamp)==float(self.request.get('timeStamp')):
                    rainGuage.rain=self.request.get('rain')
                    rainGuage.timeStamp=str(time.time())
                    rainGuage.put();
                    response.append({'result':"success",'action':"update",'timeStamp':rainGuage.timeStamp})
                    self.response.write(json.dumps(response))
                    return
                #when client data is older
                if float(rainGuage.timeStamp)>float(self.request.get('timeStamp')):
                    response.append({'result':"fail",'action':"sent",'timeStamp':rainGuage.timeStamp})
                    response.append({'rain':rainGuage.rain,'farmName':rainGuage.farmName,'theDate':rainGuage.theDate})
                    self.response.write(json.dumps(response))
                    return
        #when there's no data in server
        if not hasRainGuage:
            rainGuage=RainGuage()
            rainGuage.farmName=self.request.get('farmName')
            rainGuage.theDate=self.request.get('theDate')
            rainGuage.rain=self.request.get('rain')
            rainGuage.timeStamp=str(time.time())
            rainGuage.put();
            response.append({'result':"success",'action':"add",'timeStamp':rainGuage.timeStamp})
            self.response.write(json.dumps(response))

"""
    test url:
    
    add new rainGuage:
    http://localhost:8080/uploadRainGuage?farmName=Lux&theDate=8/22/2013&rain=12&timeStamp=0
    http://localhost:8080/uploadRainGuage?farmName=Lux&theDate=9/22/2013&rain=12&timeStamp=0
    
    modify existing field:
    http://localhost:8080/uploadRainGuage?farmName=Lux&theDate=8/22/2013&rain=20&timeStamp=(......add something here)
"""

class sentRainGuage(webapp2.RequestHandler):
    def get(self):
        response=[]
        rainGuages_query=RainGuage.query()
        rainGuages=rainGuages_query.fetch()
        for rainGuage in rainGuages:
            response.append({'farmName':rainGuage.farmName,'theDate':rainGuage.theDate,'timeStamp':rainGuage.timeStamp,'rain':rainGuage.rain})
        self.response.write(json.dumps(response))

"""
    test url:
    get all existing fields:
    http://localhost:8080/downloadRainGuage    
    """

#--------------------------------
class recieveBin(webapp2.RequestHandler):
    def get(self):
        bins_query=Bin.query()
        bins=bins_query.fetch()
        hasBin=False
        response=[]
        
        #when there's data in server
        for bin in bins:
            if bin.binID==self.request.get('binID'):
                hasBin=True
                #when client data is newer
                if float(bin.timeStamp)==float(self.request.get('timeStamp')):
                    bin.binID=self.request.get('binID')
                    bin.binSize=self.request.get('binSize')
                    bin.bushel=self.request.get('bushel')
                    bin.crop=self.request.get('crop')
                    bin.moister=self.request.get('moister')
                    bin.timeStamp=str(time.time())
                    bin.put();
                    response.append({'result':"success",'action':"update",'timeStamp':bin.timeStamp})
                    self.response.write(json.dumps(response))
                    return
                #when client data is older
                if float(bin.timeStamp)>float(self.request.get('timeStamp')):
                    response.append({'result':"fail",'action':"sent",'timeStamp':bin.timeStamp})
                    response.append({'binID':bin.binID,'binSize':bin.binSize,'bushel':bin.bushel,'crop':bin.crop,'moister':bin.moister,'timeStamp':bin.timeStamp})
                    self.response.write(json.dumps(response))
                    return
        #when there's no data in server
        if not hasBin:
            bin=Bin()
            bin.binID=self.request.get('binID')
            bin.binSize=self.request.get('binSize')
            bin.bushel=self.request.get('bushel')
            bin.crop=self.request.get('crop')
            bin.moister=self.request.get('moister')
            bin.timeStamp=str(time.time())
            bin.put();
            response.append({'result':"success",'action':"add",'timeStamp':bin.timeStamp})
            self.response.write(json.dumps(response))

"""
    test url:
    
    add new bin:
    http://localhost:8080/uploadBin?binID=1&binSize=11&bushel=1&crop=weed&moister=low&timeStamp=0
    http://localhost:8080/uploadBin?binID=2&binSize=22&bushel=2&crop=weed&moister=low&timeStamp=0
    
    modify existing field:
    http://localhost:8080/uploadBin?binID=1&binSize=44&bushel=22&crop=weed&moister=height&timeStamp=(......add something here)
"""

class sentBin(webapp2.RequestHandler):
    def get(self):
        response=[]
        bins_query=Bin.query()
        bins=bins_query.fetch()
        for bin in bins:
            response.append({'binID':bin.binID,'binSize':bin.binSize,'timeStamp':bin.timeStamp,'bushel':bin.bushel,'crop':bin.crop,'moister':bin.moister})
        self.response.write(json.dumps(response))

"""
    test url:
    get all existing fields:
    http://localhost:8080/downloadBin
    
    """

#-----------------------------------------------

class recieveField(webapp2.RequestHandler):
    def get(self):
        fields_query=Field.query()
        fields=fields_query.fetch()
        hasField=False
        response=[]
        
        #when there's data in server
        for field in fields:
            if field.farmName==self.request.get('farmName') and field.fieldName==self.request.get('fieldName'):
                hasField=True
                #when client data is newer
                if float(field.timeStamp)==float(self.request.get('timeStamp')):
                    field.acre=self.request.get('acre')
                    field.note=self.request.get('note')
                    field.timeStamp=str(time.time())
                    field.put();
                    response.append({'result':"success",'action':"update",'timeStamp':field.timeStamp})
                    self.response.write(json.dumps(response))
                    return
                #when client data is older
                if float(field.timeStamp)>float(self.request.get('timeStamp')):
                    response.append({'result':"fail",'action':"sent",'timeStamp':field.timeStamp})
                    response.append({'fieldName':field.fieldName,'farmName':field.farmName,'timeStamp':field.timeStamp,'acre':field.acre,'note':field.note})
                    self.response.write(json.dumps(response))
                    return
        #when there's no data in server
        if not hasField:
            field=Field()
            field.farmName=self.request.get('farmName')
            field.fieldName=self.request.get('fieldName')
            field.acre=self.request.get('acre')
            field.note=self.request.get('note')
            field.timeStamp=str(time.time())
            field.put();
            response.append({'result':"success",'action':"add",'timeStamp':field.timeStamp})
            self.response.write(json.dumps(response))


"""
test url:

add new field:
http://localhost:8080/uploadField?farmName=farm1&fieldName=field1&acre=99&note=hope it work&timeStamp=0
http://localhost:8080/uploadField?farmName=farm1&fieldName=field2&acre=99&note=hope it work&timeStamp=0
    
modify existing field:
http://localhost:8080/uploadField?farmName=farm1&fieldName=field1&acre=12&note=hope it work&timeStamp=0 (result data)
    
"""

class sentField(webapp2.RequestHandler):
    def get(self):
        response=[]
        fields_query=Field.query()
        fields=fields_query.fetch()
        for field in fields:
            response.append({'fieldName':field.fieldName,'farmName':field.farmName,'timeStamp':field.timeStamp,'acre':field.acre,'note':field.note})
        self.response.write(json.dumps(response))

"""
    test url:
get all existing fields:
http://localhost:8080/downloadField
"""

"""---------------------------------"""
class MainPage(webapp2.RequestHandler):
    def get(self):
        template_values={
            'invalidUser': False
        }
        template= JINJA_ENVIRONMENT.get_template('Login.html')
        self.response.write(template.render(template_values))


class CheckUser(webapp2.RequestHandler):
    def post(self):
        if isAdmin(self.request.get('userName'),hashlib.sha256(self.request.get('password')).hexdigest()):
            template_values={
                'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
                'users': User.query(User.isAdmin=="false").order(User.userName),
                'token': getToken()
            }
            template= JINJA_ENVIRONMENT.get_template('Operations.html')
            self.response.write(template.render(template_values))
        else:
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))

class DeleteUser(webapp2.RequestHandler):
    def post(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        success=False
        users_query=User.query(User.userName==self.request.get('theUser'))
        users=users_query.fetch()
        for user in users:
            user.key.delete()
            success=True
        template_values={
            'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
            'users': User.query(User.isAdmin=="false").order(User.userName),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Operations.html')
        self.response.write(template.render(template_values))

class AddUser(webapp2.RequestHandler):
    def post(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        hasUser=False
        newUserName=self.request.get('userName')
        newpassword=self.request.get('password')
        users_query=User.query()
        users=users_query.fetch()
        for user in users:
            if user.userName==newUserName:
                hasUser=True
        if not hasUser:
            user=User()
            user.userName=self.request.get('userName')
            user.password=hashlib.sha256(self.request.get('password')).hexdigest()
            
            if self.request.get('isAdmin') =="on":
                user.isAdmin="true"
            else:
                user.isAdmin="false"
            user.put()
        template_values={
            'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
            'users': User.query(User.isAdmin=="false").order(User.userName),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Operations.html')
        self.response.write(template.render(template_values))

class Logout(webapp2.RequestHandler):
    def get(self):
        validToken(self.request.get('token'))
        template_values={
            'invalidUser': False
        }
        template= JINJA_ENVIRONMENT.get_template('Login.html')
        self.response.write(template.render(template_values))

class HasUser(webapp2.RequestHandler):
    def get(self):
        hasUser=False
        response=[]
        users_query=User.query()
        users=users_query.fetch()
        for user in users:
            if user.userName == self.request.get('theUser') and user.password==self.request.get('password'):
                if(user.isAdmin=="true"):
                    response.append({'hasUser':1})
                    hasUser=True
                else:
                    response.append({'hasUser':0})
                    hasUser=True
                break
        if(not hasUser):
            response.append({'hasUser':-1})
        self.response.write(json.dumps(response))

class Report(webapp2.RequestHandler):
    def post(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        template_values={
            'bins': Bin.query().order(Bin.binID),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Report.html')
        self.response.write(template.render(template_values))

class Operations(webapp2.RequestHandler):
    def post(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return

        template_values={
            'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
            'users': User.query(User.isAdmin=="false").order(User.userName),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Operations.html')
        self.response.write(template.render(template_values))

application = webapp2.WSGIApplication([
                                       ('/', MainPage),
                                       ('/logout', Logout),
                                       ('/checkUser', CheckUser),
                                       ('/addUser',AddUser),
                                       ('/deleteUser',DeleteUser),
                                       ('/hasUser',HasUser),
                                       ('/uploadField',recieveField),
                                       ('/downloadField',sentField),
                                       ('/uploadBin',recieveBin),
                                       ('/downloadBin',sentBin),
                                       ('/uploadRainGuage',recieveRainGuage),
                                       ('/downloadRainGuage',sentRainGuage),
                                       ('/report',Report),
                                       ('/operations',Operations),
                                       ], debug=True)