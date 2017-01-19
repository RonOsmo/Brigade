using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brigade.Models;
using Brigade.Abstractions;
using Xamarin.Forms;

namespace Brigade.ViewModels
{

	public class AssetStocktakeViewModel : ViewModelBase
    {
		private readonly IAssetStockService _assetService;
		private readonly IWorkflowBuilder _workflowBuilder;

		public Asset Parent { get; set; }
        public IEnumerable<Asset> Children { get; set; }
		public IEnumerable<User> OnlineUsers { get; set; }
		public string TargetUser { get; set; }
		public DateTime When { get; set; }
		public int SelectedIndex { get; set; }

        public AssetStocktakeViewModel(IAssetStockService assetService, IWorkflowBuilder workflowBuilder)
        {
            _assetService = assetService;
			_workflowBuilder = workflowBuilder;
			When = DateTime.Now;

			Task result = GetAssetsAsync();
        }

        public async Task GetAssetsAsync()
        {
			Asset asset = null;
			if (asset != null && Children != null)
			{
				int index = 0;
				foreach (var child in Children)
				{
					if (index == SelectedIndex)
					{
						asset = child;
						break;
					}
					index++;
				}
			}
            Parent = asset;
			OnPropertyChanged("Parent");
			Title = (asset == null) ? "Top Level Assets" : asset.Description;
			OnPropertyChanged("Title");
            Children = await _assetService.GetAssetsAsync(asset);
			OnPropertyChanged("Children");
        }

        public async Task UnlockAsync()
        {
			if (Parent != null)
			{
				Parent.LockedByUser = null;
				Parent.LockedOnDevice = null;
				await _assetService.SaveAsync(Parent);
			}
        }

		public void Send()
		{
			if (Parent != null && !string.IsNullOrWhiteSpace(TargetUser))
			{
				var requests = _workflowBuilder
					.Request(Parent, "Count Assets", When)
					.User(TargetUser)
					.Tasks();
			}
		}

	}
}
