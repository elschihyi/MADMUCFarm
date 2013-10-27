import cgi
import os
import urllib
import json
import unicodedata
import time

from google.appengine.ext import db
from google.appengine.api import users

import webapp2
import jinja2

JINJA_ENVIRONMENT = jinja2.Environment(
                                       loader=jinja2.FileSystemLoader(os.path.dirname(__file__)),
                                       extensions=['jinja2.ext.autoescape'])
timeStamp = time.time();


class MainPage(webapp2.RequestHandler):
    
    def get(self):
        farm_query = db.Query(Farm)
        
        self.response.write('<h2>Existing Farms</h2>')
        found = False
        
            
        for fam in farm_query:
            found = True
            self.response.write('Farm Name: <b>%s</b> </br>' % fam.farmName)
            for fiel in fam.fieldNames:
                self.response.write('-Field Name: %s </br>' % fiel)
            self.response.write('<img src="/image?name=%s" height="60" width="120"></br><br><br>' % fam.farmName)
            
    
        if not found:
            self.response.write('<p>There are no farms yet.</p>')

                                 
        template = JINJA_ENVIRONMENT.get_template('index.html')
        self.response.write(template.render())

class Farm(db.Model):
    farmName = db.StringProperty()
    farmImage = db.BlobProperty()
    fieldNames = db.ListProperty(str,verbose_name=None, default=None)

class ProcessFarm(webapp2.RequestHandler):
    
    def post(self):
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
            
        self.redirect('/')

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
            farms.append({'farmName':farm.farmName, 'farmImgUrl':"http://madmuctut1.appspot.com/image?name="+farm.farmName, 'fields': fields})
            
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
        global timeStamp
        deleteName = self.request.get('name')
        farm = Farm().all().filter("farmName =", deleteName).get()
        if(farm != None):
            farm.delete()
            timeStamp = time.time()


        self.redirect('/')



application = webapp2.WSGIApplication([
                                       ('/', MainPage),
                                       ('/save', ProcessFarm),
                                       ('/Harvest', ProcessHarvest),
                                       ('/HarvestDownload', ProcessHarvestDownload),
                                       ('/Cultivation', ProcessCultivation),
                                       ('/CultivationDownload', ProcessCultivationDownload),
                                       ('/SoilTest', ProcessSoilTest),
                                       ('/SoilTestDownload', ProcessSoilTestDownload),
                                       ('/data', GiveData),
                                       ('/image', GiveImage),
                                       ('/delete', DeleteImage)
                                       ], debug=True)