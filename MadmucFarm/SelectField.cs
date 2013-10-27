using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using FlyoutNavigation;

namespace MadmucFarm
{
	public partial class SelectField : UIViewController
	{
		FlyoutNavigationController fnc; 

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public SelectField ()
			: base (UserInterfaceIdiomIsPhone ? "SelectField_iPhone" : "SelectField_iPad", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			fnc = new FlyoutNavigationController ();
			fnc.View.Frame = UIScreen.MainScreen.Bounds;
			View.AddSubview (fnc.View);

			// Create the field:
			fnc.NavigationRoot = new RootElement ("Task List") {
				new Section ("Task List") {
					from page in Tasks
						select new StringElement (page) as Element
				}
			};

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}

