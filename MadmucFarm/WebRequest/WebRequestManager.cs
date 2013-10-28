using System;
using System.Net;
using System.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MadmucFarm
{
	public class WebRequestManager
	{
		private static WebRequestManager webRequestManager;

		private WebRequestManager ()
		{
		}

		public static WebRequestManager getWebRequestManager()
		{
			if (webRequestManager == null) {

				webRequestManager = new WebRequestManager ();
			}

			return webRequestManager;
		}

		public void downloadSeedTemplate()
		{
			LocalStorage localdb = LocalStorage.getLocalStorageManager ();
			localdb.createTable ();
			localdb.cleanTemplates (1);

			using(WebClient webClient = new WebClient())
			{
				var url = "http://madmucfarmserver.appspot.com/downloadseedtemplate";
				//var url = "http://localhost:11080/downloadseedtemplate";
			

				var content = webClient.DownloadString (url);
				var contentJson = JsonObject.Parse (content);
				foreach (var templates in contentJson)
				{
					SeedTemplate st = new SeedTemplate ();
					st.implementedUsed = ((JsonValue)templates)["tool"];

					st.NH3 =  ((JsonValue)templates)["NH3"];
					st.seedDepth =((JsonValue)templates)["seedDepth"];
					st.seedRate= ((JsonValue)templates)["seedRate"];
					st.seedTreatment = ((JsonValue)templates)["seedTreatment"];
					st.seedTypes = ((JsonValue)templates)["seedType"];
					st.templateName = ((JsonValue)templates)["templateName"];
					st._11 = ((JsonValue)templates)["_11_52_20"];
					st.varietyName = ((JsonValue)templates)["varietyName"];

					localdb.getLocalDB ().Insert (st);

				}

			}
		}

		public void downloadChemicalTemplate()
		{
			LocalStorage localdb = LocalStorage.getLocalStorageManager ();
			localdb.createTable ();
			localdb.cleanTemplates (2);
			using(WebClient webClient = new WebClient())
			{
				var url = "http://madmucfarmserver.appspot.com/downloadchemicaltemplate";
				//var url = "http://localhost:11080/downloadchemicaltemplate";
				var content = webClient.DownloadString (url);
				var contentJson = JsonObject.Parse (content);
				foreach (var templates in contentJson)
				{
					ChemicalTemplate ct = new ChemicalTemplate ();
					ct.implementedUsed = ((JsonValue)templates)["tool"];

					ct.chemicalRates =  ((JsonValue)templates)["chemicalRate"];
					ct.chemicalTypes =((JsonValue)templates)["chemicalType"];
					ct.templateName= ((JsonValue)templates)["templateName"];

					localdb.getLocalDB ().Insert (ct);

				}

			}

		}



		public List<string> uploadObject(Object obj){
			var jsonSeed = Newtonsoft.Json.JsonConvert.SerializeObject (obj);

			Console.Out.WriteLine (jsonSeed);

			var url = "";

			if(obj is SeedDB){
				url = "http://madmucfarmserver.appspot.com/uploadseed";
				//url = "http://localhost:11080/uploadseed";
			}

			if (obj is ChemicalDB){
				url = "http://madmucfarmserver.appspot.com/uploadchemical";
				//url = "http://localhost:11080/uploadchemical";
			}
			Console.Out.WriteLine ("url : {0}", url);
			using (WebClient webclient = new WebClient()) {
				webclient.Headers ["Content-type"] = "application/json";
				webclient.Encoding = Encoding.UTF8;
				var response = webclient.UploadString (url, jsonSeed);
				var content = JsonObject.Parse (response);

				List<string> rtn = new List<string> ();

				foreach (var item in content) {
				
					rtn.Add ((string)((JsonValue)item));
				}

				return rtn;
			}
		}
		public List<SeedDB> downloadAllSeed (){
			var unsyncSeedList = new List<SeedDB> ();

			using (WebClient webclient = new WebClient()) {
				var url = "http://madmucfarmserver.appspot.com/downloadallseed";

				webclient.Headers ["Content-type"] = "application/json";
				webclient.Encoding = Encoding.UTF8;

				var content = webclient.DownloadString (url);
				var jsonContent = JsonObject.Parse (content);

				foreach (var seed in jsonContent) {

					SeedDB s = new SeedDB ();
					s.farmName = ((JsonValue)seed) ["farmName"];
					s.fieldName = ((JsonValue)seed) ["fieldName"];
					s.implementedUsed = ((JsonValue)seed) ["tool"];
					s.lastUpdate = ((JsonValue)seed) ["update"];
					s.NH3 = ((JsonValue)seed) ["NH3"];
					s.notes = ((JsonValue)seed) ["notes"];
					s.seedDate = ((JsonValue)seed) ["seedDate"];
					s.seedDepth = ((JsonValue)seed) ["seedDepth"];
					s.seedRate = ((JsonValue)seed) ["seedRate"];
					s.seedTreatment = ((JsonValue)seed) ["seedTreatment"];
					s.seedTypes = ((JsonValue)seed) ["seedType"];
					s.templateIndex = ((JsonValue)seed) ["templateIndex"];
					s.varietyName = ((JsonValue)seed) ["varietyName"];
					s._11 = ((JsonValue)seed) ["_11_52_20"];

					unsyncSeedList.Add (s);
				}

			}
			return unsyncSeedList;
		}

		public List<ChemicalDB> downloadAllChemical (){

			var unsyncChemicalList = new List<ChemicalDB> ();

			using (WebClient webclient = new WebClient()) {
				var url = "http://madmucfarmserver.appspot.com/downloadallchemical";

				//url = "http://localhost:11080/downloadchemical";


				webclient.Headers ["Content-type"] = "application/json";
				webclient.Encoding = Encoding.UTF8;

				var content = webclient.DownloadString (url);
				var jsonContent = JsonObject.Parse (content);

				foreach (var chemical in jsonContent) {

					ChemicalDB c = new ChemicalDB ();
					c.farmName = ((JsonValue)chemical) ["farmName"];
					c.fieldName = ((JsonValue)chemical) ["fieldName"];
					c.implementedUsed = ((JsonValue)chemical) ["tool"];
					c.lastUpdate = ((JsonValue)chemical) ["update"];
					c.note = ((JsonValue)chemical) ["notes"];
					c.chemicalDate = ((JsonValue)chemical) ["chemicalDate"];
					c.chemicalRates = ((JsonValue)chemical) ["chemicalRate"];
					c.chemicalTypes = ((JsonValue)chemical) ["chemicalType"];
					c.isSynch = true;
					c.serverId = ((JsonValue)chemical) ["serverId"];
					unsyncChemicalList.Add (c);
				}

			}

			return unsyncChemicalList;
		}
		public List<Object> downloadObject(List<SynchCheck> checkList){

			List<Object> unsyncList = new List<Object> ();
			var jsonCheckList = Newtonsoft.Json.JsonConvert.SerializeObject (checkList);
			Console.Out.WriteLine (jsonCheckList);

			if (checkList.Count > 0) {
				if (checkList [0].type == "seed") {
					using (WebClient webclient = new WebClient()) {
						var url = "http://madmucfarmserver.appspot.com/downloadseed";

						//url = "http://localhost:11080/downloadseed";

						webclient.Headers ["Content-type"] = "application/json";
						webclient.Encoding = Encoding.UTF8;
						var response = webclient.UploadString (url, jsonCheckList);
						var jsonContent = JsonValue.Parse (response);

						foreach (var seed in jsonContent) {

							SeedDB s = new SeedDB ();
							s.farmName = ((JsonValue)seed) ["farmName"];
							s.fieldName = ((JsonValue)seed) ["fieldName"];
							s.implementedUsed = ((JsonValue)seed) ["tool"];
							s.lastUpdate = ((JsonValue)seed) ["update"];
							s.NH3 = ((JsonValue)seed) ["NH3"];
							s.notes = ((JsonValue)seed) ["notes"];
							s.seedDate = ((JsonValue)seed) ["seedDate"];
							s.seedDepth = ((JsonValue)seed) ["seedDepth"];
							s.seedRate = ((JsonValue)seed) ["seedRate"];
							s.seedTreatment = ((JsonValue)seed) ["seedTreatment"];
							s.seedTypes = ((JsonValue)seed) ["seedType"];
							s.templateIndex = ((JsonValue)seed) ["templateIndex"];
							s.varietyName = ((JsonValue)seed) ["varietyName"];
							s._11 = ((JsonValue)seed) ["_11_52_20"];

							unsyncList.Add (s);
						}
						Console.Out.WriteLine ("Seed Need Update : {0}", response);

						return unsyncList;
					}
				}

				if (checkList [0].type == "chemical") {

					using (WebClient webclient = new WebClient()) {
						var url = "http://madmucfarmserver.appspot.com/downloadchemical";

						//url = "http://localhost:11080/downloadchemical";


						webclient.Headers ["Content-type"] = "application/json";
						webclient.Encoding = Encoding.UTF8;
						var response = webclient.UploadString (url, jsonCheckList);
						var jsonContent = JsonValue.Parse (response);

						foreach (var chemical in jsonContent) {

							ChemicalDB c = new ChemicalDB ();
							c.farmName = ((JsonValue)chemical) ["farmName"];
							c.fieldName = ((JsonValue)chemical) ["fieldName"];
							c.implementedUsed = ((JsonValue)chemical) ["tool"];
							c.lastUpdate = ((JsonValue)chemical) ["update"];
							c.note = ((JsonValue)chemical) ["notes"];
							c.chemicalDate = ((JsonValue)chemical) ["chemicalDate"];
							c.chemicalRates = ((JsonValue)chemical) ["chemicalRate"];
							c.chemicalTypes = ((JsonValue)chemical) ["chemicalType"];
							c.isSynch = true;
							c.serverId = ((JsonValue)chemical) ["serverId"];
							unsyncList.Add (c);
						}
						Console.Out.WriteLine ("Chemical Need Update : {0}", response);

						return unsyncList;
					}
				}
			}

			return null;

		}
	}
}
























