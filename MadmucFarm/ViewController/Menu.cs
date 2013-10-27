using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace UsaskFarm
{
	public partial class Menu : DialogViewController
	{

		Seed seedVC;
		Chemical chemicalVC;
		public Menu () : base (UITableViewStyle.Grouped, null)
		{
			if(seedVC == null){
				seedVC = new Seed (1);
			}

			if(chemicalVC == null ){
				chemicalVC = new Chemical (1);
			}

			Root = new RootElement ("Menu") {
				new Section(){
					new StyledStringElement("Seed",()=>this.NavigationController.PushViewController(seedVC,true)){
						Accessory = UITableViewCellAccessory.DisclosureIndicator,
					},
					new StyledStringElement("Chemical",()=>this.NavigationController.PushViewController(chemicalVC,true)){
						Accessory = UITableViewCellAccessory.DisclosureIndicator,
					},
				}
			};
		}

	}
}
