using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace MadmucFarm
{
	public partial class ModifyBin : DialogViewController
	{
		public ModifyBin (int binNum) : base (UITableViewStyle.Grouped, null)
		{
			Root = new RootElement ("Bin " + binNum) { };
			this.Pushing = true;


			var theBins = DBConnection.getBin(binNum);

			var section=new Section(){};
			EntryElement binSizeElem=new EntryElement("..",null,null);
			EntryElement bushelElem=new EntryElement("..",null,null);
			EntryElement cropElem=new EntryElement("..",null,null);
			EntryElement moisterElem=new EntryElement("..",null,null);
			//var binNumElem=new MultilineElement("Bin # ",binNum.ToString());
			//section.Add(binNumElem);
			foreach(Bin theBin in theBins){
				binSizeElem=new EntryElement("Bin Size: ","Enter bin size here",theBin.binSize.ToString());
				binSizeElem.ShouldReturn+=()=>{binSizeElem.ResignFirstResponder(true);   return true;};
				bushelElem=new EntryElement("Bushel : ","Enter bushel here",theBin.bushel.ToString());
				bushelElem.ShouldReturn+=()=>{bushelElem.ResignFirstResponder(true);   return true;};
			    cropElem=new EntryElement("Crop Kind: ","Enter crop kind in this bin here",theBin.crop);
				cropElem.ShouldReturn+=()=>{cropElem.ResignFirstResponder(true);   return true;};
				moisterElem=new EntryElement("% Moister: ","Enter % moister of this bin here",theBin.moister);
				moisterElem.ShouldReturn+=()=>{cropElem.ResignFirstResponder(true);   return true;};
				section.Add (binSizeElem);
				section.Add (bushelElem);
				section.Add (cropElem);
				section.Add (moisterElem);
			}
			Root.Add(section);

			UIBarButtonItem update = new UIBarButtonItem ("Save & Sync", UIBarButtonItemStyle.Plain, (s,e) => {  
			});

			if(Global.isAdmin){
				this.NavigationItem.RightBarButtonItem =update;
				update.Clicked+=(s,ev)=>{
					try{
					DBConnection.updateBinSize(binNum,Int32.Parse(binSizeElem.Value));
					}
					catch{
						new UIAlertView ("Error", "Wrong input format for bin size! Please enter 0 if it's empty", null, "Continue").Show ();
					return;
				}
				DBConnection.updateBinFlag(binNum,1);

				try{
					DBConnection.updateBinBushel(binNum,Int32.Parse(bushelElem.Value));
				}
				catch{
					new UIAlertView ("Error", "Wrong input format for bushel! Please enter 0 if it's empty", null, "Continue").Show ();
					return;
				}
				try{
					DBConnection.updateBinCrop(binNum,cropElem.Value);
				}
				catch{
					new UIAlertView ("Error", "Wrong input format for crop kind!", null, "Continue").Show ();
					return;
				}
				try{
					DBConnection.updateBinMoister(binNum,moisterElem.Value);
				}
				catch{
					new UIAlertView ("Error", "Wrong input format for moister!", null, "Continue").Show ();
					return;
				}



				if(Synchronization.hasConnection()) {
					Synchronization sync=new Synchronization();

					if(sync.syncBin()){
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
				this.NavigationController.PopViewControllerAnimated(true);
				};
			}


		}
	}
}
