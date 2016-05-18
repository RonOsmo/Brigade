using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Brigade.Abstractions
{
	public interface IViewModel : INotifyPropertyChanged
    {
		string ViewModelId { get; }
        string Title { get; set; }
        void SetState<T>(Action<T> action) where T : class, IViewModel;
    }

	public interface IViewFactory
	{
		void Register<TViewModel, TView>()
			where TViewModel : class, IViewModel
			where TView : Page;

		Page Resolve<TViewModel>(Action<TViewModel> setStateAction = null)
			where TViewModel : class, IViewModel;

		Page Resolve<TViewModel>(out TViewModel viewModel, Action<TViewModel> setStateAction = null)
			where TViewModel : class, IViewModel;

		Page Resolve<TViewModel>(TViewModel viewModel)
			where TViewModel : class, IViewModel;
	}
}
