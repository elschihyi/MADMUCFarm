#!/usr/bin/env python
#
# Copyright 2007 Google Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
import logging
from google.appengine.ext import db
from datetime import datetime
import json
import webapp2
import time

SEEDTEMPLATE = """
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
		<form id="send" action="/saveseedtemplate">
		    <h3>Seed Template</h3>
		    <p>
		    <label for="name">Seed Template Name *</label>
		    <input id="name" type="text" name="name" value=""  class="input"/>
		    </p>
		    
		    <p>
		    <label for="seedtype">Seed Type *</label>
		    <input id="seedtype" type="text" name="seedtype" value=""  class="input"/>
		    </p>
		    
		    <p>
		    <label for="tool">Implemented Used *</label>
		    <input id="tool" type="text" name="tool" value="" class="input"/>
		    </p>
                
		    <p>
		    <label for="depth">Seeding Depth * (in)</label>
		    <input id="depth" type="text" name="depth" value="" class="input"/>
		    </p>

		    <p>
		    <label for="varietyname">Variety Name *</label>
		    <input id="varietyname" type="text" name="varietyname" value="" class="input"/>
		    </p>    
		    
		    <p>
		    <label for="seedrate"> Seed Rate (lbs/ac) *</label>
		    <input id="seedrate" type="text" name="seedrate" value="" class="input"/>
		    </p>
                
		    <p>
		    <label for="nh3">NH3 (lbs/ac)*</label>
		    <input id="nh3" type="text" name="nh3" value="" class="input"/>
		    </p>
                
		    <p>
		    <label for="11-52-20">11-52-20 (lbs/ac)*</label>
		    <input id="11-52-20" type="text" name="11-52-20" value="" class="input"/>
		    </p>
		    
		    <p>
		    <label for="seedtreatment"> Seed Treatment (lbs/ac)*</label>
		    <input id="seedtreatment" type="text" name="seedtreatment" value="" class="input"/>
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


SEEDTEMPLATEDETAIL = """
	<html>
	<head>
	<title>Template Detail</title>
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
	<body>
	    
	    <div>
		<form id="send" action="/seed">
		    <h3>Seed Template</h3>
		    <p>
		    <label for="name">Seed Template Name * :</label>
		    <input id="name" type="text" name="name" value="%s" readonly="readonly" class="input"/>
		    </p>
		    
		    <p>
		    <label for="seedtype">Seed Type *</label>
		    <input id="seedtype" type="text" name="seedtype" value="%s" readonly="readonly" class="input"/>
		    </p>
		    
		    <p>
		    <label for="tool">Implemented Used *</label>
		    <input id="tool" type="text" name="tool" value="%s" readonly="readonly" class="input"/>
		    </p>
                
		    <p>
		    <label for="depth">Seeding Depth * (in)</label>
		    <input id="depth" type="text" name="depth" value="%s" readonly="readonly" class="input"/>
		    </p>

                    <p>
		    <label for="varietyname">Variety Name *</label>
		    <input id="varietyname" type="text" name="varietyname" value="%s" readonly="readonly" class="input"/>
		    </p>
                
		    <p>
		    <label for="seedrate"> Seed Rate (lbs/ac) *</label>
		    <input id="seedrate" type="text" name="seedrate" value="%s" readonly="readonly" class="input"/>
		    </p>
                
		    
                
		    <p>
		    <label for="nh3">NH3 (lbs/ac)*</label>
		    <input id="nh3" type="text" name="nh3" value="%s"  readonly="readonly" class="input"/>
		    </p>
                
		    <p>
		    <label for="11-52-20">11-52-20 (lbs/ac)*</label>
		    <input id="11-52-20" type="text" name="11-52-20" value="%s"  readonly="readonly" class="input"/>
		    </p>

		    <p>
		    <label for="seedtreatment"> Seed Treatment (lbs/ac)*</label>
		    <input id="seedtreatment" type="text" name="seedtreatment" value="%s" readonly="readonly" class="input"/>
		    </p>
		    
		    <p>
		    <button id="submit" type="submit">Back</button>
		    </p>
		    
		    
		</form>
            </div>
	</body>
	
	</html>"""
	
SEEDTEMPLATEEDIT = """
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
		<form id="send" action="/saveseedtemplate">
		    <h3>Seed Template</h3>
		    <p>
		    <label for="name">Seed Template Name * :</label>
		    <input id="name" type="text" name="name" value="%s" readonly="readonly" class="input"/>
		    </p>
		    
		    <p>
		    <label for="seedtype">Seed Type *</label>
		    <input id="seedtype" type="text" name="seedtype" value="%s"  class="input"/>
		    </p>
		    
		    <p>
		    <label for="tool">Implemented Used *</label>
		    <input id="tool" type="text" name="tool" value="%s"  class="input"/>
		    </p>
                
		    <p>
		    <label for="depth">Seeding Depth * (in)</label>
		    <input id="depth" type="text" name="depth" value="%s"  class="input"/>
		    </p>

                    <p>
		    <label for="varietyname">Variety Name *</label>
		    <input id="varietyname" type="text" name="varietyname" value="%s"  class="input"/>
		    </p>
                
		    <p>
		    <label for="seedrate"> Seed Rate (lbs/ac) *</label>
		    <input id="seedrate" type="text" name="seedrate" value="%s"  class="input"/>
		    </p>
                
		    <p>
		    <label for="nh3">NH3 (lbs/ac)*</label>
		    <input id="nh3" type="text" name="nh3" value="%s"  class="input"/>
		    </p>
                
		    <p>
		    <label for="11-52-20">11-52-20 (lbs/ac)*</label>
		    <input id="11-52-20" type="text" name="11-52-20" value="%s"  class="input"/>
		    </p>

		    <p>
		    <label for="seedtreatment"> Seed Treatment (lbs/ac)*</label>
		    <input id="seedtreatment" type="text" name="seedtreatment" value="%s"  class="input"/>
		    </p>
		    
		    <p>
		    <button id="submit" type="submit">Update</button>
		    </p>              
            </div>
	</body>
	
	</html>"""

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

class MainHandler(webapp2.RequestHandler):
    def get(self):
        #main web interface for choosing seed or chemical
        self.response.write('<a href="/seed"> Seed Template</a><br/><a href="/chemical"> Chemical Template</a>')

class Seed(webapp2.RequestHandler):
    def get(self):
        #seed web page
	if  SeedTemplate.all().count() > 0 :   
	    seedTemplatesSet = SeedTemplate.all()
	    name = ""
	    body = ""
	    for seedTemplate in seedTemplatesSet:
		name = seedTemplate.templateName
		body += "<p><label>%s</lable><br/>" % (name)
		body += "<a class= 'link' href= '/editseedtemplate?edit=%s'>Edit</a>" %(name)
		body += "<a class = 'link' href= '/seedtemplatedetail?detail=%s'>Detail</a>" %(name)
		body += "<a class = 'link' href= '/deleteseedtemplate?delete=%s'>Delete</a>"%(name)
		
	    self.response.write(SEEDTEMPLATE % body)
	else :
	    self.response.write(SEEDTEMPLATE % "")
	
	

#create seed template
class SeedSave(webapp2.RequestHandler):
    def get(self):
        
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
	    self.response.write("<h4>Create Templates Successful<h4><br/><form action='/seed'><button type='submit'>OK</button></form>")
    
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
	    self.response.write("<h4>Edit Templates Successful<h4><br/><form action='/seed'><button type='submit'>OK</button></form>")

# seed template edit function
class SeedEdit(webapp2.RequestHandler):
    def get(self):
	templateName = self.request.get('edit')
	templateEntity = SeedTemplate.all().filter('templateName =',templateName)
	template = templateEntity.get()
	
	self.response.write(SEEDTEMPLATEEDIT % (template.templateName, template.seedType, template.tool,template.seedDepth,template.varietyName,template.seedRate,template.NH3,template._11_52_20,template.seedTreatment))

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
	templateName = self.request.get('delete')
	templateEntity = SeedTemplate.all().filter('templateName =',templateName)
	template = templateEntity.get()
	template.delete()
	self.response.write("<h4>Delete Templates Successful<h4><br/><form action='/seed'><button type='submit'>OK</button></form>")

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
	if ChemicalTemplate.all().count() > 0:
	    chemicalTemplatesSet = ChemicalTemplate.all()
	    name = ""
	    body = ""
	    for chemicalTemplate in chemicalTemplatesSet:
		name = chemicalTemplate.templateName
		body += "<p><label>%s</lable><br/>" % (name)
		body += "<a class= 'link' href= '/editchemicaltemplate?edit=%s'>Edit</a>" %(name)
		body += "<a class = 'link' href= '/chemicaltemplatedetail?detail=%s'>Detail</a>" %(name)
		body += "<a class = 'link' href= '/deletechemicaltemplate?delete=%s'>Delete</a>"%(name)
	    self.response.write(CHEMICALTEMPLATE % (body))
	else:
	    self.response.write(CHEMICALTEMPLATE % (""))

# chemical template create ui
class ChemicalSave(webapp2.RequestHandler):
    def get(self):
	templateName = self.request.get('name')
	count = ChemicalTemplate().all().filter('templateName =',templateName).count()
	
	if count == 0:
	    template = ChemicalTemplate()
	    template.templateName= self.request.get('name')
	    template.chemicalType = self.request.get('chemicalType')
	    template.tool = self.request.get('tool')
	    template.chemicalRate = self.request.get('chemicalRate')
	    template.put()
	    self.response.write("<h4>Create Templates Successful<h4><br/><form action='/chemical'><button type='submit'>OK</button></form>")
	else:
	    template = ChemicalTemplate().all().filter('templateName =',templateName).get()
	    template.templateName= self.request.get('name')
	    template.chemicalType = self.request.get('chemicalType')
	    template.tool = self.request.get('tool')
	    template.chemicalRate = self.request.get('chemicalRate')
	    template.put()
	    self.response.write("<h4>Edit Templates Successful<h4><br/><form action='/chemical'><button type='submit'>OK</button></form>")

# chemical template edit function
class ChemicalEdit(webapp2.RequestHandler):
    def get(self):
	templateName = self.request.get('edit')
	templateEntity = ChemicalTemplate.all().filter('templateName =',templateName)
	template = templateEntity.get()
	
	self.response.write(CHEMICALTEMPLATEEDIT % (template.templateName, template.chemicalType, template.tool,template.chemicalRate))

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
	templateName = self.request.get('delete')
	templateEntity = ChemicalTemplate.all().filter('templateName =',templateName)
	template = templateEntity.get()
	template.delete()
	self.response.write("<h4>Delete Templates Successful<h4><br/><form action='/chemical'><button type='submit'>OK</button></form>")

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
	print(rtnJson)    

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
    


app = webapp2.WSGIApplication([
    ('/', MainHandler),
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
    
], debug=True)

def main():
    logging.getLogger().setLevel(logging.DEBUG)
    webapp.util.run_wsgi_app(application)
    
if __name__ == '__main__':
    main()
