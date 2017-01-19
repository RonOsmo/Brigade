using System;
using System.Collections.Generic;
using Brigade.Models;
using Brigade.Abstractions;
using System.IO;
using System.Threading.Tasks;

namespace Brigade.Services
{
    public class MockAssetStockService : IAssetStockService
    {
        public User CurrentUser { get; set; }

        public MockAssetStockService(ILoginService loginSVC)
        {
            CurrentUser = loginSVC.CurrentUser;
        }

        public async Task<IEnumerable<Asset>> GetAssetsAsync(Asset parent)
        {
            await SimulateAsync();
            if (parent == null)
            {
                return new Asset[]
                    {
                    new Asset { Name = "Tanker1", Description = "Tanker 1: Ford Model (T) - vintage 1932", Container = CurrentUser.Container, GotChildren = true },
                    new Asset { Name = "Tanker2", Description = "Tanker 2: A Newer truck - rego ABC123", Container = CurrentUser.Container, GotChildren = true },
                    new Asset { Name = "Station", Description = "Station House Model 'R'", Container = CurrentUser.Container, GotChildren = true },
                    new Asset { Name = "Hydrants", Description = "Macclesfield Hydrants for which we are responsible", Container = CurrentUser.Container, GotChildren = true },
                    };
            }

            switch (parent.Name)
            {
                case "Box1-LeftMid":
                case "Box2-LeftMid":
                case "Box3-LeftBack":
                case "Box4-LeftBack":
                case "Box5-RightMid":
                case "Box6-RightBack":
                    return new Asset[]
                    {
                        new Asset { Name = "Screwdriver-PhillipsHead", Container = parent },
                        new Asset { Name = "Screwdriver-Plain", Container = parent },
                        new Asset { Name = "Socket-14mm", Container = parent },
                        new Asset { Name = "Socket-15mm", Container = parent },
                        new Asset { Name = "Socket-16mm", Container = parent },
                        new Asset { Name = "Hose-10m", Container = parent },
                        new Asset { Name = "Hose-20m", Container = parent },
                    };
                case "Hydrants":
                    return new Asset[]
                    {
                        new Asset { Name = "Hydrant1", Description = "Hydrant1 - Corner of Tschampion & Macclesfield Roads", Container = parent },
                    };
                case "Tanker1":
                case "Tanker2":
                    return new Asset[]
                    {
                        new Asset { Name = "Box1-LeftMid", Container = parent, GotChildren = true },
                        new Asset { Name = "Box2-LeftMid", Container = parent, GotChildren = true },
                        new Asset { Name = "Box3-LeftBack", Container = parent, GotChildren = true },
                        new Asset { Name = "Box4-LeftBack", Container = parent, GotChildren = true },
                        new Asset { Name = "Box5-RightMid", Container = parent, GotChildren = true },
                        new Asset { Name = "Box6-RightBack", Container = parent, GotChildren = true },
                    };
                case "Station":
                    return new Asset[]
                    {
                        new Asset { Name="Kitchen", Container = parent },
                        new Asset { Name="Assembly", Container = parent },
                        new Asset { Name="Sick", Container = parent },
                        new Asset { Name="Comms", Container = parent } ,
                        new Asset { Name="Loo", Container = parent },
                    };
            }

            return null;
        }

        async Task SimulateAsync()
        {
			var result = Task.Run(() =>
				{
					for (int i = 0; i < int.MaxValue; i++)
						;
				});
			await result;			
        }

        public async Task LockAsync(Asset parent)
        {
            Task result = SimulateAsync();

            if (parent != null || parent.LockedByUser != null)
                parent.LockedByUser = CurrentUser;

            await result;
        }

        public async Task SaveAsync(Asset asset)
        {
            Task result = SimulateAsync();
            if (asset == null)
                return;

			using (var file = File.AppendText(@"c:\BrigadeAssets.log"))
			{
				file.WriteLine($"Parent: Id={asset.Id} Container={asset.Container.Id} Name={asset.Name} Sighted={asset.Sighted} MissingDate={(asset.MissingDate.HasValue ? asset.MissingDate.Value.ToString("dMy") : "")} Description={(!string.IsNullOrEmpty(asset.Description) ? asset.Description : string.Empty)}");
			}

			await result;
        }

		public async Task SaveAsync(IEnumerable<Asset> children)
		{
			Task result = SimulateAsync();

			using (var file = File.AppendText(@"c:\BrigadeAssets.log"))
			{
				foreach (Asset asset in children)
				{
					if (asset.Sighted)
						asset.MissingDate = null;
					else
						asset.MissingDate = DateTime.Now;

					file.WriteLine($" Child: Id={asset.Id} Container={asset.Container.Id} Name={asset.Name} Sighted={asset.Sighted} MissingDate={(asset.MissingDate.HasValue ? asset.MissingDate.Value.ToString("dMy") : "")} Description={(!string.IsNullOrEmpty(asset.Description) ? asset.Description : string.Empty)}");
				}
			}

			await result;
		}

	}
}
