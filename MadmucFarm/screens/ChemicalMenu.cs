using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace MadmucFarm
{
	public partial class ChemicalMenu : DialogViewController
	{
		string _farmName;
		string _fieldName;

		public ChemicalMenu (string farmName, string fieldName) : base (UITableViewStyle.Grouped, null)
		{
			_farmName = farmName;
			_fieldName = fieldName;

			this.Pushing = true;
//			Root = new RootElement ("ChemicalMenu");
//			Section section = new Section ("Existing Chemical ");
//
//			var chemicals = LocalStorage.getLocalStorageManager ().loadChemical (_farmName,_fieldName);
//
//			if (chemicals != null){
//				foreach( var c in chemicals){
//
//					section.Add (new StringElement (c.chemicalTypes,()=>{
//						this.NavigationController.PushViewController(new Chemical (c),true);
//
//					}));
//				}
//			}
//
//
//
//			Section newSection = new Section ("New Chemical "){
//				new StringElement("Add New Chemical",()=>{
//
//					ChemicalDB c = new ChemicalDB ();
//					c.farmName = _farmName;
//					c.fieldName = _fieldName;
//
//
//					this.NavigationController.PushViewController(new Chemical(c),true);
//
//				})
//			};
//
//			Root.Add (section);
//			Root.Add (newSection);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			Root = new RootElement ("ChemicalMenu");
			Section section = new Section ("Existing Chemical ");

			var chemicals = LocalStorage.getLocalStorageManager ().loadChemical (_farmName,_fieldName);

			if (chemicals != null){
				foreach( var c in chemicals){

					section.Add (new StringElement (c.chemicalTypes,()=>{
						this.NavigationController.PushViewController(new Chemical (c),true);

					}));
				}
			}



			Section newSection = new Section ("New Chemical "){
				new StringElement("Add New Chemical",()=>{

					ChemicalDB c = new ChemicalDB ();
					c.farmName = _farmName;
					c.fieldName = _fieldName;


					this.NavigationController.PushViewController(new Chemical(c),true);

				})
			};

			Root.Add (section);
			Root.Add (newSection);
		}

	}
}
