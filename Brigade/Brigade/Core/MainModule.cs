using Brigade.Services;
using Brigade.ViewModels;
using Brigade.Abstractions;
using Autofac;

namespace Brigade.Core
{
    public class MainModule : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<MainViewModel>()
				.InstancePerLifetimeScope();
		}
	}
}
