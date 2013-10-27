using System;
using System.Net;
using System.Json;
using System.IO;
using MonoTouch.UIKit;
using System.Security.Cryptography;
using System.Text;

namespace MadmucFarm
{
	public class Synchronization
	{
		public static string url="http://madmucfarmserver.appspot.com/";
		//public static string url="localhost:8080";
		public Synchronization ()
		{
		}

		public bool syncRainGuage(){
			bool uploadSuccess=true;
			using (WebClient webClient = new WebClient()) {
				//0. check if it's user
				var isUser=hasUser ();
				if (isUser == -1)
					return false;

				//1. upload the rainGuage frirst
				var rainGuages = DBConnection.getAllRainGuage();
				foreach (RainGauge theRainGuage in rainGuages) {
					// 1.1 upload all rainGuages that has flag
					if (theRainGuage.flag == 1) {
						var operation = "/uploadRainGuage";
						var args = "?";

						args += "farmName=" + DBConnection.getFarmName(theRainGuage.farmID) + "&";
						args += "theDate=" + theRainGuage.theDate.ToShortDateString() + "&";
						args += "rain=" + theRainGuage.rain + "&";
						args += "timeStamp=" + theRainGuage.timeStamp;

						Console.WriteLine ("" + url + operation + args);

						var content = webClient.DownloadString (url + operation + args);
						DBConnection.updateRainGuageFlag (theRainGuage.rainGuageID, 0);
						var result = JsonObject.Parse (content);


						//1.1.1 server version is newer than our old data download data from server and disgard what we had changed
						if (((JsonValue)result [0]) ["result"] == "fail") {
							Console.WriteLine ("fail " + content);

							var totalRain=DBConnection.getRain(theRainGuage.farmID);
							var thisRain=DBConnection.getRainGuage (theRainGuage.rainGuageID);
							DBConnection.updateRainGuageRain (theRainGuage.rainGuageID, Convert.ToInt32(""+((JsonValue)result [1]) ["rain"]));
							DBConnection.setRain (theRainGuage.farmID, totalRain - thisRain + Convert.ToInt32 ("" + ((JsonValue)result [1]) ["rain"]));
							DBConnection.updateRainGuageTimeStamp (theRainGuage.rainGuageID, "" + ((JsonValue)result [0]) ["timeStamp"]);

							var tRainGuages = DBConnection.getAllRainGuage();
							foreach (RainGauge tRainGuage in tRainGuages)
								if (tRainGuage.rainGuageID == theRainGuage.rainGuageID)
									Console.WriteLine ("rainGuage timestamp update to " + tRainGuage.timeStamp);
							uploadSuccess = false;
						}

						//1.1.2 server version is old or not exist  upload success
						if (((JsonValue)result [0]) ["result"] == "success") {
							Console.WriteLine ("success" + content);
							DBConnection.updateRainGuageTimeStamp (theRainGuage.rainGuageID, "" + ((JsonValue)result [0]) ["timeStamp"]);

							var tRainGuages = DBConnection.getAllRainGuage();
							foreach (RainGauge tRainGuage in tRainGuages)
								if (tRainGuage.rainGuageID == theRainGuage.rainGuageID)
									Console.WriteLine ("rainGuage timestamp update to " + tRainGuage.timeStamp);	
						}
					}
					//1.2 update fields with no flags infomation from server if the server has newer version 

				}
				//2. download new fields from server
			}

			return uploadSuccess;
		}

		public void downLoadRainGuage(){
			using (WebClient webClient = new WebClient()) {
				//0. check if it's user
				var isUser=hasUser ();
				if (isUser == -1)
					return;

				var operation = "/downloadRainGuage";
				Console.WriteLine ("" + url + operation);
				var content = webClient.DownloadString (url + operation);
				var result = JsonObject.Parse (content);
				for (int i=0; i<result.Count; i++) {
					var farmID = DBConnection.getFarmID (((JsonValue)result [i]) ["farmName"]);
					if (farmID == -1)
						continue;
					var totalRain=DBConnection.getRain(farmID);
					var rainGuageID = DBConnection.getRainGuageID (((JsonValue)result [i]) ["farmName"], ((JsonValue)result [i]) ["theDate"]);

					if (rainGuageID != -1) {
						Console.WriteLine ("rainguage time stamp" + DBConnection.getRainGuageTimeStamp (rainGuageID));


						if(DBConnection.getRainGuageTimeStamp(rainGuageID)==((JsonValue)result [i]) ["timeStamp"])
							continue;

						var thisRain = DBConnection.getRainGuage (rainGuageID);
						DBConnection.updateRainGuageRain (rainGuageID, Convert.ToInt32 ("" + ((JsonValue)result [i]) ["rain"]));
						DBConnection.setRain (farmID, totalRain - thisRain + Convert.ToInt32 ("" + ((JsonValue)result [i]) ["rain"]));
						DBConnection.updateRainGuageTimeStamp (rainGuageID, "" + ((JsonValue)result [i]) ["timeStamp"]);
					} else {
						DBConnection.insertRainGuage (farmID, Convert.ToInt32 ("" + ((JsonValue)result [i]) ["rain"]), ((JsonValue)result [i]) ["theDate"]
						                             , ((JsonValue)result [i]) ["timeStamp"]);
						DBConnection.setRain (farmID, totalRain+ Convert.ToInt32 ("" + ((JsonValue)result [i]) ["rain"]));
					}
				}
			}
		}

		public bool syncBin(){
			bool uploadSuccess=true;
			using (WebClient webClient = new WebClient()) {
				//0. check if it's user
				var isUser=hasUser ();
				if (isUser == -1)
					return false;

				//1. upload the bin frirst
				var bins = DBConnection.getAllBin();
				foreach (Bin theBin in bins) {
					// 1.1 upload all bins that has flag
					if (theBin.flag == 1) {
							var operation = "/uploadBin";
							var args = "?";
							args += "binID=" + theBin.binID + "&";
							args += "binSize=" + theBin.binSize + "&";
							args += "bushel=" + theBin.bushel + "&";
							args += "crop=" + theBin.crop + "&";
							args += "moister=" + theBin.moister + "&";
							args += "timeStamp=" + theBin.timeStamp;

							Console.WriteLine ("" + url + operation + args);
							var content = webClient.DownloadString (url + operation + args);
							DBConnection.updateBinFlag (theBin.binID, 0);
							var result = JsonObject.Parse (content);


							//1.1.1 server version is newer than our old data download data from server and disgard what we had changed
							if (((JsonValue)result [0]) ["result"] == "fail") {
								Console.WriteLine ("fail " + content);

								DBConnection.updateBinMoister (theBin.binID, ((JsonValue)result [1]) ["moister"]);
								DBConnection.updateBinSize (theBin.binID, ((JsonValue)result [1]) ["binSize"]);
								DBConnection.updateBinBushel (theBin.binID, ((JsonValue)result [1]) ["bushel"]);
								DBConnection.updateBinCrop (theBin.binID, ((JsonValue)result [1]) ["crop"]);                         
								DBConnection.updateBinTimeStamp (theBin.binID, ((JsonValue)result [1]) ["timeStamp"]);
								
								var tbins = DBConnection.getAllBin();
								foreach (Bin tbin in tbins)
									if (tbin.binID == theBin.binID)
										Console.WriteLine ("bin timestamp update to " + tbin.timeStamp);
								
								uploadSuccess = false;

							}

							//1.1.2 server version is old or not exist  upload success
							if (((JsonValue)result [0]) ["result"] == "success") {
								Console.WriteLine ("success" + content);
								DBConnection.updateBinTimeStamp (theBin.binID, "" + ((JsonValue)result [0]) ["timeStamp"]);

								var tbins = DBConnection.getAllBin();
								foreach (Bin tbin in tbins)
									if (tbin.binID == theBin.binID)
										Console.WriteLine ("bin timestamp update to " + tbin.timeStamp);	
							}
						}
				}
				//2. download new fields from server
			}
			return uploadSuccess;
		}

		public void downLoadBin(){
			using (WebClient webClient = new WebClient()) {
				//0. check if it's user
				var isUser=hasUser ();
				if (isUser == -1)
					return;

				var operation = "/downloadBin";
				Console.WriteLine ("" + url + operation);
				var content = webClient.DownloadString (url + operation);
				var result = JsonObject.Parse (content);
				for (int i=0; i<result.Count; i++) {

					if(DBConnection.getBinTimeStamp(Convert.ToInt32(""+((JsonValue)result [i]) ["binID"]))==((JsonValue)result [i]) ["timeStamp"])
					   continue;

					DBConnection.updateBinMoister (((JsonValue)result [i]) ["binID"], ((JsonValue)result [i]) ["moister"]);
					DBConnection.updateBinSize (((JsonValue)result [i]) ["binID"], ((JsonValue)result [i]) ["binSize"]);
					DBConnection.updateBinBushel (((JsonValue)result [i]) ["binID"], ((JsonValue)result [i]) ["bushel"]);
					DBConnection.updateBinCrop (((JsonValue)result [i]) ["binID"], ((JsonValue)result [i]) ["crop"]);                         
					DBConnection.updateBinTimeStamp (((JsonValue)result [i]) ["binID"], ((JsonValue)result [i]) ["timeStamp"]);	
				}
			}
		}

		public bool syncField(){
			bool uploadSuccess=true;
			using (WebClient webClient = new WebClient()) {
				//0. check if it's user
				var isUser=hasUser ();
				if (isUser == -1)
					return false;

				//1. upload the field frirst
				var farms = DBConnection.getAllFarms ();
				foreach (Farm theFarm in farms) {
					var fields = DBConnection.getAllFields (theFarm.farmID);
					foreach (Field theField in fields) {
						// 1.1 upload all fields that has flag
						if (theField.flag == 1) {
							var operation = "/uploadField";
							var args = "?";
							args += "farmName=" + theFarm.farmName + "&";
							args += "fieldName=" + theField.fieldName + "&";
							args += "acre=" + theField.acre.ToString () + "&";
							args += "note=" + theField.note.ToString () + "&";
							args += "timeStamp=" + theField.timeStamp;

							Console.WriteLine ("" + url + operation + args);
							var content = webClient.DownloadString (url + operation + args);
							DBConnection.updateFieldFlag (theField.fieldID, 0);
							var result = JsonObject.Parse (content);
							//[{"action": "sent", "timeStamp": "1377129556.79", "result": "fail"}, {"note": "Put note here", "timeStamp": "1377129556.79", "fieldName": "asdasd", "farmName": "farm2", "acre": "0"}]
							//((JsonValue)farm) ["farmName"];

							//1.1.1 server version is newer than our old data download data from server and disgard what we had changed
							if (((JsonValue)result [0]) ["result"] == "fail") {
								Console.WriteLine ("fail " + content);

								DBConnection.updateField (theField.fieldID, ((JsonValue)result [1]) ["fieldName"],
								                         Convert.ToInt32 ("" + ((JsonValue)result [1]) ["acre"]),
								                         ((JsonValue)result [1]) ["note"],
								                         ((JsonValue)result [1]) ["timeStamp"]);
								uploadSuccess = false;

							}

							//1.1.2 server version is old or not exist  upload success
							if (((JsonValue)result [0]) ["result"] == "success") {
								Console.WriteLine ("success" + content);
								DBConnection.updateFieldTimeStamp (theField.fieldID, "" + ((JsonValue)result [0]) ["timeStamp"]);
							}
						}
						//1.2 update fields with no flags infomation from server if the server has newer version 
					}
				}
				//2. download/update (new) fields from server
			}
			return uploadSuccess;
		}


		public void downLoadField(){
			using (WebClient webClient = new WebClient()) {
				//0. check if it's user
				var isUser=hasUser ();
				if (isUser == -1)
					return;

				var operation = "/downloadField";
				Console.WriteLine ("" + url + operation);
				var content = webClient.DownloadString (url + operation);
				var result = JsonObject.Parse (content);
				for (int i=0; i<result.Count; i++) {
					int fieldID=DBConnection.getFieldID (((JsonValue)result [i]) ["farmName"],((JsonValue)result [i]) ["fieldName"]);
					if (fieldID == -1)
						continue;
					if(DBConnection.getFieldTimeStamp(fieldID)==((JsonValue)result [i]) ["timeStamp"])
						continue;

					DBConnection.updateField (fieldID, ((JsonValue)result [i]) ["fieldName"],
					                          Convert.ToInt32 ("" + ((JsonValue)result [i]) ["acre"]),
					                          ((JsonValue)result [i]) ["note"],
					                          ((JsonValue)result [i]) ["timeStamp"]);	
				}
			}
		}

		public void downloadFarm(){

			using(WebClient webClient = new WebClient())
			{
				//0. check if it's user
				var isUser=hasUser ();
				if (isUser == -1)
					return;

				var url = "http://madmucfarmserver.appspot.com/data";

				var content = webClient.DownloadString (url);
				var farmsAndTime = JsonObject.Parse (content);
				int ObjectNum = 0;
				string timeStamp="";
				bool upTodate = false;

				foreach (var farm in farmsAndTime)
				{
					if (upTodate)
						break;

					//check if farm is upTODate
					if (ObjectNum == 0) {
						timeStamp=((JsonValue)farm) ["time"].ToString();

						var farms = DBConnection.getAllFarms ();
						foreach (Farm theFarm in farms) {
							if (theFarm.timeStamp == timeStamp) {
								upTodate = true;
							}
							break;
						}

						ObjectNum++;
						if (!upTodate) {
							DBConnection.dropFarmTable ();
							DBConnection.dropFieldTable ();
							DBConnection.dropRainGaugeTable ();
							DBConnection.dropBinTable ();
							DBConnection.insertBin ();
						}
						continue;
					}

					var farmName = ((JsonValue)farm) ["farmName"];
					DBConnection.insertFarm(farmName,timeStamp);
					var farmID=DBConnection.getFarmID (farmName);
					foreach (var field in ((JsonValue)farm) ["fields"]) {
						DBConnection.insertField (((JsonValue)field) ["fieldName"], 0, farmID, "Put note here");
					}
					var webClient2 = new WebClient();
					webClient2.DownloadDataCompleted += (s, e) => {
						var bytes = e.Result; // get the downloaded data
						string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
						string localFilename =farmName+".jpg";
						string localPath = Path.Combine (documentsPath, localFilename);
						File.WriteAllBytes (localPath, bytes); // writes to local storage
						webClient2.Dispose();
					};
					var url2= new Uri(((JsonValue)farm) ["farmImgUrl"]);
					webClient2.DownloadDataAsync(url2);

				}
			}
		}


		public static bool hasConnection(){

			if(!Reachability.IsHostReachable("www.google.com")) {
				Console.WriteLine("host not not not reachable");
				return false;
			}
			else
			{
				Console.WriteLine("host reachable");
				return true;
			}

			//return true;
		}

		//return -1 if no a user 0 if is a user and 1 if is an admin
		public int hasUser(){
			using(WebClient webClient = new WebClient())
			{
				var operation = "/hasUser";
				var args = "?theUser=" + Global.userName + "&password=" +Global.password;
				Console.WriteLine ("" + url + operation+args);
				var content = webClient.DownloadString (url + operation+args);
				var result = JsonObject.Parse (content);
				return result[0] ["hasUser"];
			}
		}
	}
}

