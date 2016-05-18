using Brigade.iOS;
using Xamarin.Forms;
using Brigade.Views;
using UIKit;

[assembly: ExportRenderer(typeof(TextCellExtended), typeof(DiscolosureTextCellRenderer))]

namespace Brigade.iOS
{
	public class DiscolosureTextCellRenderer : Xamarin.Forms.Platform.iOS.TextCellRenderer
	{
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var cell = base.GetCell(item, reusableCell, tv);

			var textCellExtended = item as TextCellExtended;

			if (textCellExtended.ShowDisclosure)
				cell.Accessory = UIKit.UITableViewCellAccessory.DisclosureIndicator;

			return cell;
		}
	}
}