
using System.ComponentModel;

namespace Brigade.Abstractions
{
	public interface IEntityBase<T>
    {
		T Container { get; set; }
	}

	public interface IEntityId : INotifyPropertyChanged
	{
		string Id { get; set; }
		void SetId(string identifier);
		void SetId();
	}
}
