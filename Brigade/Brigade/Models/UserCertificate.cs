using System;
using System.Collections.Generic;
using Brigade.Abstractions;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Brigade.Models
{
	public class UserCertificate : EntityBase<User>
	{
		#region Certificate member

		private string _certificateId;
		private Certificate _certificate;

		public string CertificateId
		{
			get
			{
				return _certificateId;
			}
			set
			{
				if (value != _certificateId)
				{
					_certificateId = value;
					_certificate = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public Certificate Certificate
		{
			get
			{
				if (_certificate == null && !string.IsNullOrWhiteSpace(_certificateId))
				{
					LocalDB.CertificateTable.LookupAsync(_certificateId).ContinueWith(x =>
					{
						_certificate = x.Result;
					});
				}
				return _certificate;
			}
			set
			{
				if ((_certificate != null && value == null) || (_certificate == null && value != null) || (_certificate != null && value != null && !_certificate.Equals(value)))
				{
					_certificate = value;
					if (value == null)
						_certificateId = null;
					else
						_certificateId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
		public bool AssignmentApproved { get; set; }
	}
}
