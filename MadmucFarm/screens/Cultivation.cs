using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using SQLite;
using ElementPack;
using System.Net;

namespace MadmucFarm
{
	public partial class Cultivation : DialogViewController
	{
		SQLiteConnection sql;
		WebClient webClient = new WebClient ();
		String stamp;

		DateElement date;
		EntryElement implement;
		FloatElementEx depth;
		SimpleMultilineEntryElement notes;

		String fieldID;

		public Cultivation (string farmName,string fieldName,SQLiteConnection s) : base (UITableViewStyle.Grouped, null)
		{
			fieldID = farmName + "." + fieldName; 

			sql = s;
			sql.CreateTable<CultivationData> (); 


			date = new DateElement (null, DateTime.Today);
			implement = new EntryElement (null, "Which implement was used?", null);
			depth = new FloatElementEx (0);

			notes = new SimpleMultilineEntryElement (null, null); 

			depth.UseCaptionForValueDisplay = true; 
			depth.ShowCaption = true; 
			depth.MinValue = 0;
			depth.MaxValue = 2; 
			implement.ShouldReturn += delegate {
				implement.ResignFirstResponder (true);
				return true; 
			};

			notes.Editable = true; 

			Root = new RootElement (null) {
				new Section ("Date"){
					date,
				},
				new Section ("Implement Used"){
					implement,
				},
				new Section("Depth (in)"){
					depth,
				},
				new Section ("Notes"){
					notes,
				},
			};

		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.Title = "Cultivation";
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
					var cultivationData = new CultivationData {
						DbField = fieldID,
						DbDate = date.DateValue,
						DbImplement = implement.Value,
						DbDepth = depth.Value,
						DbNotes = notes.Value,
						DbTimeStamp = stamp,
						DbUpdate = true
					};
					//insert to database
					var query = from x in sql.Table<CultivationData> ()
						where x.DbField == this.fieldID
							select x;

					if (query.Count () != 0) {
						sql.Delete (query.Last ());
					}
					sql.Insert (cultivationData);


					alert.Message = "No internet connection found. Your data has been saved to the device. Please try to sync later when you have a connection.";
				} else {
					//Connection available, begin sync with server.
					var response = webClient.UploadString ("http://madmuctut1.appspot.com/Cultivation",
					                                       "field=" + fieldID +
					                                       "&date=" + date.DateValue.ToLongDateString() +
					                                       "&implement=" + implement.Value +
					                                       "&depth=" + depth.Value.ToString() +
					                                       "&notes=" + notes.Value +
					                                       "&stamp=" + stamp
					                                       );

					var cultivationData = new CultivationData {
						DbField = fieldID,
						DbDate = date.DateValue,
						DbImplement = implement.Value,
						DbDepth = depth.Value,
						DbNotes = notes.Value,
						DbTimeStamp = response,
						DbUpdate = false
					};
					var query = from x in sql.Table<CultivationData> ()
						where x.DbField == this.fieldID
							select x;

					if (query.Count () != 0) {
						sql.Delete (query.Last ());
					}

					sql.Insert (cultivationData);

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
			var q = from x in sql.Table<CultivationData> ()
				where x.DbField == this.fieldID
					select x;


			if (q.Count () != 0) {
				//use the most recent data
				var data = q.Last ();

				date.DateValue = data.DbDate;
				implement.Value = data.DbImplement; 
				depth.Value = data.DbDepth; 
				depth.Caption = data.DbDepth.ToString (); 
				notes.Value = data.DbNotes; 

			}



		}


	}
}
