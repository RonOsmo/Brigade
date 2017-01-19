using System;
using System.Linq;
using Brigade.Abstractions;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json;

namespace Brigade.Models
{
	public abstract class EntityBase<T> : IEntityId, IEntityBase<T> where T : class, IEntityId
	{

		#region Data context

		ILocalRepositoryService _localService;

		[JsonIgnore]
		[Microsoft.Practices.Unity.Dependency]
		public ILocalRepositoryService LocalDB
		{
			get { return _localService; }
			set
			{
				if (_localService != value)
				{
					_localService = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Container implementation

		T _container;
		string _containerId;

		[JsonIgnore]
		public virtual bool UseContainerFullPath { get { return false; } }

		[JsonIgnore]
		public T Container
		{
			get
			{
				if (_container == null && !string.IsNullOrWhiteSpace(_containerId))
				{
					if (_localService.TypeSyncTableMap.ContainsKey(typeof(T)))
					{
						IMobileServiceSyncTable<T> table = (IMobileServiceSyncTable<T>)_localService.TypeSyncTableMap[typeof(T)];
						table.LookupAsync(_containerId).ContinueWith(x =>
						{
							_container = x.Result;
						});
					}
				}
				return _container;
			}
			set
			{
				if ((_container != null && value == null) || (_container == null && value != null) || (_container != null && value != null && !_container.Equals(value)))
				{
					_container = value;
					_containerId = AnyContainer.GetDependentContainers(_container);
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public string ContainerId
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(_containerId) && !string.IsNullOrWhiteSpace(_id) && _id.Contains(","))
				{
					string[] seps = { "," };
					string[] parts = _id.Split(seps, System.StringSplitOptions.RemoveEmptyEntries);
					_containerId = parts[0];
				}
				return _containerId;
			}
			set
			{
				if (value != _containerId)
				{
					_containerId = value;
					_container = null;
					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region IEntityId members

		string _id;

		[JsonProperty(Required = Required.Always)]
		public string Id
		{
			get
			{
				// don't set the id until a container has been set
				if (string.IsNullOrWhiteSpace(_id) && !string.IsNullOrWhiteSpace(_containerId))
				{
					_id = _containerId + "," + (new Guid()).ToString() + SetIdExtras();
				}
				return _id;
			}
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}

		public virtual string SetIdExtras() { return string.Empty; }

		[Version]
		public long Version { get; set; }

		[UpdatedAt]
		public DateTime UpdatedAt { get; set; }

		[CreatedAt]
		public DateTime CreatedAt { get; set; }

		[Deleted]
		public bool Deleted { get; set; }

		#endregion

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}

	public abstract class RootEntity : IEntityId
	{

		#region Data context

		ILocalRepositoryService _localService;

		[JsonIgnore]
		[Microsoft.Practices.Unity.Dependency]
		public ILocalRepositoryService LocalDB
		{
			get { return _localService; }
			set
			{
				if (_localService != value)
				{
					_localService = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region IEntityId members

		string _id;

		[JsonProperty(Required = Required.Always)]
		public string Id
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_id))
				{
					_id = (new Guid()).ToString() + SetIdExtras();
				}
				return _id;
			}
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}

		public virtual string SetIdExtras() { return string.Empty; }

		[Version]
		public long Version { get; set; }

		[UpdatedAt]
		public DateTime UpdatedAt { get; set; }

		[CreatedAt]
		public DateTime CreatedAt { get; set; }

		[Deleted]
		public bool Deleted { get; set; }

		#endregion

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}

	public abstract class AnyContainer : IEntityId
	{

		#region Data context

		ILocalRepositoryService _localService;

		[JsonIgnore]
		[Microsoft.Practices.Unity.Dependency]
		public ILocalRepositoryService LocalDB
		{
			get { return _localService; }
			set
			{
				if (_localService != value)
				{
					_localService = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region Container implementation

		// IEntityBase<T> members:
		IEntityId _container;
		string _containerId;

		[JsonIgnore]
		public virtual bool UseContainerFullPath { get { return false; } }

		[JsonIgnore]
		public IEntityId Container
		{
			get
			{
				if (_container == null && !string.IsNullOrWhiteSpace(_containerId) && _containerId.Contains("."))
				{
					string[] seps = { "=" };
					string[] parts = _containerId.Split(seps, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length >= 2 && _localService.TableNameTypeMap.ContainsKey(parts[0]))
					{
						Type t = _localService.TableNameTypeMap[parts[0]];
						IMobileServiceSyncTable<IEntityId> table = _localService.TypeSyncTableMap[t];
						table.LookupAsync(_containerId).ContinueWith(x =>
						{
							_container = x.Result;
						});
					}
				}
				return _container;
			}
			set
			{
				if ((_container != null && value == null) || (_container == null && value != null) || (_container != null && value != null && !_container.Equals(value)))
				{
					_container = value;
					if (value != null)
					{
						_containerId = _container.GetType().Name + "=" + GetDependentContainers(_container);
					}
					OnPropertyChanged();
				}
			}
		}

		public static string GetDependentContainers(IEntityId entity)
		{
			string containerId = string.Empty;
			while (entity != null)
			{
				if (entity is IEntityBase<IEntityId>)
				{
					IEntityBase<IEntityId> baseEntity = (IEntityBase<IEntityId>)entity;
					containerId += entity.Id;
					if (baseEntity.UseContainerFullPath)
					{
						containerId += ".";
						entity = baseEntity.Container;
						continue;
					}
				}
				else if (entity is AnyContainer)
				{
					AnyContainer baseEntity = (AnyContainer)entity;
					containerId += entity.GetType().Name + "=" + entity.Id;
					if (baseEntity.UseContainerFullPath)
					{
						containerId += ".";
						entity = baseEntity.Container;
						continue;
					}
				}
				break;
			}
			return containerId;
		}

		[JsonIgnore]
		public string ContainerId
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(_containerId) && !string.IsNullOrWhiteSpace(_id) && _id.Contains(","))
				{
					string[] seps = { "," };
					string[] parts = _id.Split(seps, System.StringSplitOptions.RemoveEmptyEntries);
					_containerId = parts[0];
				}
				return _containerId;
			}
			set
			{
				if (value != _containerId)
				{
					_containerId = value;
					_container = null;
					OnPropertyChanged();
				}
			}
		}

		#endregion

		#region IEntityId members

		string _id;

		[JsonProperty(Required = Required.Always)]
		public string Id
		{
			get
			{
				// don't set the id until a container has been set
				if (string.IsNullOrWhiteSpace(_id) && !string.IsNullOrWhiteSpace(_containerId))
				{
					_id = _containerId + "," + (new Guid()).ToString() + SetIdExtras();
				}
				return _id;
			}
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}

		public virtual string SetIdExtras() { return string.Empty; }

		[Version]
		public long Version { get; set; }

		[UpdatedAt]
		public DateTime UpdatedAt { get; set; }

		[CreatedAt]
		public DateTime CreatedAt { get; set; }

		[Deleted]
		public bool Deleted { get; set; }

		#endregion

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}
}
