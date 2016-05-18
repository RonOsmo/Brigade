using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brigade.Abstractions;
using Brigade.Models;
using Brigade.Services;

namespace Brigade.ViewModels
{
    public class AssetStockcountViewModel : ViewModelBase
    {
		private readonly IAssetStockService _assetService;
		private readonly IWorkflowBuilder _workflowBuilder;
		private readonly ILoginService _loginService;

		public Asset Parent { get; set; }
		public IEnumerable<Asset> Children { get; set; }
		public int SelectedIndex { get; set; }
		public User FromUser { get; set; }

		public AssetStockcountViewModel(IAssetStockService assetService, IWorkflowBuilder workflowBuilder, ILoginService loginService)
		{
			_assetService = assetService;
			_workflowBuilder = workflowBuilder;
			_loginService = loginService;
		}

		public async Task LockAssetsAsync(Asset asset)
		{
			Parent = asset;
			Children = await _assetService.GetAssetsAsync(asset);
			if (Parent != null && Children != null)
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
			OnPropertyChanged("Parent");
			Title = (asset == null) ? "Top Level Assets" : asset.Description;
			OnPropertyChanged("Title");
			Children = await _assetService.GetAssetsAsync(asset);
			OnPropertyChanged("Children");
			Parent.LockedByUser = _loginService.CurrentUser;
			Parent.LockedOnDevice = _loginService.CurrentDevice;

			Task result = _assetService.SaveAsync(Parent);
		}

		public async Task SetSightedAsync()
		{
			if (Children != null)
			{
				int index = 0;
				foreach (var child in Children)
				{
					if (index == SelectedIndex)
					{
						child.Sighted = true;
						child.MissingDate = null;
						await _assetService.SaveAsync(child);
						break;
					}
					index++;
				}
			}
		}

		public async Task SetMissingAsync()
		{
			if (Children != null)
			{
				int index = 0;
				foreach (var child in Children)
				{
					if (index == SelectedIndex)
					{
						child.Sighted = false;
						child.MissingDate = DateTime.Now;
						await _assetService.SaveAsync(child);
						break;
					}
					index++;
				}
			}
		}


	}
}
