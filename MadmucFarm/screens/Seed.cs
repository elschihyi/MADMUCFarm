using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using ElementPack;

namespace MadmucFarm
{
	public class myRadioElement : RadioElement{
		public myRadioElement (string s):base(s){
		}
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			base.Selected (dvc, tableView, indexPath);
			var selected = OnSelected;
			if (selected != null){
				selected (this, EventArgs.Empty);

			}
		}

		public event EventHandler<EventArgs> OnSelected;

	}

	public partial class Seed : DialogViewController
	{

		string farmName;
		string fieldName;

		UIBarButtonItem saveBtn;
		Dictionary <string,SeedTemplate> seedTemplateDict;

		RadioGroup seedTemplate;
		DateElement seedDate;
		EntryElement tools;
		EntryElement seedDepth;
		EntryElement seedTypes;
		EntryElement varietyName;
		FloatElementEx seedRate;
		EntryElement seedTreatment;
		FloatElementEx NH3;
		FloatElementEx _11;
		SimpleMultilineEntryElement note;

		public Seed (string farmName,string fieldName) : base (UITableViewStyle.Grouped, null)
		{
			LocalStorage.getLocalStorageManager().createTable();

			//this.fieldId = fieldId; 
			this.farmName = farmName;
			this.fieldName = fieldName;
			this.Pushing = true;
			Root = new RootElement ("Seed");

			saveBtn = new UIBarButtonItem (UIBarButtonSystemItem.Save);
			this.NavigationItem.RightBarButtonItem =saveBtn;

			initializeUserInterface ();
		}

		//		public Seed (int fieldId) : base (UITableViewStyle.Grouped, null)
		//		{
		//
		//		}

		public void initializeUserInterface ()
		{
			// 0. Seed Templates
			seedTemplateDict = new Dictionary<string, SeedTemplate> ();
			seedTemplateDict = LocalStorage.getLocalStorageManager ().loadSeedTemplate ();

			Section seedTemplateS = new Section ("Seed Template");
			seedTemplate = new RadioGroup (0);



			Section stSection = new Section ();
			foreach(var templateName in seedTemplateDict){
				var t = new myRadioElement(templateName.Value.templateName);
				t.OnSelected += delegate(object sender, EventArgs e) {

					InvokeOnMainThread(()=>{
						loadValueFromTemplate(t.Caption);
					});
				};
				stSection .Add(t);
			}
			RootElement stRoot = new RootElement ("Seed Template", seedTemplate) { };
			stRoot.Add(stSection);

			seedTemplateS.Add (stRoot);

			// 1. Seed Date
			Section seedDateS = new Section ("Seed Date");
			this.seedDate = new DateElement ("", DateTime.Now);
			seedDateS.Add (this.seedDate);

			// 2. Seed Type
			Section seedTypeS = new Section ("Seed Types");

			seedTypes = new EntryElement (" ", "Seed Types", "");
			seedTypes.ShouldReturn += delegate {
				seedTypes.ResignFirstResponder(true);
				return true;
			};
			seedTypes.ReturnKeyType = UIReturnKeyType.Done;
			seedTypeS.Add (seedTypes);

			// 3. Seeding Depth
			Section seedDepthS = new Section ("Seeding Depth (in)");
			//			seedDepth = new FloatElementEx (0, lockable: false) {
			//				ShowCaption = true,
			//				UseCaptionForValueDisplay = true,
			//				MaxValue = 2,
			//			};
			seedDepth = new EntryElement(" ","Seed Depth", "0");
			seedDepth.ShouldReturn += delegate {
				seedDepth.ResignFirstResponder(true);
				return true;
			};
			seedDepth.ReturnKeyType = UIReturnKeyType.Done;
			seedDepthS.Add(seedDepth);

			// 4. Implemented Used
			Section implementedUsedS = new Section ("Implemented Used");
			tools = new EntryElement (" ","Tools","");
			tools.ShouldReturn += delegate {
				tools.ResignFirstResponder(true);
				return true;
			};
			tools.ReturnKeyType = UIReturnKeyType.Done;
			implementedUsedS.Add (tools);

			// 5. Variety Name
			Section varietyNameS = new Section ("Variety Name");
			varietyName = new EntryElement (" ","Enter Variety Name","");
			varietyName.ReturnKeyType = UIReturnKeyType.Done;
			varietyName.ShouldReturn += delegate {

				varietyName.ResignFirstResponder(true);
				return true;
			};
			varietyNameS.Add (varietyName);


			// 6. Seed Rate
			Section seedRateS = new Section ("Seed Rate (lb/ac)");
			seedRate = new FloatElementEx (0, lockable: false) {
				ShowCaption = true,
				UseCaptionForValueDisplay = true,
				MaxValue = 300,
			};
			seedRateS.Add(seedRate);

			// 7. Seed Treatment
			Section seedTreatmentS = new Section ("Seed Treatment");
			seedTreatment = new  EntryElement (" ","Enter Seed Treatment","");
			seedTreatment.ReturnKeyType = UIReturnKeyType.Done;

			seedTreatmentS.Add (seedTreatment);


			// 8. NH3
			Section NH3S = new Section ("NH3 (lb/ac)");
			NH3 = new FloatElementEx (0, lockable: false) { 
				ShowCaption = true,
				UseCaptionForValueDisplay = true,
				MaxValue = 120,
			};
			NH3S.Add (NH3);

			// 9. 11-52-20
			Section _11S = new Section ("11-52-20 (lb/ac)");
			_11 = new FloatElementEx (0, lockable: false) { 
				ShowCaption = true,
				UseCaptionForValueDisplay = true,
				MaxValue = 100,
			};
			_11S.Add(_11);


			// 10. Note
			Section noteS = new Section ("Notes");		

			note = new SimpleMultilineEntryElement ("", " ") { Editable = true }; 
			noteS.Add (note);



			Root.Add (seedTemplateS);
			Root.Add (seedDateS);
			Root.Add (seedTypeS);
			Root.Add (implementedUsedS);
			Root.Add (seedDepthS);
			Root.Add (varietyNameS);
			Root.Add (seedRateS);
			Root.Add (seedTreatmentS);
			Root.Add (NH3S);
			Root.Add (_11S);
			Root.Add (noteS);


		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			ReadDataFromLocalStorage ();
			saveBtn.Clicked += (sender, e) => {
				RecordSeedHistory();

			};

		}

		public void RecordSeedHistory(){
			var seed = new SeedDB();
			seed.seedDate = seedDate.DateValue.ToString();
			seed.implementedUsed = tools.Value;
			seed.seedDepth =System.Convert.ToSingle(seedDepth.Value);	
			seed.seedTypes = seedTypes.Value;
			seed.varietyName = varietyName.Value;
			seed.seedRate = seedRate.Value;
			seed.seedTreatment = seedTreatment.Value;
			seed.NH3 = NH3.Value;
			seed._11 = _11.Value;
			seed.notes = note.Value;
			seed.farmName = this.farmName;
			seed.fieldName = this.fieldName;
			seed.templateIndex = seedTemplate.Selected;
			seed.isSynch = false;

			//bool response = false;

			bool status = Reachability.InternetConnectionStatus() != NetworkStatus.NotReachable && Reachability.IsHostReachable("usaskwef889.appspot.com");


			if (status) {
				Console.Out.WriteLine ("Internet Connected !!!");
				LocalStorage.getLocalStorageManager ().sychSeed (seed);
			} else {
				Console.Out.WriteLine ("Internet Disconnected !!!");
				LocalStorage.getLocalStorageManager ().insertSeedHistory (seed);
			}

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

		public void ReadDataFromLocalStorage(){

			SeedDB seedHistory = LocalStorage.getLocalStorageManager ().loadSeed (farmName,fieldName);

			if(seedHistory != null){
				seedDate.DateValue = System.Convert.ToDateTime (seedHistory.seedDate);
				tools.Value = seedHistory.implementedUsed;
				seedDepth.Value = seedHistory.seedDepth.ToString("0.00");
				seedTypes.Value = seedHistory.seedTypes;
				varietyName.Value = seedHistory.varietyName;
				seedRate.Value = seedHistory.seedRate;
				seedRate.Caption = seedHistory.seedRate.ToString ();
				seedTreatment.Value = seedHistory.seedTreatment;
				NH3.Value = seedHistory.NH3;
				NH3.Caption = seedHistory.NH3.ToString ();
				_11.Value = seedHistory._11;
				_11.Caption = seedHistory._11.ToString ();
				note.Value = seedHistory.notes;
				seedTemplate.Selected = seedHistory.templateIndex;
			}


		}

		public void loadValueFromTemplate(string templateName)
		{
			var t = seedTemplateDict [templateName];

			tools.Value = t.implementedUsed;
			seedDepth.Value = t.seedDepth;
			seedTypes.Value = t.seedTypes;
			varietyName.Value = t.varietyName;
			seedRate.Value = System.Convert.ToInt32(t.seedRate);
			seedRate.Caption = t.seedRate.ToString ();
			seedTreatment.Value = t.seedTreatment;
			NH3.Value = System.Convert.ToInt32(t.NH3);
			NH3.Caption = t.NH3;
			_11.Value = System.Convert.ToInt32(t._11);
			_11.Caption = t._11;
		}


	}


}
