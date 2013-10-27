using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using FlyoutNavigation;
using System.Drawing;
using System.IO;

namespace MadmucFarm
{
	public partial class SelectField : DialogViewController
	{
		public SelectField (String farmName,int farmID,int fieldNumber) : base (UITableViewStyle.Grouped, null)
		{
			Root = new RootElement (farmName) {};
			this.Pushing = true;

   			var fnc = new FlyoutNavigationController() { };
			fnc.View.Frame = UIScreen.MainScreen.Bounds;
			View.AddSubview (fnc.View);

			// button for slide out fields
			/*
			this.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem (UIBarButtonSystemItem.Action, (s,e)=> {
				fnc.ToggleMenu ();
			}),true);
			*/
			this.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem("Field", UIBarButtonItemStyle.Plain, (sender,args) => {fnc.ToggleMenu ();}), true);


		    //create the menu
			var fields = DBConnection.getAllFields(farmID);
			var menuSection = new Section ("Fields") {};
			foreach(Field field in fields){
				var fieldID=field.fieldID;//change this
				var fieldName=field.fieldName;//change this

				var fieldNameElment=new StringElement(fieldName,()=>{
					Selection selection=new Selection(farmName,fieldName,fieldID);
					this.NavigationController.PushViewController(selection,true);
				});
				menuSection.Add (fieldNameElment);
			}

			fnc.NavigationRoot=new RootElement("Fields"){menuSection};


			fnc.ViewControllers=new []{
				new FieldImage(farmName,farmID,fnc,this),
				//new UIViewController { View = new UILabel { Text = "Animals (drag right)" } },
			};

		}
	}

	public partial class FieldImage :  DialogViewController{
		public FieldImage(string farmName,int farmID,FlyoutNavigationController fnc,SelectField sf) : base (UITableViewStyle.Grouped, null)
		{
			Root = new RootElement (null) {};
			this.Pushing = true;

			var section0 = new Section () {};
			var totalRainGuage = new StringElement ("Total Raid Guage(mm): " + DBConnection.getRain (farmID),()=>{});
			var showDetail=new StringElement("Show Rain Guage Detail",()=>{
				RainDetail rd=new RainDetail(farmID,farmName);
				sf.NavigationController.PushViewController(rd,true);
			});
			section0.Add (totalRainGuage);
			section0.Add (showDetail);
			Root.Add (section0);


			var section1 = new Section () {};
			var rainGuage=new EntryElement ("Rain to add(mm):   ",null,"");
			rainGuage.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
			var update=new StringElement("Add and Sync",()=>{
				try{
					DBConnection.updateRain(farmID,Int32.Parse(rainGuage.Value));
				}
				catch{
					new UIAlertView ("Error", "Wrong input format!", null, "Continue").Show (); 
					return;
				}
				DBConnection.updateRainGuageFlagByFarmID(farmID,1);

				//has connection
				if(Synchronization.hasConnection()) {
					Synchronization sync=new Synchronization();
					if(sync.syncRainGuage()){
						totalRainGuage.Caption="Total Raid Guage(mm): " + DBConnection.getRain (farmID);
						rainGuage.Value="0";
						this.Root.Reload(totalRainGuage,UITableViewRowAnimation.Bottom);
						//this.Root.Reload(rainGuage,UITableViewRowAnimation.Bottom);

						UIAlertView alert = new UIAlertView ();
						alert.Title = "Success";
						alert.Message = "Data has been save and uploaded";
						alert.AddButton("OK");
						alert.Show();
						RainDetail rd=new RainDetail(farmID,farmName);
						sf.NavigationController.PushViewController(rd,true);
					}
					else{
						totalRainGuage.Caption="Total Raid Guage(mm): " + DBConnection.getRain (farmID);
						rainGuage.Value="0";
						this.Root.Reload(totalRainGuage,UITableViewRowAnimation.Bottom);
						//this.Root.Reload(rainGuage,UITableViewRowAnimation.Bottom);

						UIAlertView alert = new UIAlertView ();
						alert.Title = "Out of Date";
						alert.Message = "server have newer data";
						alert.AddButton("OK");
						alert.Show();
						RainDetail rd=new RainDetail(farmID,farmName);
						sf.NavigationController.PushViewController(rd,true);
					}
				}
				//no connection
				else{
					totalRainGuage.Caption="Total Raid Guage(mm): " + DBConnection.getRain (farmID);
					rainGuage.Value="0";
					this.Root.Reload(totalRainGuage,UITableViewRowAnimation.Bottom);
					//this.Root.Reload(rainGuage,UITableViewRowAnimation.Bottom);

					UIAlertView alert = new UIAlertView ();
					alert.Title = "Reminder";
					alert.Message = "Your Data is not saved to server ";
					alert.AddButton("OK");
					alert.Show();
					RainDetail rd=new RainDetail(farmID,farmName);
					sf.NavigationController.PushViewController(rd,true);
				}


			});
			section1.Add (rainGuage);
			section1.Add (update);

			if(Global.isAdmin)
				Root.Add (section1);


			var section2 = new Section () { };
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string localFilename =farmName+".jpg";
			string localPath = Path.Combine (documentsPath, localFilename);
			var farmImg=UIImage.FromFile (localPath);
			if(farmImg==null)
				farmImg=UIImage.FromFile ("Icon.png");
			var imageView = new UIImageView (farmImg);
			var scrollView=new UIScrollView (
				new RectangleF(0,0,fnc.View.Frame.Width-20,250)
			);

			scrollView.ContentSize = imageView.Image.Size;
			scrollView.AddSubview (imageView);

			scrollView.MaximumZoomScale = 3f;
			scrollView.MinimumZoomScale = .1f;
			scrollView.ViewForZoomingInScrollView += (UIScrollView sv) => { return imageView; };

			var imageElement=new UIViewElement(null,scrollView,false);
			section2.Add(imageElement);
			Root.Add (section2);
		}
	}
}
