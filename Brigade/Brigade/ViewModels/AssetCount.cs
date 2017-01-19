using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Brigade.Abstractions;
using Brigade.Models;
using System.Threading.Tasks;

namespace Brigade.ViewModels
{
	public class AssetCountViewModel : BindableBase
	{
		private readonly IWorkflowBuilder _workflowBuilder;
		private readonly ILoginService _loginService;
		private readonly ILocalRepositoryService _db;
		public AssetCountViewModel(IWorkflowBuilder workflowBuilder, ILoginService loginService, ILocalRepositoryService repositoryService)
        {
			_workflowBuilder = workflowBuilder;
			_loginService = loginService;
			_db = repositoryService;
		}

		public string Title { get; set; }
		public IEnumerable<Asset> Children { get; set; }
		public int SelectedIndex { get; set; }
		public User FromUser { get; set; }

		public async Task LockAssetsAsync(string assetId)
		{
			string parentId = (string.IsNullOrWhiteSpace(assetId)) ? _loginService.CurrentBrigade.Id : assetId; 
			Asset parent = App.Container

			Children = await asset.LocalDB.AssetTable.CreateQuery()
				.Where(a => a.ContainerId == asset.Id)
				.OrderBy(a => a.Name)
				.ToEnumerableAsync();

			if (Children != null)
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
