using Brigade.Abstractions;
using Newtonsoft.Json;

namespace Brigade.Models
{
	public class WorkflowProcess : EntityBase<WorkflowType>
	{
		public bool IsDone { get; set; }
		public bool IsInitialised { get; set; }
		public bool IsActivated { get; set; }
		public WorkflowStatus? CurrentState { get; set; }

		#region ProcessingOnDeviceId and ProcessingOnDevice properties

		private string _processingOnDeviceId;
		private Device _processingOnDevice;

		public string ProcessingOnDeviceId
		{
			get
			{
				return _processingOnDeviceId;
			}
			set
			{
				if (value != _processingOnDeviceId)
				{
					_processingOnDeviceId = value;
					_processingOnDevice = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public Device ProcessingOnDevice
		{
			get
			{
				if (_processingOnDevice == null && !string.IsNullOrWhiteSpace(_processingOnDeviceId))
				{
					LocalDB.DeviceTable.LookupAsync(_processingOnDeviceId).ContinueWith(x =>
					{
						_processingOnDevice = x.Result;
					});
				}
				return _processingOnDevice;
			}
			set
			{
				if ((_processingOnDevice != null && value == null) || (_processingOnDevice == null && value != null) || (_processingOnDevice != null && value != null && !_processingOnDevice.Equals(value)))
				{
					_processingOnDevice = value;
					if (value == null)
						_processingOnDeviceId = null;
					else
						_processingOnDeviceId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
	}

}
