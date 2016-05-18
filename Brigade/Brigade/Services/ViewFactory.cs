using System;
using System.Collections.Generic;
using Brigade.Abstractions;
using Autofac;
using Xamarin.Forms;

namespace Brigade.Services
{
	public class ViewFactory : IViewFactory
	{
		private readonly IDictionary<Type, Type> _map = new Dictionary<Type, Type>();
		private readonly IDictionary<string, Tuple<Type,Type>> _mapName = new Dictionary<string, Tuple<Type, Type>>();
		private readonly IComponentContext _componentContext;

		public ViewFactory(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Register<TViewModel, TView>() where TViewModel : class, IViewModel where TView : Page
		{
			Type tvm = typeof(TViewModel);
			Type tv = typeof(TView);

			_map[tvm] = tv;
			_mapName[tvm.Name] = new Tuple<Type, Type>(tvm, tv);
		}

		public Page Resolve<TViewModel>(Action<TViewModel> setStateAction = null) where TViewModel : class, IViewModel
		{
			TViewModel viewModel;
			return Resolve<TViewModel>(out viewModel, setStateAction);
		}

		public Page Resolve<TViewModel>(out TViewModel viewModel, Action<TViewModel> setStateAction = null) where TViewModel : class, IViewModel
		{
			viewModel = _componentContext.Resolve<TViewModel>();

			var viewType = _map[typeof(TViewModel)];
			var view = _componentContext.Resolve(viewType) as Page;

			if (setStateAction != null)
				viewModel.SetState(setStateAction);

			view.BindingContext = viewModel;
			return view;
		}

		public Page Resolve<TViewModel>(TViewModel viewModel) where TViewModel : class, IViewModel
		{
			var viewType = _map[typeof(TViewModel)];
			var view = _componentContext.Resolve(viewType) as Page;
			view.BindingContext = viewModel;
			return view;
		}

		public Page Resolve<TViewModel>(string viewModelName) where TViewModel : class, IViewModel
		{
			var tuple = _mapName[viewModelName];
			var viewModel = tuple.Item1;
			var viewType = tuple.Item2;
			var view = _componentContext.Resolve(viewType) as Page;
			view.BindingContext = viewModel;
			return view;
		}
	}
}
