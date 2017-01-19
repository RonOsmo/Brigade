using System;
using System.ComponentModel;

namespace Brigade.Abstractions
{
	public interface IEntityBase<T> where T : class, IEntityId
    {
		T Container { get; }
		bool UseContainerFullPath { get; }
	}

	public interface IEntityId : INotifyPropertyChanged
	{
		string Id { get; }
		string SetIdExtras();
		DateTime CreatedAt { get; }
		DateTime UpdatedAt { get; }
		long Version { get; }
		bool Deleted { get; }
	}
}
