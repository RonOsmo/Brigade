using System;
using System.Linq;
using Brigade.Abstractions;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Brigade.Models
{
    public abstract class EntityBase<T> : IEntityId, IEntityBase<T> where T : IEntityId
    {
		// IEntityBase<T> member:
		T _container;
		public T Container
		{
			get { return _container; }
			set
			{
				if ((_container != null && value == null) || (_container == null && value != null) || (_container != null && value != null && !_container.Equals(value)))
				{
					_container = value;
					OnPropertyChanged();
				}
			}
		}

		// IEntityId members:
		string _id;
		public string Id
		{
			get { return _id; }
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void SetId(string identifier)
        {
            if (Container == null)
                throw new System.ArgumentNullException(nameof(Container));
            if (string.IsNullOrWhiteSpace(identifier))
                throw new System.ArgumentNullException(nameof(identifier));
            Id = Container.Id + "." + identifier;
        }

        public virtual void SetId()
        {
            if (Container == null)
                throw new System.ArgumentNullException(nameof(Container));
            Id = Container.Id;
        }
	}

	public abstract class RootEntity : IEntityId
	{
		string _id;
		public string Id
		{
			get { return _id; }
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public void SetId(string identifier)
		{
			Id = identifier;
		}
		public virtual void SetId()
		{
			throw new System.NotImplementedException(nameof(SetId));
		}
	}

	public abstract class AnyContainer : IEntityId
	{
		// IEntityBase<T> member:
		IEntityId _container;
		public IEntityId Container
		{
			get { return _container; }
			set
			{
				if ((_container != null && value == null) || (_container == null && value != null) || (_container != null && value != null && !_container.Equals(value)))
				{
					_container = value;
					OnPropertyChanged();
				}
			}
		}

		// IEntityIdentifier members:
		string _id;
		public string Id
		{
			get { return _id; }
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void SetId(string identifier)
		{
			if (Container == null)
				throw new System.ArgumentNullException(nameof(Container));
			if (string.IsNullOrWhiteSpace(identifier))
				throw new System.ArgumentNullException(nameof(identifier));
			Id = Container.Id + "." + identifier;
		}

		public virtual void SetId()
		{
			if (Container == null)
				throw new System.ArgumentNullException(nameof(Container));
			Id = Container.Id;
		}

		public string GetLastPart(int numberOfParts = 1)
		{
			char[] splitChars = { '.' };
			string[] parts = Id.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > numberOfParts)
			{
				return "." + string.Join(".", parts.Skip(parts.Length - numberOfParts).Take(numberOfParts));
			}
			return "." + Id;
		}

	}
}
