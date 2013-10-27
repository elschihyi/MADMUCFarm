using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using ElementPack;

namespace MadmucFarm
{
	public partial class Chemical : DialogViewController
	{
		//int fieldId;
		ChemicalDB chemical;
		string farmName;
		string fieldName;
		UIBarButtonItem saveBtn;

		Dictionary<string,ChemicalTemplate> chemicalTemplateDict;
		RadioGroup chemicalTemplate;
		DateElement chemicalDate;
		EntryElement tools;
		EntryElement chemicalTypes;
		EntryElement chemicalRate;
		SimpleMultilineEntryElement note;
		string serverId;

		public Chemical (ChemicalDB c) : base (UITableViewStyle.Grouped, null)
		{
			this.Pushing = true;
			this.farmName = c.farmName;
			this.fieldName = c.fieldName;
			this.chemical = c;

			Root = new RootElement ("Chemical");

			saveBtn = new UIBarButtonItem (UIBarButtonSystemItem.Save);
			this.NavigationItem.RightBarButtonItem =saveBtn;

			initializeUserInterface ();
		}



		public void initializeUserInterface ()
		{
			// 0. Chemical Template

			chemicalTemplateDict = new Dictionary<string, ChemicalTemplate> ();
			chemicalTemplateDict = LocalStorage.getLocalStorageManager ().loadChemicalTemplate();

			Section chemicalTemplateS = new Section ("Chemical Template");
			chemicalTemplate = new RadioGroup (0);



			Section stSection = new Section ();
			foreach(var templateName in chemicalTemplateDict){
				var t = new myRadioElement(templateName.Value.templateName);
				t.OnSelected += delegate(object sender, EventArgs e) {

					InvokeOnMainThread(()=>{
						loadValueFromTemplate(t.Caption);
					});
				};
				stSection .Add(t);
			}
			RootElement stRoot = new RootElement ("Chemical Template", chemicalTemplate) { };
			stRoot.Add(stSection);
			chemicalTemplateS.Add (stRoot);

			// 1. Chemical Date
			Section chemicalDateS = new Section ("Chemical Date");
			var now = DateTime.Now;
			this.chemicalDate = new DateElement ("", now);
			chemicalDateS.Add (this.chemicalDate);

			// 2. Implemented Used
			Section implementedUsedS = new Section ("Implemented Used");
			tools = new EntryElement (" ","Tools","");
			tools.ShouldReturn += delegate {
				tools.ResignFirstResponder(true);
				return true;
			};
			tools.ReturnKeyType = UIReturnKeyType.Done;
			implementedUsedS.Add (tools);


			// 3. Seed Type
			Section chemicalTypeS = new Section ("Chemical Types");

			chemicalTypes = new EntryElement (" ", "Chemical Types", "");
			chemicalTypes.ShouldReturn += delegate {
				chemicalTypes.ResignFirstResponder(true);
				return true;
			};
			chemicalTypes.ReturnKeyType = UIReturnKeyType.Done;
			chemicalTypeS.Add (chemicalTypes);

			// 4. chemical Rate
			Section chemicalRateS = new Section ("Chemical Rate (L/ac)");
			chemicalRate = new EntryElement (" ", "Chemical Rates", "");
			chemicalRate.ShouldReturn += delegate {
				chemicalRate.ResignFirstResponder(true);
				return true;
			};
			chemicalRate.ReturnKeyType = UIReturnKeyType.Done;
			chemicalRateS.Add (chemicalRate);

			// 5. Note
			Section noteS = new Section ("Notes");		
			note = new SimpleMultilineEntryElement ("", " ") { Editable = true }; 
			noteS.Add (note);

			Root.Add (chemicalTemplateS);
			Root.Add (chemicalDateS);
			Root.Add (implementedUsedS);
			Root.Add (chemicalTypeS);
			Root.Add (chemicalRateS);
			Root.Add (noteS);

		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			saveBtn.Clicked += (sender, e) => {
				RecordChemicalHistory();

			};

			ReadDataFromLocalStorage ();
		}

		public void RecordChemicalHistory(){
			//LocalStorage.getLocalStorageManager().deleteObject(farmName,fieldName,2);
			//var db = LocalStorage.getLocalStorageManager().getLocalDB();

			var c = new ChemicalDB ();
			c.chemicalDate = chemicalDate.DateValue.ToString();
			c.chemicalRates = chemicalRate.Value;
			c.chemicalTypes = chemicalTypes.Value;
			c.note = note.Value;
			c.implementedUsed = tools.Value;
			c.farmName = farmName;
			c.fieldName = fieldName;
			c.templateIndex = chemicalTemplate.Selected;
			c.isSynch = false;
			c.id = chemical.id;
			c.serverId = serverId;


			bool status = Reachability.InternetConnectionStatus() != NetworkStatus.NotReachable && Reachability.IsHostReachable("usaskwef889.appspot.com");

			if (status) {
				Console.Out.WriteLine ("Internet Connected !!!");
				LocalStorage.getLocalStorageManager ().sychChemical (c);

			} else {
				Console.Out.WriteLine ("Internet Disconnected !!!");
				LocalStorage.getLocalStorageManager ().insertChemicalHistory (c);
			}
			//db.Insert (chemical);

			UIAlertView alert = new UIAlertView ();
			if(status){

				alert.Title = "Saved";
				alert.Message = "Your Data Successfully Synchronized";
				alert.AddButton("OK");
			}else{
				alert.Title = "Saved";
				alert.Message = "Your Data Save Into Local Storage";
				alert.AddButton("OK");
			}
			alert.Clicked += (sender, e) => {
				if(e.ButtonIndex == 0)
				{
					this.NavigationController.PopViewControllerAnimated(true);
				}
			};
			alert.Show();

		}

		public void ReadDataFromLocalStorage()
		{
			//ChemicalDB chemical = LocalStorage.getLocalStorageManager ().loadChemical (farmName,fieldName).First();

			if (chemical.chemicalDate != null) {
				chemicalDate.DateValue = System.Convert.ToDateTime (chemical.chemicalDate);
				chemicalRate.Value = chemical.chemicalRates;
				chemicalTypes.Value = chemical.chemicalTypes;
				note.Value = chemical.note;
				tools.Value = chemical.implementedUsed;
				chemicalTemplate.Selected = chemical.templateIndex;
				serverId = chemical.serverId;

			}
		}


		public void loadValueFromTemplate(string templateName)
		{
			//Console.Out.WriteLine(templateName);
			var t = chemicalTemplateDict [templateName];

			tools.Value = t.implementedUsed;
			chemicalRate.Value = t.chemicalRates;
			chemicalTypes.Value = t.chemicalTypes;

		}
	}
}
