using Brigade.Abstractions;
using Brigade.Services;
using Brigade.Views;
using Autofac;
using Xamarin.Forms;

namespace Brigade.Core
{
	public class AutofacModule : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			// service registration
			builder.RegisterType<ViewFactory>()
				.As<IViewFactory>()
				.SingleInstance();

			builder.RegisterType<Navigator>()
				.As<INavigator>()
				.SingleInstance();

			// navigation registration
			builder.Register<INavigation>(context => App.Current.MainPage.Navigation)
				.SingleInstance();
		}
	}
}
