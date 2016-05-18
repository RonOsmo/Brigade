using System;
using System.Linq;
using Brigade.Abstractions;

namespace Brigade.Models
{
    public abstract class EntityBase<T> : IEntityId, IEntityBase<T> where T : IEntityId
    {
		// IEntityBase<T> member:

		public T Container { get; set; }

		// IEntityIdentifier members:

		public string Id { get; set; }

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
		public string Id { get; set; }
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

		public IEntityId Container { get; set; }

		// IEntityIdentifier members:

		public string Id { get; set; }

		public void SetId(string identifier)
		{
			if (Container == null)
				throw new System.ArgumentNullException(nameof(Container));
			if (string.IsNullOrWhiteSpace(identifier))
				throw new System.ArgumentNullException(nameof(identifier));
			Id = Container.Id + "." + identifier;
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

		public virtual void SetId()
		{
			if (Container == null)
				throw new System.ArgumentNullException(nameof(Container));
			Id = Container.Id;
		}

	}
}
