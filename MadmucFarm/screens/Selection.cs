using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using ElementPack;
using SQLite; 

namespace MadmucFarm
{
	public partial class Selection : DialogViewController
	{
		public Selection (String farmName,String fieldName,int fieldID) : base (UITableViewStyle.Grouped, null)
		{
			this.Pushing = true;
			LocalStorage.getLocalStorageManager ().createTable ();
			Root = new RootElement (farmName+"  "+fieldName) {};

			var section0 = new Section ("Field Info:") { };
			var acreElem=new EntryElement("Acre: ","Enter field size here",DBConnection.getFieldAcre(fieldID).ToString());
			acreElem.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
			acreElem.ShouldReturn+=()=>{acreElem.ResignFirstResponder(true);return true;};
			var noteElem=new SimpleMultilineEntryElement("Note",DBConnection.getFieldNote(fieldID));
			noteElem.Editable=true;
			//noteElem.ShouldReturn+=()=>{noteElem.ResignFirstResponder(true);return true;};
			var btnSave=new StringElement("Save and Sync",()=>{
				try{
					DBConnection.updateAcre(fieldID,Int32.Parse(acreElem.Value));
				}
				catch{
					new UIAlertView ("Error", "Wrong input format for acre!", null, "Continue").Show ();
					return;
				}

				DBConnection.updateFieldFlag(fieldID,1);


				try{
					DBConnection.updateNote(fieldID,noteElem.Value);
				}
				catch{
					new UIAlertView ("Error", "Wrong input format for note!", null, "Continue").Show ();
					return;
				}

				if(Synchronization.hasConnection()) {
					Synchronization sync=new Synchronization();

					if(sync.syncField()){
						UIAlertView alert = new UIAlertView ();
						alert.Title = "Success";
						alert.Message = "Data has been save and uploaded";
						alert.AddButton("OK");
						alert.Show();
					}
					else{
						UIAlertView alert = new UIAlertView ();
						alert.Title = "Out of Date";
						alert.Message = "server have newer data";
						alert.AddButton("OK");
						alert.Show();
					}
				}

				else{
					UIAlertView alert = new UIAlertView ();
					alert.Title = "Reminder";
					alert.Message = "Your Data is not saved to server ";
					alert.AddButton("OK");
					alert.Show();
				}

				acreElem.Value=DBConnection.getFieldAcre(fieldID).ToString();
				noteElem.Value=DBConnection.getFieldNote(fieldID);
				this.Root.Reload(acreElem,UITableViewRowAnimation.Bottom);
				this.Root.Reload(noteElem,UITableViewRowAnimation.Bottom);
			});

			section0.Add(acreElem);
			section0.Add(noteElem);
			if(Global.isAdmin)
				section0.Add(btnSave);
			Root.Add(section0);

			SQLiteConnection sql = DBConnection.initialDB(); 

			var section = new Section ("choose the action you want to do") { };
			var seed=new StringElement("Seed",()=>{
				//add code here Wen

				this.NavigationController.PushViewController(new Seed(farmName,fieldName),true);
			});
			var chemical=new StringElement("Chemical",()=>{
				//add code here Wen
				//this.NavigationController.PushViewController(new Chemical(farmName,fieldName),true);
				this.NavigationController.PushViewController(new ChemicalMenu(farmName,fieldName),true);
			});


			var harvest=new StringElement("Harvest",()=>{
				//add code here khaled
				this.NavigationController.PushViewController(new Harvest(farmName,fieldName, sql),true);
			});
			var cultivation=new StringElement("Cultivation",()=>{
				//add code here khaled
				this.NavigationController.PushViewController(new Cultivation(farmName,fieldName, sql),true);
			});
			var soilTest=new StringElement("Soil Test",()=>{
				//add code here khaled
				this.NavigationController.PushViewController(new SoilTest(farmName,fieldName, sql),true);
			});


			section.Add (seed);
			section.Add(chemical);
			section.Add(harvest);
			section.Add (cultivation);
			section.Add(soilTest);
			Root.Add(section);


		


		}
	}
}
