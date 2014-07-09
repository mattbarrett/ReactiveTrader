using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public static class Styles
	{
		public static readonly UIColor RTLightBlue = UIColor.FromRGB (98, 127, 152);
		public static readonly UIColor RTWarnOrange = UIColor.FromRGB (128, 80, 0);
		public static readonly UIColor RTFailRed = UIColor.FromRGB (192, 48, 0);
		public static readonly UIColor RTDarkBlue = UIColor.FromRGB (10, 15, 30);
		public static readonly UIColor RTDarkerBlue = UIColor.FromRGB (8, 11, 20);


		//
		// Make all tables look the same...
		//
		// But is that desirable? TODO: UI workshop around presentation of eg trades blotter.
		//

		public static void ConfigureTable(UITableView uiTableView) {
			uiTableView.RowHeight = 206.0f;
			uiTableView.AllowsSelection = false;
			uiTableView.SeparatorColor = Styles.RTLightBlue;
		}


		//
		// Format nicely, or 'vanilla' for ease of editing...
		//

		public static String FormatNotional (long notionalToFormat, bool niceFormatting)
		{
			if (niceFormatting) {
				return String.Format ("{0:#,##0}", notionalToFormat);
			} else {
				return notionalToFormat.ToString();
			}
		}

	}
}

