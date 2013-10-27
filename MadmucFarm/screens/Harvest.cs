using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using SQLite;
using ElementPack;
using System.Net;
using System.Collections.Specialized;
using System.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace MadmucFarm
{
	public partial class Harvest : DialogViewController
	{
		SQLiteConnection sql;
		WebClient webClient = new WebClient ();
		DateElement date;
		EntryElement implement;
		FloatElementEx yield;
		FloatElementEx moisture;
		EntryElement bin;
		SimpleMultilineEntryElement notes;
		String fieldID;
		String stamp;

		public Harvest (string farmName, string fieldName, SQLiteConnection s) : base (UITableViewStyle.Grouped, null)
		{
			fieldID = farmName + "." + fieldName;


			sql = s;
			//Create the db table for this viewcontroller. 
			sql.CreateTable<HarvestData> ();


			//The Elements with their variables
			date = new DateElement (null, DateTime.Today);
			implement = new EntryElement (null, "Which implement was used?", null);
			yield = new FloatElementEx (0);
			moisture = new FloatElementEx (0);
			bin = new EntryElement (null, "Enter Bin #", null);
			notes = new SimpleMultilineEntryElement (null, null); 

			//Specify element details
			yield.UseCaptionForValueDisplay = true; 
			yield.ShowCaption = true; 
			yield.MinValue = 0;
			yield.MaxValue = 120; 
			yield.Continuous = true; 
			moisture.UseCaptionForValueDisplay = true; 
			moisture.ShowCaption = true; 
			moisture.MinValue = 0; 
			moisture.MaxValue = 25;
			notes.Editable = true; 
			implement.ShouldReturn += delegate {
				implement.ResignFirstResponder (true); 
				return true;
			};
			bin.ShouldReturn += delegate {
				bin.ResignFirstResponder (true);
				return true; 
			};

			//Create the GUI
			Root = new RootElement (null) {
				new Section ("Date"){
					date,
				},
				new Section("Implement Used") {
					implement,
				},
				new Section("Yield (bushel / acre)"){
					yield,
				},
				new Section("Moisture (%)"){
					moisture,
				},
				new Section ("Bin"){
					bin,
				},
				new Section("Notes") {
					notes,
				},
			};

		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.Title = "Harvest";
			this.Pushing = true; 

			UIAlertView alert = new UIAlertView ();
			alert.AddButton ("OK");
			alert.Title = "Alert"; 
			alert.Clicked += delegate {
				this.NavigationController.PopViewControllerAnimated (true);
			};

			var button = new UIButton (UIButtonType.Custom); 
			button.TouchUpInside += (object sender, EventArgs e) => {

				if (!Reachability.IsHostReachable("www.google.com")) {

					//Create table row with data.
					var harvestData = new HarvestData {
						DbField = fieldID,
						DbDate = date.DateValue,
						DbImplement = implement.Value,
						DbYield = yield.Value,
						DbMoisture = moisture.Value,
						DbBin = bin.Value,
						DbNotes = notes.Value,
						DbTimeStamp = stamp,
						DbUpdate = true
					};
					//insert to database
					var query = from x in sql.Table<HarvestData> ()
						where x.DbField == this.fieldID
							select x;

					if (query.Count () != 0) {
						sql.Delete (query.Last ());
					}
					sql.Insert (harvestData);


					alert.Message = "No internet connection found. Your data has been saved to the device. Please try to sync later when you have a connection.";
				} else {
					//Connection available, begin sync with server.
					var response = webClient.UploadString ("http://madmuctut1.appspot.com/Harvest",
					                                       "field=" + fieldID +
					                                       "&date=" + date.DateValue.ToLongDateString() +
					                                       "&implement=" + implement.Value +
					                                       "&cropYield=" + yield.Value.ToString () +
					                                       "&moisture=" + moisture.Value.ToString () +
					                                       "&bin=" + bin.Value +
					                                       "&notes=" + notes.Value +
					                                       "&stamp=" + stamp
					                                       );



					//					if (IsJson (response)) {
					//						var value = JsonObject.Parse (response);
					//
					//						var harvestData = new HarvestData {
					//							DbField = ((JsonValue)value[0])["field"],
					//							DbDate = DateTime.Parse(((JsonValue)value[0])["date"]),
					//							DbImplement = ((JsonValue)value[0])["implement"],
					//							DbYield = ((JsonValue)value[0])["cropYield"],
					//							DbMoisture = ((JsonValue)value[0])["moisture"],
					//							DbBin = ((JsonValue)value[0])["bin"],
					//							DbNotes = ((JsonValue)value[0])["notes"],
					//							DbTimeStamp = ((JsonValue)value[0])["stamp"],
					//							DbUpdate = false
					//						};
					//						var query = from x in sql.Table<HarvestData> ()
					//							where x.DbField == this.fieldID
					//								select x;
					//
					//						if (query.Count () != 0) {
					//							sql.Delete (query.Last ());
					//						}
					//
					//						sql.Insert (harvestData);
					//					} else {

					var harvestData = new HarvestData {
						DbField = fieldID,
						DbDate = date.DateValue,
						DbImplement = implement.Value,
						DbYield = yield.Value,
						DbMoisture = moisture.Value,
						DbBin = bin.Value,
						DbNotes = notes.Value,
						DbTimeStamp = response,
						DbUpdate = false
					};
					var query = from x in sql.Table<HarvestData> ()
						where x.DbField == this.fieldID
							select x;

					if (query.Count () != 0) {
						sql.Delete (query.Last ());
					}

					sql.Insert (harvestData);


					//UpdateLocalHarvest (); 

					alert.Message = "Your data has been synced.";
				}

				alert.Show ();
			};

			button.Frame = new System.Drawing.RectangleF (0, 0, 80, 20);
			button.SetImage (UIImage.FromBundle ("SaveSync"), UIControlState.Normal); 

			if(Global.isAdmin)
			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (button), true);

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			//var query = sql.Query("SELECT max(timeStamp) FROM HavestData",null);


			//grab from database, according to the unique field ID
			var query = from x in sql.Table<HarvestData> ()
				where x.DbField == this.fieldID
					select x;


			if (query.Count () != 0) {
				//use the most recent data
				var data = query.Last (); 
				//populate viewcontroller with data
				date.DateValue = data.DbDate;
				implement.Value = data.DbImplement; 
				yield.Value = data.DbYield;
				yield.Caption = data.DbYield.ToString (); 
				moisture.Value = data.DbMoisture; 
				moisture.Caption = data.DbMoisture.ToString (); 
				bin.Value = data.DbBin; 
				notes.Value = data.DbNotes;
				stamp = data.DbTimeStamp; 

			}
		}

		//		public static bool IsJson (string input)
		//		{ 
		//			input = input.Trim (); 
		//			return input.StartsWith ("{") && input.EndsWith ("}")  
		//				|| input.StartsWith ("[") && input.EndsWith ("]"); 
		//		}

		//		public void UpdateLocalHarvest ()
		//		{
		//			var query = from x in sql.Table<HarvestData> ()
		//				where x.DbUpdate == true
		//					select x; 
		//
		//			foreach (var obj in query) {
		//				var response = webClient.UploadString ("http://madmuctut1.appspot.com/Harvest",
		//				                                       "field=" + obj.DbField +
		//					"&date=" + obj.DbDate.ToShortDateString () +
		//					"&implement=" + obj.DbImplement +
		//					"&cropYield=" + obj.DbYield +
		//					"&moisture=" + obj.DbMoisture +
		//					"&bin=" + obj.DbBin +
		//					"&notes=" + obj.DbNotes +
		//					"&stamp=" + obj.DbTimeStamp
		//				);
		//
		//				if (IsJson (response)) {
		//					var value = JsonObject.Parse (response);
		//
		//
		//					obj.DbField = ((JsonValue)value [0]) ["field"];
		//					obj.DbDate = DateTime.Parse (((JsonValue)value [0]) ["date"]);
		//					obj.DbImplement = ((JsonValue)value [0]) ["implement"];
		//					obj.DbYield = ((JsonValue)value [0]) ["cropYield"];
		//					obj.DbMoisture = ((JsonValue)value [0]) ["moisture"];
		//					obj.DbBin = ((JsonValue)value [0]) ["bin"];
		//					obj.DbNotes = ((JsonValue)value [0]) ["notes"];
		//					obj.DbTimeStamp = ((JsonValue)value [0]) ["stamp"];
		//					obj.DbUpdate = false;
		//					
		//					var query2 = from x in sql.Table<HarvestData> ()
		//						where x.DbField == obj.DbField
		//							select x;
		//
		//					if (query.Count () != 0) {
		//						sql.Delete (query2.Last ());
		//					}
		//					sql.Insert (obj);
		//				} else {
		//
		//					obj.DbTimeStamp = response;
		//					obj.DbUpdate = false;
		//					
		//					var query2 = from x in sql.Table<HarvestData> ()
		//						where x.DbField == obj.DbField
		//							select x;
		//
		//					if (query.Count () != 0) {
		//						sql.Delete (query2.Last ());
		//					}
		//					sql.Insert (obj);
		//				}
		//			} 
		//		}
	}
}
