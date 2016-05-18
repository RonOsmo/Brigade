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
                    new Asset { AssetId = "Tanker1", Description = "Tanker 1: Ford Model (T) - vintage 1932", Container = CurrentUser.Container, GotChildren = true },
                    new Asset { AssetId = "Tanker2", Description = "Tanker 2: A Newer truck - rego ABC123", Container = CurrentUser.Container, GotChildren = true },
                    new Asset { AssetId = "Station", Description = "Station House Model 'R'", Container = CurrentUser.Container, GotChildren = true },
                    new Asset { AssetId = "Hydrants", Description = "Macclesfield Hydrants for which we are responsible", Container = CurrentUser.Container, GotChildren = true },
                    };
            }

            switch (parent.AssetId)
            {
                case "Box1-LeftMid":
                case "Box2-LeftMid":
                case "Box3-LeftBack":
                case "Box4-LeftBack":
                case "Box5-RightMid":
                case "Box6-RightBack":
                    return new Asset[]
                    {
                        new Asset { AssetId = "Screwdriver-PhillipsHead", Container = parent },
                        new Asset { AssetId = "Screwdriver-Plain", Container = parent },
                        new Asset { AssetId = "Socket-14mm", Container = parent },
                        new Asset { AssetId = "Socket-15mm", Container = parent },
                        new Asset { AssetId = "Socket-16mm", Container = parent },
                        new Asset { AssetId = "Hose-10m", Container = parent },
                        new Asset { AssetId = "Hose-20m", Container = parent },
                    };
                case "Hydrants":
                    return new Asset[]
                    {
                        new Asset { AssetId = "Hydrant1", Description = "Hydrant1 - Corner of Tschampion & Macclesfield Roads", Container = parent },
                    };
                case "Tanker1":
                case "Tanker2":
                    return new Asset[]
                    {
                        new Asset { AssetId = "Box1-LeftMid", Container = parent, GotChildren = true },
                        new Asset { AssetId = "Box2-LeftMid", Container = parent, GotChildren = true },
                        new Asset { AssetId = "Box3-LeftBack", Container = parent, GotChildren = true },
                        new Asset { AssetId = "Box4-LeftBack", Container = parent, GotChildren = true },
                        new Asset { AssetId = "Box5-RightMid", Container = parent, GotChildren = true },
                        new Asset { AssetId = "Box6-RightBack", Container = parent, GotChildren = true },
                    };
                case "Station":
                    return new Asset[]
                    {
                        new Asset { AssetId="Kitchen", Container = parent },
                        new Asset { AssetId="Assembly", Container = parent },
                        new Asset { AssetId="Sick", Container = parent },
                        new Asset { AssetId="Comms", Container = parent } ,
                        new Asset { AssetId="Loo", Container = parent },
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
				file.WriteLine($"Parent: Id={asset.Id} Container={asset.Container.Id} AssetId={asset.AssetId} Sighted={asset.Sighted} MissingDate={(asset.MissingDate.HasValue ? asset.MissingDate.Value.ToString("dMy") : "")} Description={(!string.IsNullOrEmpty(asset.Description) ? asset.Description : string.Empty)}");
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

					file.WriteLine($" Child: Id={asset.Id} Container={asset.Container.Id} AssetId={asset.AssetId} Sighted={asset.Sighted} MissingDate={(asset.MissingDate.HasValue ? asset.MissingDate.Value.ToString("dMy") : "")} Description={(!string.IsNullOrEmpty(asset.Description) ? asset.Description : string.Empty)}");
				}
			}

			await result;
		}

	}
}
