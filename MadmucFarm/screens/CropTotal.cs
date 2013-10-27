using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Collections;

namespace MadmucFarm
{
	public partial class CropTotal : DialogViewController
	{
		public CropTotal () : base (UITableViewStyle.Grouped, null)
		{
			this.Pushing = true;
			Root = new RootElement ("CropTotal") { };
			var section=new Section (){};
			Crop [] crops=new Crop[75];
			for(int binNum=1;binNum<75;binNum++){
				var theBins = DBConnection.getBin(binNum);
				foreach(Bin theBin in theBins){
					if(theBin.bushel==0)
						continue;
					for(int i=0;i<crops.Length;i++){
						if(crops[i]!=null && crops[i].cropName==theBin.crop){
							crops[i].cropSize+=theBin.bushel;
							break;
						}
						if(crops[i]==null){
							crops[i]=new Crop();
							crops[i].cropName=theBin.crop;
							crops[i].cropSize=theBin.bushel;
							break;
						}
					}
				}
			}
			for(int i=0;i<crops.Length;i++){
				if(crops[i]!=null){
					var cropElem=new StyledStringElement(crops[i].cropName,crops[i].cropSize.ToString());
					section.Add (cropElem);
				}
				else {
					break;
				}
			}

			Root.Add(section);
						
		}
	}
}
