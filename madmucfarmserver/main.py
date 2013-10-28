import cgi
import os
import urllib
import json
import unicodedata
import time
import hashlib
import webapp2
import jinja2

from google.appengine.ext import db
from google.appengine.api import users

from uuid import uuid4
from google.appengine.ext import ndb
from datetime import datetime

JINJA_ENVIRONMENT = jinja2.Environment(
                                       loader=jinja2.FileSystemLoader(os.path.dirname(__file__)),
                                       extensions=['jinja2.ext.autoescape'])



CHEMICALTEMPLATE = """
	<html>
	<head>
	<title>Template Dashboard</title>
	</head>
	<style type = "text/css">
    .input
    {
    position: absolute;
    left: 170px;
    }
    
    .link
    {
    margin-left:5px;
    margin-right:5px;
    }
	</style>
	<script>
	
	</script>
	<body>
    <div>
    <form action = "/">
    <button type="submit"> Go Back Main Menu</button>
    </form>
    </div>
    <div>
    <form id="send" action="/savechemicaltemplate">
    <h3>Chemical Template</h3>
    <p>
    <label for="name">Template Name *</label>
    <input id="name" type="text" name="name" value=""  class="input"/>
    </p>
    
    <p>
    <label for="chemicalType">Chemical Type *</label>
    <input id="chemicalType" type="text" name="chemicalType" value=""  class="input"/>
    </p>
    
    <p>
    <label for="tool">Implemented Used *</label>
    <input id="tool" type="text" name="tool" value="" class="input"/>
    </p>
    
    <p>
    <label for="chemicalRate"> Chemical Rate (lbs/ac) *</label>
    <input id="chemicalRate" type="text" name="chemicalRate" value="" class="input"/>
    </p>
    
    <p>
    <button id="submit" type="submit" onclick="reloadPage()" = >Submit</button>
    </p>
    </form>
    </div>
    
    <hr>
    
    <div>
    <h3>Existing Templates</h3>
    %s
    </div>
	</body>
	
	</html>"""

CHEMICALTEMPLATEEDIT = """
	<html>
	<head>
	<title>Template Edit</title>
	</head>
	<style type = "text/css">
    .input
    {
    position: absolute;
    left: 170px;
    }
    .link
    {
    margin-left:5px;
    margin-right:5px;
    }
	</style>
	<script>
    
	</script>
	<body>
    <div>
    <form id="send" action="/savechemicaltemplate">
    <h3>Chemical Template</h3>
    <p>
    <label for="name">Template Name *</label>
    <input id="name" type="text" name="name" value="%s" readonly="readonly"  class="input"/>
    </p>
    
    <p>
    <label for="chemicalType">Chemical Type *</label>
    <input id="chemicalType" type="text" name="chemicalType" value="%s"  class="input"/>
    </p>
    
    <p>
    <label for="tool">Implemented Used *</label>
    <input id="tool" type="text" name="tool" value="%s" class="input"/>
    </p>
    
    <p>
    <label for="chemicalRate"> Chemical Rate (lbs/ac) *</label>
    <input id="chemicalRate" type="text" name="chemicalRate" value="%s" class="input"/>
    </p>
    
    <p>
    <button id="submit" type="submit">Update</button>
    </p>
    </form>
    
    </div>
	</body>
	
	</html>"""

CHEMICALTEMPLATEDETAIL = """
	<html>
	<head>
	<title>Template Edit</title>
	</head>
	<style type = "text/css">
    .input
    {
    position: absolute;
    left: 170px;
    }
    .link
    {
    margin-left:5px;
    margin-right:5px;
    }
	</style>
	<script>
    
	</script>
	<body>
    <div>
    <form id="send" action="/chemical">
    <h3>Chemical Template</h3>
    <p>
    <label for="name">Template Name *</label>
    <input id="name" type="text" name="name" value="%s" readonly="readonly"  class="input"/>
    </p>
    
    <p>
    <label for="chemicalType">Chemical Type *</label>
    <input id="chemicalType" type="text" name="chemicalType" value="%s"  readonly="readonly" class="input"/>
    </p>
    
    <p>
    <label for="tool">Implemented Used *</label>
    <input id="tool" type="text" name="tool" value="%s" readonly="readonly" class="input"/>
    </p>
    
    <p>
    <label for="chemicalRate"> Chemical Rate (lbs/ac) *</label>
    <input id="chemicalRate" type="text" name="chemicalRate" value="%s" readonly="readonly" class="input"/>
    </p>
    
    <p>
    <button id="submit" type="submit">Back</button>
    </p>
    </form>
    
    </div>
	</body>
	
	</html>"""

timeStamp = time.time();

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
        if float(token.timeStamp)+600<time.time():
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

class SeedObject(db.Model):
    seedType = db.StringProperty()
    tool = db.StringProperty()
    seedDepth = db.StringProperty()
    varietyName = db.StringProperty()
    seedRate = db.StringProperty()
    seedTreatment = db.StringProperty()
    NH3 = db.StringProperty()
    _11_52_20 = db.StringProperty()
    notes = db.StringProperty()
    seedDate = db.StringProperty()
    update = db.StringProperty()
    farmName = db.StringProperty()
    fieldName = db.StringProperty()
    templateIndex = db.StringProperty()

class SeedTemplate(db.Model):
    templateName = db.StringProperty()
    seedType = db.StringProperty()
    tool = db.StringProperty()
    seedDepth = db.StringProperty()
    varietyName = db.StringProperty()
    seedRate = db.StringProperty()
    seedTreatment = db.StringProperty()
    NH3 = db.StringProperty()
    _11_52_20 = db.StringProperty()

class ChemicalObject(db.Model):
    chemicalType = db.StringProperty()
    chemicalRate = db.StringProperty()
    tool = db.StringProperty()
    notes = db.StringProperty()
    chemicalDate = db.StringProperty()
    update = db.StringProperty()
    farmName = db.StringProperty()
    fieldName = db.StringProperty()
    serverId = db.StringProperty()


class ChemicalTemplate(db.Model):
    templateName = db.StringProperty()
    chemicalType = db.StringProperty()
    chemicalRate = db.StringProperty()
    tool = db.StringProperty()

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
                'farms':db.Query(Farm),
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
            'farms':db.Query(Farm),
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
            'farms':db.Query(Farm),
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

class BinReport(webapp2.RequestHandler):
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
        template= JINJA_ENVIRONMENT.get_template('BinReport.html')
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
            'farms':db.Query(Farm),
            'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
            'users': User.query(User.isAdmin=="false").order(User.userName),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Operations.html')
        self.response.write(template.render(template_values))

class Farm(db.Model):
    farmName = db.StringProperty()
    farmImage = db.BlobProperty()
    fieldNames = db.ListProperty(str,verbose_name=None, default=None)

class ProcessFarm(webapp2.RequestHandler):
    def post(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
        
        farm_query = db.Query(Farm)
        sameName = False
        global timeStamp
        
        for qFarm in farm_query:
            if (qFarm.farmName == self.request.get('farmName')):
                sameName = True
                break
        
        if (sameName != True):
            farm = Farm()
            farm.farmName = self.request.get('farmName')
            farm.farmImage = db.Blob(self.request.get('farmImg'))
            farm.fieldNames = self.request.get_all('fieldName')
            farm.put()
            timeStamp = time.time()
        
        template_values={
            'farms':db.Query(Farm),
            'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
            'users': User.query(User.isAdmin=="false").order(User.userName),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Operations.html')
        self.response.write(template.render(template_values))

class Harvest(db.Model):
    
    field = db.StringProperty()
    date = db.StringProperty()
    implement = db.StringProperty()
    cropYield = db.StringProperty()
    moisture = db.StringProperty()
    bin = db.StringProperty()
    notes = db.StringProperty()
    stamp = db.StringProperty()

class ProcessHarvest(webapp2.RequestHandler):
    
    def post(self):
        
        harvest_query2 = Harvest().all().filter("field =",self.request.get('field')).get()
        
        if(harvest_query2 == None):
            #No data on server, populate server
            harvest = Harvest()
            harvest.field = self.request.get('field')
            harvest.date = self.request.get('date')
            harvest.implement = self.request.get('implement')
            harvest.cropYield = self.request.get('cropYield')
            harvest.moisture = self.request.get('moisture')
            harvest.bin = self.request.get('bin')
            harvest.notes = self.request.get('notes')
            harvest.stamp = str(time.time())
            harvest.put()
            self.response.out.write(harvest.stamp)
        else:
            #Update server with new data, sync timestamps
            harvest_query2.field = self.request.get('field')
            harvest_query2.date = self.request.get('date')
            harvest_query2.implement = self.request.get('implement')
            harvest_query2.cropYield = self.request.get('cropYield')
            harvest_query2.moisture = self.request.get('moisture')
            harvest_query2.bin = self.request.get('bin')
            harvest_query2.notes = self.request.get('notes')
            harvest_query2.stamp = str(time.time())
            harvest_query2.put()
            self.response.out.write(harvest_query2.stamp)

class ProcessHarvestDownload(webapp2.RequestHandler):
    
    def post(self):
        harvest_query = db.Query(Harvest)
        harvests = json.loads(self.request.body)
        returnJson = []
        
        for harvest in harvests:
            harvest_query2 = Harvest().all().filter("field =",harvest['field']).get()
            if(harvest_query2 != None):
                if(float(harvest_query2.stamp) <= float(harvest['stamp'])):
                    harvest_query = harvest_query.filter("field !=",harvest['field'])
        
        for harv in harvest_query:
            returnJson.append({'field':harv.field,'date':harv.date,'implement':harv.implement,'cropYield':harv.cropYield,'moisture':harv.moisture,'bin':harv.bin,'notes':harv.notes,'stamp':harv.stamp})
        
        self.response.out.write(json.dumps(returnJson))





class Cultivation(db.Model):
    
    field = db.StringProperty()
    date = db.StringProperty()
    implement = db.StringProperty()
    depth = db.StringProperty()
    notes = db.StringProperty()
    stamp = db.StringProperty()

class ProcessCultivation(webapp2.RequestHandler):
    
    def post(self):
        
        cultivation_query2 = Cultivation().all().filter("field =",self.request.get('field')).get()
        
        if(cultivation_query2 == None):
            #No data on server, populate server
            cultivation = Cultivation()
            cultivation.field = self.request.get('field')
            cultivation.date = self.request.get('date')
            cultivation.implement = self.request.get('implement')
            cultivation.depth = self.request.get('depth')
            cultivation.notes = self.request.get('notes')
            cultivation.stamp = str(time.time())
            cultivation.put()
            self.response.out.write(cultivation.stamp)
        else:
            #Update server with new data, sync timestamps
            cultivation_query2.field = self.request.get('field')
            cultivation_query2.date = self.request.get('date')
            cultivation_query2.implement = self.request.get('implement')
            cultivation_query2.depth = self.request.get('depth')
            cultivation_query2.notes = self.request.get('notes')
            cultivation_query2.stamp = str(time.time())
            cultivation_query2.put()
            self.response.out.write(cultivation_query2.stamp)

class ProcessCultivationDownload(webapp2.RequestHandler):
    
    def post(self):
        cultivation_query = db.Query(Cultivation)
        cultivations = json.loads(self.request.body)
        returnJson = []
        
        for cultivation in cultivations:
            cultivation_query2 = Cultivation().all().filter("field =",cultivation['field']).get()
            if(cultivation_query2 != None):
                if(float(cultivation_query2.stamp) <= float(cultivation['stamp'])):
                    cultivation_query = cultivation_query.filter("field !=",cultivation['field'])
        
        for cult in cultivation_query:
            returnJson.append({'field':cult.field,'date':cult.date,'implement':cult.implement,'depth':cult.depth,'notes':cult.notes,'stamp':cult.stamp})
        
        self.response.out.write(json.dumps(returnJson))




class SoilTest(db.Model):
    
    field = db.StringProperty()
    notes = db.StringProperty()
    stamp = db.StringProperty()

class ProcessSoilTest(webapp2.RequestHandler):
    
    def post(self):
        
        soilTest_query2 = SoilTest().all().filter("field =",self.request.get('field')).get()
        
        if(soilTest_query2 == None):
            #No data on server, populate server
            soilTest = SoilTest()
            soilTest.field = self.request.get('field')
            soilTest.notes = self.request.get('notes')
            soilTest.stamp = str(time.time())
            soilTest.put()
            self.response.out.write(soilTest.stamp)
        else:
            #Update server with new data, sync timestamps
            soilTest_query2.field = self.request.get('field')
            soilTest_query2.notes = self.request.get('notes')
            soilTest_query2.stamp = str(time.time())
            soilTest_query2.put()
            self.response.out.write(soilTest_query2.stamp)

class ProcessSoilTestDownload(webapp2.RequestHandler):
    
    def post(self):
        soiltest_query = db.Query(SoilTest)
        soiltests = json.loads(self.request.body)
        returnJson = []
        
        for soiltest in soiltests:
            soiltest_query2 = SoilTest().all().filter("field =",soiltest['field']).get()
            if(soiltest_query2 != None):
                if(float(soiltest_query2.stamp) <= float(soiltest['stamp'])):
                    soiltest_query = soiltest_query.filter("field !=",soiltest['field'])
        
        for soil in soiltest_query:
            returnJson.append({'field':soil.field, 'notes':soil.notes,'stamp':soil.stamp})
        
        self.response.out.write(json.dumps(returnJson))

class GiveData(webapp2.RequestHandler):
    
    def get(self):
        farm_query = db.Query(Farm)
        farms = []
        
        farms.append({'time': timeStamp})
        for farm in farm_query:
            fields = []
            for field in farm.fieldNames:
                fields.append({'fieldName': field})
            farms.append({'farmName':farm.farmName, 'farmImgUrl':"http://madmucfarmserver.appspot.com/image?name="+farm.farmName, 'fields': fields})
        
        self.response.write(json.dumps(farms))


class GiveImage(webapp2.RequestHandler):
    
    def get(self):
        farm_query = db.Query(Farm)
        found = False
        self.response.headers['Content-Type'] = 'image/jpg'
        imageName = self.request.get('name')
        
        for farm in farm_query:
            if(farm.farmName == imageName):
                self.response.out.write(farm.farmImage)

class DeleteImage(webapp2.RequestHandler):
    def post(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        global timeStamp
        deleteName = self.request.get('name')
        farm = Farm().all().filter("farmName =", deleteName).get()
        if(farm != None):
            farm.delete()
            timeStamp = time.time()
        
        
        template_values={
            'farms':db.Query(Farm),
            'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
            'users': User.query(User.isAdmin=="false").order(User.userName),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Operations.html')
        self.response.write(template.render(template_values))




class Seed(webapp2.RequestHandler):
    def get(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
        token=getToken()
        #seed web page
        if  SeedTemplate.all().count() > 0 :
            seedTemplatesSet = SeedTemplate.all()
            name = ""
            body = ""
            
            
            for seedTemplate in seedTemplatesSet:
                name = seedTemplate.templateName
                body += "<p><label>%s</lable><br/>" % (name)
                body += "<a class= 'link' href= '/editseedtemplate?edit=%s&token=%s'>Edit</a>" %(name,token)
                body += "<a class = 'link' href= '/deleteseedtemplate?delete=%s&token=%s'>Delete</a>"%(name,token)
            template_values={
                'token': token,
                'body': body
            }
            template= JINJA_ENVIRONMENT.get_template('SEEDTEMPLATE.html')
            self.response.write(template.render(template_values))
        else :
            template_values={
                'token': token,
                'body': ""
            }
            template= JINJA_ENVIRONMENT.get_template('SEEDTEMPLATE.html')
            self.response.write(template.render(template_values))



#create seed template
class SeedSave(webapp2.RequestHandler):
    def get(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
        
        templateName = self.request.get('name')
        count = SeedTemplate().all().filter('templateName =',templateName).count()
        
        if count == 0 :
            template = SeedTemplate()
            template.templateName= self.request.get('name')
            template.seedType = self.request.get('seedtype')
            template.tool = self.request.get('tool')
            template.seedDepth = self.request.get('depth')
            template.varietyName = self.request.get('varietyname')
            template.seedRate = self.request.get('seedrate')
            template.seedTreatment = self.request.get('seedtreatment')
            template.NH3 = self.request.get('nh3')
            template._11_52_20 = self.request.get('11-52-20')
            template.put()
            self.response.write("<h4>Create Templates Successful<h4><br/><form action='/seed?'><input type='hidden' name='token' value='"+getToken()+"'><button type='submit'>OK</button></form>")
        
        else:
            template = SeedTemplate().all().filter('templateName =',templateName).get()
            template.templateName= self.request.get('name')
            template.seedType = self.request.get('seedtype')
            template.tool = self.request.get('tool')
            template.seedDepth = self.request.get('depth')
            template.varietyName = self.request.get('varietyname')
            template.seedRate = self.request.get('seedrate')
            template.seedTreatment = self.request.get('seedtreatment')
            template.NH3 = self.request.get('nh3')
            template._11_52_20 = self.request.get('11-52-20')
            template.put()
            self.response.write("<h4>Edit Templates Successful<h4><br/><form action='/seed?'><input type='hidden' name='token' value='"+getToken()+"'><button type='submit'>OK</button></form>")

# seed template edit function
class SeedEdit(webapp2.RequestHandler):
    def get(self):
        templateName = self.request.get('edit')
        templateEntity = SeedTemplate.all().filter('templateName =',templateName)
        template = templateEntity.get()

        template_values={
            'token': getToken(),
            'templateName':template.templateName,
            'seedType':template.seedType,
            'tool':template.tool,
            'seedDepth':template.seedDepth,
            'varietyName':template.varietyName,
            'seedRate':template.seedRate,
            'NH3':template.NH3,
            '_11_52_20':template._11_52_20,
            'seedTreatment':template.seedTreatment,
        }
        template= JINJA_ENVIRONMENT.get_template('SEEDTEMPLATEEDIT.html')
        self.response.write(template.render(template_values))
        

# seed template detail function
class SeedDetail(webapp2.RequestHandler):
    def get(self):
        templateName = self.request.get('detail')
        templateEntity = SeedTemplate.all().filter('templateName =',templateName)
        template = templateEntity.get()
        
        self.response.write(SEEDTEMPLATEDETAIL % (template.templateName, template.seedType, template.tool,template.seedDepth,template.varietyName,template.seedRate,template.NH3,template._11_52_20,template.seedTreatment))

# seed template delete funtion
class SeedTemplateDelete(webapp2.RequestHandler):
    def get(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        templateName = self.request.get('delete')
        templateEntity = SeedTemplate.all().filter('templateName =',templateName)
        template = templateEntity.get()
        template.delete()
        self.response.write("<h4>Delete Templates Successful<h4><br/><form action='/seed'><input type='hidden' name='token' value='"+getToken()+"'><button type='submit'>OK</button></form>")

# seed template download template
class DownloadSeedTemplate(webapp2.RequestHandler):
    def get(self):
        seedtemplates = SeedTemplate.all()
        json_dic = []
        for template in seedtemplates:
            json_dic.append(db.to_dict(template))
	    
        jsonRTN = json.dumps(json_dic)
        
        self.response.headers['Access-Controll-Allow-Origin']='*'
        self.response.headers['Content-Type'] = 'application-json'
        self.response.write(jsonRTN)

# main web interface for chemical
class Chemical (webapp2.RequestHandler):
    def get (self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        token=getToken()
        
        if ChemicalTemplate.all().count() > 0:
            chemicalTemplatesSet = ChemicalTemplate.all()
            name = ""
            body = ""
            for chemicalTemplate in chemicalTemplatesSet:
                name = chemicalTemplate.templateName
                body += "<p><label>%s</lable><br/>" % (name)
                body += "<a class= 'link' href= '/editchemicaltemplate?edit=%s&token=%s'>Edit</a>" %(name,token)
                body += "<a class = 'link' href= '/deletechemicaltemplate?delete=%s&token=%s'>Delete</a>"%(name,token)

            template_values={
                'token': token,
                'body': body
            }
            template= JINJA_ENVIRONMENT.get_template('CHEMICALTEMPLATE.html')
            self.response.write(template.render(template_values))

        else:
            template_values={
                'token': token,
                'body': ""
            }
            template= JINJA_ENVIRONMENT.get_template('CHEMICALTEMPLATE.html')
            self.response.write(template.render(template_values))


# chemical template create ui
class ChemicalSave(webapp2.RequestHandler):
    def get(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        templateName = self.request.get('name')
        count = ChemicalTemplate().all().filter('templateName =',templateName).count()
        
        if count == 0:
            template = ChemicalTemplate()
            template.templateName= self.request.get('name')
            template.chemicalType = self.request.get('chemicalType')
            template.tool = self.request.get('tool')
            template.chemicalRate = self.request.get('chemicalRate')
            template.put()
            self.response.write("<h4>Create Templates Successful<h4><br/><form action='/chemical'><input type='hidden' name='token' value='"+getToken()+"'><button type='submit'>OK</button></form>")
        else:
            template = ChemicalTemplate().all().filter('templateName =',templateName).get()
            template.templateName= self.request.get('name')
            template.chemicalType = self.request.get('chemicalType')
            template.tool = self.request.get('tool')
            template.chemicalRate = self.request.get('chemicalRate')
            template.put()
            self.response.write("<h4>Edit Templates Successful<h4><br/><form action='/chemical'><input type='hidden' name='token' value='"+getToken()+"'><button type='submit'>OK</button></form>")

# chemical template edit function
class ChemicalEdit(webapp2.RequestHandler):
    def get(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
            
        templateName = self.request.get('edit')
        templateEntity = ChemicalTemplate.all().filter('templateName =',templateName)
        template = templateEntity.get()
        
        template_values={
            'token': getToken(),
            'templateName':template.templateName,
            'chemicalType':template.chemicalType,
            'tool':template.tool,
            'chemicalRate':template.chemicalRate
        }
        template= JINJA_ENVIRONMENT.get_template('CHEMICALTEMPLATEEDIT.html')
        self.response.write(template.render(template_values))

# chemical template detail function
class ChemicalDetail(webapp2.RequestHandler):
    def get(self):
        templateName = self.request.get('detail')
        templateEntity = ChemicalTemplate.all().filter('templateName =',templateName)
        template = templateEntity.get()
        
        self.response.write(CHEMICALTEMPLATEDETAIL % (template.templateName, template.chemicalType, template.tool,template.chemicalRate))

# chemical template detelte function
class ChemicalTemplateDelete(webapp2.RequestHandler):
    def get(self):
        if (not validToken(self.request.get('token'))):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
    
        templateName = self.request.get('delete')
        templateEntity = ChemicalTemplate.all().filter('templateName =',templateName)
        template = templateEntity.get()
        template.delete()
        self.response.write("<h4>Delete Templates Successful<h4><br/><form action='/chemical'><input type='hidden' name='token' value='"+getToken()+"'><button type='submit'>OK</button></form>")

# chemcial template download uri
class DownloadChemicalTemplate(webapp2.RequestHandler):
    def get(self):
        chemicaltemplates = ChemicalTemplate.all()
        json_dic = []
        for template in chemicaltemplates:
            json_dic.append(db.to_dict(template))
	    
        jsonRTN = json.dumps(json_dic)
        
        self.response.headers['Access-Controll-Allow-Origin']='*'
        self.response.headers['Content-Type'] = 'application-json'
        self.response.write(jsonRTN)


# get seed object from mobile client if existing, update information
class UploadSeed(webapp2.RequestHandler):
    def post(self):
        
        body = self.request.body
        uploadSeed = json.loads(body)
        
        rtnList = []
        
        seedDB = SeedObject().all().filter('farmName =',uploadSeed['farmName']).filter('fieldName =', uploadSeed['fieldName']).get()
        
        if seedDB is None:
            seed = SeedObject()
            seed.NH3 = str(uploadSeed['NH3'])
            seed.notes = str(uploadSeed['notes'])
            seed.seedDate =str(uploadSeed['seedDate'])
            seed.seedDepth = str(uploadSeed['seedDepth'])
            seed.seedRate = str(uploadSeed['seedRate'])
            seed.tool = str(uploadSeed['implementedUsed'])
            seed.seedTreatment = str(uploadSeed['seedTreatment'])
            seed.varietyName = str(uploadSeed['varietyName'])
            seed._11_52_20 =str(uploadSeed['_11'])
            seed.farmName = str(uploadSeed['farmName'])
            seed.fieldName = str(uploadSeed['fieldName'])
            seed.seedType = str(uploadSeed['seedTypes'])
            seed.templateIndex = str(uploadSeed['templateIndex'])
            seed.update = str(time.time())
            seed.put()
            rtnList.append(seed.update)
	    
	    
        else:
            seedDB.NH3 = str(uploadSeed['NH3'])
            seedDB.notes = str(uploadSeed['notes'])
            seedDB.seedDate =str(uploadSeed['seedDate'])
            seedDB.seedDepth = str(uploadSeed['seedDepth'])
            seedDB.seedRate = str(uploadSeed['seedRate'])
            seedDB.tool = str(uploadSeed['implementedUsed'])
            seedDB.seedTreatment = str(uploadSeed['seedTreatment'])
            seedDB.varietyName = str(uploadSeed['varietyName'])
            seedDB._11_52_20 =str(uploadSeed['_11'])
            seedDB.farmName = str(uploadSeed['farmName'])
            seedDB.fieldName = str(uploadSeed['fieldName'])
            seedDB.seedType = str(uploadSeed['seedTypes'])
            seedDB.templateIndex = str(uploadSeed['templateIndex'])
            seedDB.update = str(time.time())
            seedDB.put()
            rtnList.append(seedDB.update)
	    
        
        rtnJson = json.dumps(rtnList)
            
        self.response.write(rtnJson)

# get chemical object from mobile client if existing, update information

class UploadChemical(webapp2.RequestHandler):
    def post(self):
        body = self.request.body
        uploadChemical = json.loads(body)
        
        rtnList = []
        chemicalDB = ChemicalObject().all().filter('farmName =',uploadChemical['farmName']).filter('fieldName =', uploadChemical['fieldName']).filter('serverId =',uploadChemical['serverId']).get()
        
        if chemicalDB is None:
            print('upload not find')
            chemical = ChemicalObject()
            chemical.chemicalDate = str(uploadChemical['chemicalDate'])
            chemical.chemicalRate =str(uploadChemical['chemicalRates'])
            chemical.chemicalType = str(uploadChemical['chemicalTypes'])
            chemical.farmName = str(uploadChemical['farmName'])
            chemical.fieldName = str(uploadChemical['fieldName'])
            chemical.tool = str(uploadChemical['implementedUsed'])
            chemical.notes = str(uploadChemical['note'])
            chemical.update = str(time.time())
            chemical.put()
            chemical.serverId = str(chemical.key().id())
            chemical.put()
            rtnList.append(chemical.serverId)
            rtnList.append(chemical.update)
        
        else:
            print('upload  find')
            chemicalDB.chemicalDate = str(uploadChemical['chemicalDate'])
            chemicalDB.chemicalRate = str(uploadChemical['chemicalRates'])
            chemicalDB.chemicalType = str(uploadChemical['chemicalTypes'])
            chemicalDB.farmName = str(uploadChemical['farmName'])
            chemicalDB.fieldName = str(uploadChemical['fieldName'])
            chemicalDB.tool = str(uploadChemical['implementedUsed'])
            chemicalDB.notes = str(uploadChemical['note'])
            chemicalDB.update = str(time.time())
            chemicalDB.put()
            rtnList.append(chemicalDB.serverId)
            rtnList.append(chemicalDB.update)
        
        rtnJson = json.dumps(rtnList)
        print(rtnJson)
        self.response.write(rtnJson)

# synchronize seed object
# get timestamps from clients and send update information back
class DownloadSeed (webapp2.RequestHandler):
    def post(self):
        body = self.request.body
        checkList = json.loads(body)
        rtnSeed = []
        seedAllList=[]
        
        
        
        #update client local storage
        for seed in checkList:
            s = SeedObject().all().filter('farmName =',seed['farmName']).filter('fieldName =',seed['fieldName']).get()
            if s is not None:
                sUpdate = float(s.update)
                seedUpdate = float(seed['update'])
                if sUpdate > seedUpdate:
                    rtnSeed.append(s)
        preJson = []
        for seed in rtnSeed:
            preJson.append(db.to_dict(seed))
	    
	    
        #get all
        seedAll = SeedObject().all()
        for seed in seedAll:
            seedAllList.append(db.to_dict(seed))
        
        # delete all incoming seeds
        for item in checkList:
            for i in seedAllList:
                if item['farmName'] == i['farmName'] and item['fieldName'] == i['fieldName']:
                    seedAllList.remove(i)
        
        # add update seed
        for seed in preJson:
            seedAllList.append(seed)
        
        jsonRTN = json.dumps(seedAllList)
        self.response.headers['Access-Controll-Allow-Origin']='*'
        self.response.headers['Content-Type'] = 'application-json'
        self.response.write(jsonRTN)

# synchronize chemical object
# get timestamps from clients and send update information back

class DownloadChemical(webapp2.RequestHandler):
    def post(self):
        body = self.request.body
        checkList = json.loads(body)
        rtnChemical = []
        chemicalAllList = []
        
        
        
        #update client local storage chemical
        for chemical in checkList:
            c = ChemicalObject().all().filter('farmName =',chemical['farmName']).filter('fieldName =',chemical['fieldName']).filter('serverId =',chemical['chemicalServerId']).get()    
            if c is not None:
                cUpdate = float(c.update)
                chemicalUpdate = float(chemical['update'])
                if cUpdate > chemicalUpdate:
                    rtnChemical.append(c)		    
        preJson = []
        for chemical in rtnChemical:
            preJson.append(db.to_dict(chemical))
        
        #add all chemicals
        chemicalAll = ChemicalObject().all()
        for chemical in chemicalAll:
            chemicalAllList.append(db.to_dict(chemical))
        
        
        #filter
        for item in checkList:
            for i in chemicalAllList:
                if item['chemicalServerId'] == i['serverId']:
                    chemicalAllList.remove(i)
                if i['serverId'] is None:
                    chemicalAllList.remove(i)
        
        for chemical in preJson:
            chemicalAllList.append(chemical)
        
        jsonRTN = json.dumps(chemicalAllList)
        
        print("download")
        print(jsonRTN)
        self.response.headers['Access-Controll-Allow-Origin']='*'
        self.response.headers['Content-Type'] = 'application-json'
        self.response.write(jsonRTN)

class DownloadAllSeed(webapp2.RequestHandler):
    def get(self):
        print ("download all seed")
        seedAll = SeedObject().all()
        seedAllList = []
        for seed in seedAll:
            seedAllList.append(db.to_dict(seed))
	    
        jsonRTN = json.dumps(seedAllList)	
        self.response.headers['Access-Controll-Allow-Origin']='*'
        self.response.headers['Content-Type'] = 'application-json'
        self.response.write(jsonRTN)

class DownloadAllChemical(webapp2.RequestHandler):
    def get(self):
        
        print ("download all seed")
        chemicalAllList = []
        chemicalAll = ChemicalObject().all()
        for chemical in chemicalAll:
            chemicalAllList.append(db.to_dict(chemical))
        
        jsonRTN = json.dumps(chemicalAllList)
        self.response.headers['Access-Controll-Allow-Origin']='*'
        self.response.headers['Content-Type'] = 'application-json'
        self.response.write(jsonRTN)

class Reset(webapp2.RequestHandler):
    def post(self):
        if not validToken(self.request.get('token')) or not isAdmin(self.request.get('userName'),hashlib.sha256(self.request.get('password')).hexdigest()):
            template_values={
                'invalidUser': True
            }
            template= JINJA_ENVIRONMENT.get_template('Login.html')
            self.response.write(template.render(template_values))
            return
        
        global timeStamp
        timeStamp=time.time()
        
        rainGuages=RainGuage.query()
        for rainGuage in rainGuages:
            rainGuage.key.delete()
        
        bins=Bin.query()
        for bin in bins:
            bin.key.delete()

        fields=Field.query()
        for field in fields:
            field.note=""
            field.put()

# delete all seeds
        db.delete(SeedObject.all())

# delete all chemicals
        db.delete(ChemicalObject.all())

# delete all harvests/cultivatin/soiltests
        db.delete(Harvest.all())
        db.delete(Cultivation.all())
        db.delete(SoilTest.all())

        template_values={
            'farms':db.Query(Farm),
            'adminUsers': User.query(User.isAdmin=="true").order(User.userName),
            'users': User.query(User.isAdmin=="false").order(User.userName),
            'token': getToken()
        }
        template= JINJA_ENVIRONMENT.get_template('Operations.html')
        self.response.write(template.render(template_values))

application = webapp2.WSGIApplication([
                                       ('/', MainPage),
                                       ('/Harvest', ProcessHarvest),
                                       ('/HarvestDownload', ProcessHarvestDownload),
                                       ('/Cultivation', ProcessCultivation),
                                       ('/CultivationDownload', ProcessCultivationDownload),
                                       ('/SoilTest', ProcessSoilTest),
                                       ('/SoilTestDownload', ProcessSoilTestDownload),
                                       ('/data', GiveData),
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
                                       ('/binreport',BinReport),
                                       ('/operations',Operations),
                                       ('/ProcessFarm',ProcessFarm),
                                       ('/delete', DeleteImage),
                                       ('/image', GiveImage),
                                       ('/seed',Seed),
                                       ('/saveseedtemplate',SeedSave),
                                       ('/editseedtemplate',SeedEdit),
                                       ('/seedtemplatedetail',SeedDetail),
                                       ('/deleteseedtemplate',SeedTemplateDelete),
                                       ('/downloadseedtemplate',DownloadSeedTemplate),
                                       ('/chemical',Chemical),
                                       ('/savechemicaltemplate',ChemicalSave),
                                       ('/editchemicaltemplate',ChemicalEdit),
                                       ('/chemicaltemplatedetail',ChemicalDetail),
                                       ('/deletechemicaltemplate',ChemicalTemplateDelete),
                                       ('/downloadchemicaltemplate',DownloadChemicalTemplate),
                                       ('/uploadseed',UploadSeed),
                                       ('/downloadseed',DownloadSeed),
                                       ('/uploadchemical',UploadChemical),
                                       ('/downloadchemical',DownloadChemical),
                                       ('/downloadallseed',DownloadAllSeed),
                                       ('/downloadallchemical',DownloadAllChemical),
                                       ('/reset',Reset),
                                       ], debug=True)
