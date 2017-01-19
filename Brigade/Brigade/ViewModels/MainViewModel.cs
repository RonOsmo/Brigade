using System;
using System.Collections.Generic;
using System.Windows.Input;
using Brigade.Abstractions;
using Xamarin.Forms;

namespace Brigade.ViewModels
{
	public class MenuItem
	{
		public string Name;
		public ICommand Command;
	}

    public class MainViewModel : ViewModelBase
    {
		INavigator _navigator;
		AssetStocktakeViewModel _assetViewModel;

		public List<MenuItem> MenuItems = new List<MenuItem>();

		public MainViewModel(INavigator navigator, AssetStocktakeViewModel assetViewModel)
		{
			_navigator = navigator;
			_assetViewModel = assetViewModel;
			AssetsCommand = new Command(AssetStocktake);
			Title = "Brigade Main Menu";

			MenuItem[] items = 
				{ 
					new MenuItem { Name = "Admin", Command = AdministrationCommand },
					new MenuItem { Name = "Assets", Command = AssetsCommand },
					new MenuItem { Name = "Availability", Command = AvailabilityCommand },
					new MenuItem { Name = "Events", Command = EventsCommand },
					new MenuItem { Name = "Tasks", Command = TasksCommand },
					new MenuItem { Name = "Training", Command = TasksCommand },
				};

			MenuItems.AddRange(items);
		}

		public ICommand AssetsCommand { get; set; }
		public ICommand AvailabilityCommand { get; set; }
		public ICommand EventsCommand { get; set; }
		public ICommand TasksCommand { get; set; }
		public ICommand TrainingCommand { get; set; }
		public ICommand AdministrationCommand { get; set; }

		private void AssetStocktake()
		{
			_navigator.PushAsync<AssetStocktakeViewModel>(_assetViewModel);
		}
	}
}
