
using Xamarin.Forms;

namespace Brigade.Views
{
	public partial class AssetView : ContentPage
	{
		public AssetView (AssetViewModel viewModel)
		{
            BindingContext = viewModel;
		}
	}
}
