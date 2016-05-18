using Brigade.Services;
using Brigade.ViewModels;
using Brigade.Abstractions;
using Autofac;

namespace Brigade.Core
{
	public class AssetStocktakeModule : Module
    {
        public string AzureConnection { get; set; }
        public string TableName { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MockLoginService>()
                .As<ILoginService>()
                .SingleInstance();

            builder.RegisterType<MockAssetStockService>()
                .As<IAssetStockService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<AssetStocktakeViewModel>()
                .InstancePerLifetimeScope();
        }
    }
}
