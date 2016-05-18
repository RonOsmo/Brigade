using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brigade.Abstractions
{
	public interface IRepositoryService<T> 
    {
        Task<T> GetByIdAsync(string id);
        Task<IEnumerable<T>> ListAsync();
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveAsync(T entity);
		Task SaveAsync(IEnumerable<T> entities);
    }
}
