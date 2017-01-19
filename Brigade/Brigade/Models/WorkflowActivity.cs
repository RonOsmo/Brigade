using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Brigade.Abstractions;

namespace Brigade.Models
{
    public class WorkflowActivity : EntityBase<WorkflowProcess>
    {
		public bool IsDone { get; set; }
		#region UserTaskId and UserTask properties

		private string _userTaskId;
		private UserTask _userTask;

		public string UserTaskId
		{
			get
			{
				return _userTaskId;
			}
			set
			{
				if (value != _userTaskId)
				{
					_userTaskId = value;
					_userTask = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public UserTask UserTask
		{
			get
			{
				if (_userTask == null && !string.IsNullOrWhiteSpace(_userTaskId))
				{
					LocalDB.UserTaskTable.LookupAsync(_userTaskId).ContinueWith(x =>
					{
						_userTask = x.Result;
					});
				}
				return _userTask;
			}
			set
			{
				if ((_userTask != null && value == null) || (_userTask == null && value != null) || (_userTask != null && value != null && !_userTask.Equals(value)))
				{
					_userTask = value;
					if (value == null)
						_userTaskId = null;
					else
						_userTaskId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
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
