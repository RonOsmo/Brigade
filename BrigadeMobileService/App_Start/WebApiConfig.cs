using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Web.Http;
using BrigadeMobileService.DataObjects;
using BrigadeMobileService.Models;
using Microsoft.WindowsAzure.Mobile.Service;

namespace BrigadeMobileService
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Database.SetInitializer(new MobileServiceInitializer());
        }
    }

    public class MobileServiceInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            List<Asset> assets = new List<Asset>
            {
                new Asset { AssetCode = "Tanker1", Description = "Tanker 1 - Rego ABC-123", Sighted = true },
                new Asset { AssetCode = "Tanker2", Description = "Tanker 1 - Rego XYZ-987", Sighted = true },
                new Asset { AssetCode = "Station", Description = "Station Building - 117 Maccelsfield Rd", Sighted = true },
            };
            Asset bin1 = new Asset { AssetCode = "Bin1", Container = assets[0], Sighted = true };
            assets.Add(bin1);
            assets.Add(new Asset { AssetCode = "Screwdriver", Container = bin1, Description = "Screwdriver Philips Head", Sighted = true });
            assets.Add(new Asset { AssetCode = "Socket25", Container = bin1, Description = "Socket Spanner Metric 25mm", Sighted = true });
            Asset commsRoom = new Asset { AssetCode = "Comms", Container = assets[2], Description = "Communications Room", Sighted = true };
            assets.Add(commsRoom);

            for (int i = 1; i < 6; i++)
                assets.Add(new Asset { AssetCode = "CB" + i.ToString(), Container = commsRoom, Description = "CFA Standard UHF Tranceiver #" + i.ToString(), Sighted = true });

            foreach (Asset item in assets)
            {
                item.SetId();
                context.Set<Asset>().Add(item);
            }

            base.Seed(context);
        }
    }
}

