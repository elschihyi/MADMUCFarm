using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.IO;
using SQLite;
using System.Net;
using System.Runtime.Serialization;
using System.Json;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace MadmucFarm
{
	public partial class SelectFarm : DialogViewController
	{
		WebClient webClient = new WebClient ();
		Section section;

		[DataContract]
		internal class ServerData
		{
			[DataMember]
			internal string field;

			[DataMember]
			internal string stamp;
		}
		public SelectFarm () : base (UITableViewStyle.Grouped, null)
		{

			if(Synchronization.hasConnection()){

				var webRequest = WebRequestManager.getWebRequestManager();
				webRequest.downloadSeedTemplate();
				webRequest.downloadChemicalTemplate();
				/*
				Synchronization sync = new Synchronization ();
				sync.downloadFarm();
				*/
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();


			this.NavigationItem.SetRightBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Refresh, (sender,args) => {
				//add a loading view
				LoadingOverlay loadingOverlay=new LoadingOverlay (UIScreen.MainScreen.Bounds);
				View.Add (loadingOverlay);

				/*
				if(!Reachability.IsHostReachable("www.google.com")){	

					new UIAlertView ("Alert", "No internet connection detected. Please try again later.", null, "OK", null).Show ();
				}else {
					LocalStorage.getLocalStorageManager().sychAllUnsychSeed();
					LocalStorage.getLocalStorageManager().sychAllUnsychChemical();

					CreateTables();

					//Upadate logged changes
					SyncHarvest();
					SyncCultivation();
					SyncSoilTest();

					//Download other changes
					DownloadHarest();
					DownloadCultivation();
					DownloadSoilTest();

					var sync=new Synchronization();

					//sync farm
					sync.downloadFarm();
		
					//sync is just those onject have upload flag true;
					sync.syncBin();
					sync.syncField();
					sync.syncRainGuage();

					//download all current data
					sync.downLoadBin();
					sync.downLoadField();
					sync.downLoadRainGuage();
				}
				*/
				Task.Factory.StartNew (
					// tasks allow you to use the lambda syntax to pass work
					() => {
					if(!Reachability.IsHostReachable("www.google.com")){	

						new UIAlertView ("Alert", "No internet connection detected. Please try again later.", null, "OK", null).Show ();
					}else {
						LocalStorage.getLocalStorageManager().sychAllUnsychSeed();
						LocalStorage.getLocalStorageManager().sychAllUnsychChemical();

						CreateTables();

						//Upadate logged changes
						SyncHarvest();
						SyncCultivation();
						SyncSoilTest();

						//Download other changes
						DownloadHarest();
						DownloadCultivation();
						DownloadSoilTest();

						var sync=new Synchronization();

						//sync farm
						sync.downloadFarm();

						//sync is just those onject have upload flag true;
						sync.syncBin();
						sync.syncField();
						sync.syncRainGuage();

						//download all current data
						sync.downLoadBin();
						sync.downLoadField();
						sync.downLoadRainGuage();
					}
				}
				).ContinueWith ( 
				                t => {
					loadingOverlay.Hide ();
					ViewWillAppear (true);
					new UIAlertView ("Alert", "Sync Complete.", null, "OK", null).Show ();

				}, TaskScheduler.FromCurrentSynchronizationContext()
				);



			}), true); 
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Root = new RootElement ("Madmac Farm"){};
			this.Pushing=true;


			//farm section
			var farms = DBConnection.getAllFarms();
			//int farmNumber=farms.Count();
			section = new Section ("Farms:"){};

			foreach(Farm farm in farms){
				int farmID = farm.farmID;
				string farmName = farm.farmName;
				int fieldNumber = DBConnection.getAllFields(farmID).Count ();
				string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				string localFilename =farmName+".jpg";
				string localPath = Path.Combine (documentsPath, localFilename);
				var farmImg=UIImage.FromFile (localPath);
				if(farmImg==null)
					farmImg=UIImage.FromFile ("Icon.png");
				var theFarm=new BadgeElement(farmImg,farmName+"      "+fieldNumber+" fields",()=>{
					Console.WriteLine("Farm Name is: "+farmName);
					var field=new SelectField(farmName,farmID,fieldNumber);
					this.NavigationController.PushViewController(field,true);
				});
				section.Add(theFarm);
			}
			Root.Add(section);


			//grain section
			var section2 = new Section ("Grain Inventory:"){};
			var grin1 = new StringElement ("Bins' Detail", () => {
				var selectBin=new SelectBin(1);
				this.NavigationController.PushViewController(selectBin,true);
			});
			var cropTotalbtn = new StringElement ("Crop Total Detail", () => {
				var cropTotal=new CropTotal();
				this.NavigationController.PushViewController(cropTotal,true);
			});

			section2.Add (grin1);
			section2.Add (cropTotalbtn);

			Root.Add (section2);
		}

		public void CreateTables(){
			SQLiteConnection sql = DBConnection.initialDB ();
			sql.CreateTable<HarvestData> ();
			sql.CreateTable<CultivationData> ();
			sql.CreateTable<SoilTestData> ();
			sql.Close ();
		}

		public void SyncHarvest()
		{
			SQLiteConnection sql = DBConnection.initialDB (); 

			var query = from x in sql.Table<HarvestData> ()
				where x.DbUpdate == true
					select x; 

			foreach (var obj in query) {
				var response = webClient.UploadString ("http://madmuctut1.appspot.com/Harvest",
				                                       "field=" + obj.DbField +
				                                       "&date=" + obj.DbDate.ToLongDateString() +
				                                       "&implement=" + obj.DbImplement +
				                                       "&cropYield=" + obj.DbYield +
				                                       "&moisture=" + obj.DbMoisture +
				                                       "&bin=" + obj.DbBin +
				                                       "&notes=" + obj.DbNotes +
				                                       "&stamp=" + obj.DbTimeStamp
				                                       );

				obj.DbTimeStamp = response;
				obj.DbUpdate = false;

				var query2 = from x in sql.Table<HarvestData> ()
					where x.DbField == obj.DbField
						select x;

				if (query2.Count () != 0) {
					sql.Delete (query2.Last ());
				}
				sql.Insert (obj);
			}
			sql.Close ();
		}

		public void SyncCultivation()
		{
			SQLiteConnection sql = DBConnection.initialDB ();

			var query = from x in sql.Table<CultivationData> ()
				where x.DbUpdate == true
					select x; 

			foreach (var obj in query) {
				var response = webClient.UploadString ("http://madmuctut1.appspot.com/Cultivation",
				                                       "field=" + obj.DbField +
				                                       "&date=" + obj.DbDate.ToLongDateString() +
				                                       "&implement=" + obj.DbImplement +
				                                       "&depth=" + obj.DbDepth +
				                                       "&notes=" + obj.DbNotes +
				                                       "&stamp=" + obj.DbTimeStamp
				                                       );

				obj.DbTimeStamp = response;
				obj.DbUpdate = false;

				var query2 = from x in sql.Table<CultivationData> ()
					where x.DbField == obj.DbField
						select x;

				if (query2.Count () != 0) {
					sql.Delete (query2.Last ());
				}
				sql.Insert (obj);
			}
			sql.Close ();
		}

		public void SyncSoilTest()
		{
			SQLiteConnection sql = DBConnection.initialDB ();

			var query = from x in sql.Table<SoilTestData> ()
				where x.DbUpdate == true
					select x; 

			foreach (var obj in query) {
				var response = webClient.UploadString ("http://madmuctut1.appspot.com/SoilTest",
				                                       "field=" + obj.DbField +
				                                       "&notes=" + obj.DbNotes +
				                                       "&stamp=" + obj.DbTimeStamp
				                                       );

				obj.DbTimeStamp = response;
				obj.DbUpdate = false;

				var query2 = from x in sql.Table<SoilTestData> ()
					where x.DbField == obj.DbField
						select x;

				if (query2.Count () != 0) {
					sql.Delete (query2.Last ());
				}
				sql.Insert (obj);
			}

			sql.Close ();
		}

		public void DownloadHarest()
		{
			SQLiteConnection sql = DBConnection.initialDB (); 

			var query = from x in sql.Table<HarvestData> ()
				select x;

			var harvests = new List<ServerData> ();

			foreach (var obj in query) {
				harvests.Add(new ServerData { 
					field = obj.DbField,
					stamp = obj.DbTimeStamp
				});
			}

			var ser = new DataContractJsonSerializer (typeof(ServerData));
			var stream = new MemoryStream (); 
			var reader = new StreamReader (stream);
			ser.WriteObject (stream, harvests);
			stream.Position = 0;

			var response = webClient.UploadString ("http://madmuctut1.appspot.com/HarvestDownload", reader.ReadToEnd());
			var value = JsonObject.Parse (response);
			foreach (var val in value) {
				var harvestData = new HarvestData ();
				harvestData.DbField = ((JsonValue)val) ["field"];
				harvestData.DbDate = DateTime.Parse(((JsonValue)val)["date"]);
				harvestData.DbImplement = ((JsonValue)val)["implement"];
				harvestData.DbYield = int.Parse(((JsonValue)val)["cropYield"]);
				harvestData.DbMoisture = int.Parse(((JsonValue)val)["moisture"]);
				harvestData.DbBin = ((JsonValue)val)["bin"];
				harvestData.DbNotes = ((JsonValue)val)["notes"];
				harvestData.DbTimeStamp = ((JsonValue)val)["stamp"];
				harvestData.DbUpdate = false;

				var query2 = from x in sql.Table<HarvestData> ()
					where x.DbField == harvestData.DbField
						select x;

				if (query2.Count() != 0) {
					sql.Delete (query2.First());
				}

				sql.Insert (harvestData);
			}

			sql.Close ();
		}	

		public void DownloadCultivation()
		{
			SQLiteConnection sql = DBConnection.initialDB (); 

			var query = from x in sql.Table<CultivationData> ()
				select x;

			var cultivations = new List<ServerData> ();

			foreach (var obj in query) {
				cultivations.Add(new ServerData { 
					field = obj.DbField,
					stamp = obj.DbTimeStamp
				});
			}

			var ser = new DataContractJsonSerializer (typeof(ServerData));
			var stream = new MemoryStream (); 
			var reader = new StreamReader (stream);
			ser.WriteObject (stream, cultivations);
			stream.Position = 0;

			var response = webClient.UploadString ("http://madmuctut1.appspot.com/CultivationDownload", reader.ReadToEnd());
			var value = JsonObject.Parse (response);
			foreach (var val in value) {
				var cultivationData = new CultivationData ();
				cultivationData.DbField = ((JsonValue)val) ["field"];
				cultivationData.DbDate = DateTime.Parse(((JsonValue)val)["date"]);
				cultivationData.DbImplement = ((JsonValue)val)["implement"];
				cultivationData.DbNotes = ((JsonValue)val)["notes"];
				cultivationData.DbTimeStamp = ((JsonValue)val)["stamp"];
				cultivationData.DbUpdate = false;

				var query2 = from x in sql.Table<CultivationData> ()
					where x.DbField == cultivationData.DbField
						select x;

				if (query2.Count() != 0) {
					sql.Delete (query2.First());
				}

				sql.Insert (cultivationData);
			}

			sql.Close ();
		}

		public void DownloadSoilTest()
		{
			SQLiteConnection sql = DBConnection.initialDB (); 

			var query = from x in sql.Table<SoilTestData> ()
				select x;

			var soiltests = new List<ServerData> ();

			foreach (var obj in query) {
				soiltests.Add(new ServerData { 
					field = obj.DbField,
					stamp = obj.DbTimeStamp
				});
			}

			var ser = new DataContractJsonSerializer (typeof(ServerData));
			var stream = new MemoryStream (); 
			var reader = new StreamReader (stream);
			ser.WriteObject (stream, soiltests);
			stream.Position = 0;

			var response = webClient.UploadString ("http://madmuctut1.appspot.com/SoilTestDownload", reader.ReadToEnd());
			var value = JsonObject.Parse (response);
			foreach (var val in value) {
				var soiltestData = new SoilTestData ();
				soiltestData.DbField = ((JsonValue)val) ["field"];
				soiltestData.DbNotes = ((JsonValue)val)["notes"];
				soiltestData.DbTimeStamp = ((JsonValue)val)["stamp"];
				soiltestData.DbUpdate = false;

				var query2 = from x in sql.Table<SoilTestData> ()
					where x.DbField == soiltestData.DbField
						select x;

				if (query2.Count() != 0) {
					sql.Delete (query2.First());
				}

				sql.Insert (soiltestData);
			}

			sql.Close ();
		}
	}
}
