using System.Collections.Generic;
using Brigade.Models;
using System.Threading.Tasks;

namespace Brigade.Abstractions
{
	public interface IAssetStockService
    {
        Task<IEnumerable<Asset>> GetAssetsAsync(Asset parent);
        Task LockAsync(Asset parent);
        Task SaveAsync(Asset parent);
		Task SaveAsync(IEnumerable<Asset> children);
    }

    public interface IAdminAssetService
    {

    }
}
