using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace MadmucFarm
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			//insert bin

			if (DBConnection.getBin(1).Count () == 0) {
				DBConnection.insertBin ();
			}





			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
		    Login login= new Login ();
			//DialogViewController dvc = new DialogViewController (logIn.Root);
			var nvc = new UINavigationController();
			nvc.PushViewController (login, false);

			window.RootViewController = nvc;
			// If you have defined a root view controller, set it here:
			// window.RootViewController = myViewController;
			
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

