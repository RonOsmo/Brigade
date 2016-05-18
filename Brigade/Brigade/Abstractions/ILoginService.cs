using Brigade.Models;

namespace Brigade.Abstractions
{
	public interface ILoginService
    {
		Models.Brigade CurrentBrigade { get; }
		User CurrentUser { get; }
		Device CurrentDevice { get; }
		string StorageConnectionString { get; }
    }
}
