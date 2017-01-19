using System.Collections.Generic;
using Brigade.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Brigade.Models
{
	public class Certificate : EntityBase<Authority>
	{
		public string Description { get; set; }

		public Task<List<UserCertificate>> GetUserCertificates()
		{
			var query =
				LocalDB.UserCertificateTable.CreateQuery()
				.Where(uc => uc.CertificateId == this.Id)
				.OrderBy(uc => uc.Container.UserId)
				.ToListAsync();
			return query;
		}
	}
}
