using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using ElementPack;
using SQLite;
using System.Net;

namespace MadmucFarm
{
	public partial class SoilTest : DialogViewController
	{
		SQLiteConnection sql;
		string stamp;
		WebClient webClient = new WebClient ();

		SimpleMultilineEntryElement notes;
		String fieldID;

		public SoilTest (string farmName,string fieldName,SQLiteConnection s) : base (UITableViewStyle.Grouped, null)
		{

			sql = s;
			sql.CreateTable<SoilTestData> (); 


			fieldID = farmName + "." + fieldName; 

			notes = new SimpleMultilineEntryElement (null, null); 

			notes.Editable = true; 


			Root = new RootElement (null) {
				new Section ("Notes"){
					notes,
				},
			};

		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.Title = "Soil Test";
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
					var soilTestData = new SoilTestData {
						DbField = fieldID,
						DbNotes = notes.Value,
						DbTimeStamp = stamp,
						DbUpdate = true
					};
					//insert to database
					var query = from x in sql.Table<SoilTestData> ()
						where x.DbField == this.fieldID
							select x;

					if (query.Count () != 0) {
						sql.Delete (query.Last ());
					}
					sql.Insert (soilTestData);


					alert.Message = "No internet connection found. Your data has been saved to the device. Please try to sync later when you have a connection.";
				} else {
					//Connection available, begin sync with server.
					var response = webClient.UploadString ("http://madmuctut1.appspot.com/SoilTest",
					                                       "field=" + fieldID +
					                                       "&notes=" + notes.Value +
					                                       "&stamp=" + stamp
					                                       );

					var soilTestData = new SoilTestData {
						DbField = fieldID,
						DbNotes = notes.Value,
						DbTimeStamp = response,
						DbUpdate = false
					};
					var query = from x in sql.Table<SoilTestData> ()
						where x.DbField == this.fieldID
							select x;

					if (query.Count () != 0) {
						sql.Delete (query.Last ());
					}

					sql.Insert (soilTestData);

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

			//grab from database
			var q = from x in sql.Table<SoilTestData> ()
				where x.DbField == this.fieldID
					select x;


			if (q.Count () != 0) {
				//use the most recent data
				var data = q.Last ();

				notes.Value = data.DbNotes; 

			}



		}


	}
}
