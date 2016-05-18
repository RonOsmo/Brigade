using System;
using System.Windows.Input;
using Brigade.Abstractions;
using Xamarin.Forms;

namespace Brigade.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
		INavigator _navigator;
		AssetStocktakeViewModel _assetViewModel;

		public MainViewModel(INavigator navigator, AssetStocktakeViewModel assetViewModel)
		{
			_navigator = navigator;
			_assetViewModel = assetViewModel;
			ShowAssetStocktakeCommand = new Command(ShowAssetStocktake);
			Title = "Brigade Main Menu";
		}

		public ICommand ShowAssetStocktakeCommand { get; set; }

		private void ShowAssetStocktake()
		{
			_navigator.PushAsync<AssetStocktakeViewModel>(_assetViewModel);
		}
	}
}
