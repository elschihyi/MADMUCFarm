using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace MadmucFarm
{
	public partial class RainDetail : DialogViewController
	{
		public RainDetail (int farmID,string farmName) : base (UITableViewStyle.Grouped, null)
		{
			Root = new RootElement ("RainDetail") { };
			this.Pushing=true;
			var section=new Section(farmName){};
			var details=DBConnection.getRainDetails(farmID);

			foreach(RainGauge rg in details){
				var detailElem=new StyledStringElement(rg.rain.ToString()+"(mm)", "  "+rg.theDate.ToLongDateString());
				section.Add(detailElem);
			}
			Root.Add (section);
		}
	}
}
