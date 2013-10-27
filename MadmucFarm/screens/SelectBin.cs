using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace MadmucFarm
{
	public partial class SelectBin : DialogViewController
	{
		Section section;
		List<StringElement> theBinElemList;
		int startBin;

		public SelectBin (int startBin) : base (UITableViewStyle.Grouped, null)
		{
			this.startBin = startBin;
			/*
			foreach(Bin bin in bins){
				var newBinID=bin.binID;
				var theBinElem=new StringElement("Bin "+newBinID,()=>{
					Console.WriteLine("BinID is"+ newBinID);
					var theBin=new ModifyBin(newBinID);
					this.NavigationController.PushViewController(theBin,true);
				});
				section.Add(theBinElem);
			}
			*/

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.Pushing = true;

			Root = new RootElement ("Bins") {};

			//var bins = DBConnection.getBins(startBin);
			section = new Section () { };
			theBinElemList=new List<StringElement>();

			for(int i=0;i<75;i++){
				var newBinID=startBin+i;
				var theBins = DBConnection.getBin(newBinID);
				foreach (Bin theBin in theBins){ 
					var theBinElem=new StringElement("Bin "+newBinID+"  kind:"+theBin.crop+"        bushel:"+theBin.bushel,()=>{
						var theNewBin=new ModifyBin(newBinID);
						this.NavigationController.PushViewController(theNewBin,true);
					});
					theBinElemList.Add(theBinElem);
					section.Add(theBinElem);
				}
			}
			Root.Add(section);
		}
	}
}


