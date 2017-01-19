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
}
