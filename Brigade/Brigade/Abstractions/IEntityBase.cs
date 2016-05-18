namespace Brigade.Abstractions
{
	public interface IEntityBase<T>
    {
		T Container { get; set; }
	}

	public interface IEntityId
	{
		string Id { get; set; }
		void SetId(string identifier);
		void SetId();
	}
}
