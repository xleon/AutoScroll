using Foundation;
using UIKit;

namespace AutoScroll
{
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
		    var controller = new UINavigationController(new FormViewController());
	        Window = new UIWindow(UIScreen.MainScreen.Bounds) { RootViewController = controller };
	        Window.MakeKeyAndVisible();

	        return true;
	    }
	}
}

