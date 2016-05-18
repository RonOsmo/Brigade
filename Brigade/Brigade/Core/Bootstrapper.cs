using Brigade.ViewModels;
using Brigade.Abstractions;
using Brigade.Views;
using Autofac;
using Xamarin.Forms;

namespace Brigade.Core
{
	public abstract class AutofacBootstrapper
	{
		public void Run()
		{
			var builder = new ContainerBuilder();

			ConfigureContainer(builder);

			var container = builder.Build();
			var viewFactory = container.Resolve<IViewFactory>();

			RegisterViews(viewFactory);

			ConfigureApplication(container);
		}

		protected virtual void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterModule<AutofacModule>();
		}

		protected abstract void RegisterViews(IViewFactory viewFactory);

		protected abstract void ConfigureApplication(IContainer container);
	}

	public class Bootstrapper : AutofacBootstrapper
	{
		private readonly Application _application;

		public Bootstrapper(Application application)
		{
			_application = application;
		}

		protected override void ConfigureContainer(ContainerBuilder builder)
		{
			base.ConfigureContainer(builder);
			builder.RegisterModule<AssetStocktakeModule>();
		}

		protected override void RegisterViews(IViewFactory viewFactory)
		{
			viewFactory.Register<MainViewModel, MainView>();
			viewFactory.Register<AssetStocktakeViewModel, AssetStocktakeView>();
			//viewFactory.Register<AssetStockcountViewModel, AssetStockcountView>();
		}

		protected override void ConfigureApplication(IContainer container)
		{
			// set main page
			var viewFactory = container.Resolve<IViewFactory>();
			var mainPage = viewFactory.Resolve<MainViewModel>();
			var navigationPage = new NavigationPage(mainPage);

			_application.MainPage = navigationPage;
		}
	}
}
