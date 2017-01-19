using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Prism.Mvvm;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Brigade.Abstractions;
using Brigade.Models;
using Brigade.Services;
using System.ComponentModel;

namespace Brigade.Services
{
	[JsonObject(MemberSerialization.Fields)]
	public abstract class BaseActivity : IWorkflowActivity 
	{
		public IDictionary<string, IWorkflowUserAction> Actions
		{
			get { return (IDictionary<string, IWorkflowUserAction>)_actions; }
		}
		public virtual string ActionTaken { get; set; }
		public BaseActivity()
		{
			PreExecuteAction = new PreExecuteAction(this);
			PostExecuteAction = new PostExecuteAction(this);
		}
		public virtual bool CanExecuteAction(string actionName)
		{
			switch (actionName)
			{
				case "PreExecute":
					return string.IsNullOrEmpty(ActionTaken) && !_isPreExecuteDone;
				case "PostExecute":
					return !string.IsNullOrEmpty(ActionTaken) && !_isPostExecuteDone;
			}
			return true;
		}
		public IList<string> Errors
		{
			get { return (IList<string>)_errors; }
		}
		public virtual bool IsDone { get; set; }
		public virtual bool IsPreExecuteDone
		{
			get { return _isPreExecuteDone; }
			set { _isPreExecuteDone = value; }
		}
		public virtual bool IsPostExecuteDone
		{
			get { return _isPostExecuteDone; }
			set { _isPostExecuteDone = value; }
		}
		public virtual IWorkflowExecuteAction PreExecuteAction { get; set; }
		public virtual IWorkflowExecuteAction PostExecuteAction { get; set; }
		public IDictionary<string, object> Variables
		{
			get { return (IDictionary<string, object>)_variables; }
		}
		public virtual string WorkflowActivityId { get; set; }
		public virtual string WorkflowProcessId { get; set; }

		#region Fields
		Dictionary<string, System.Windows.Input.ICommand> _actions = new Dictionary<string, System.Windows.Input.ICommand>();
		List<string> _errors = new List<string>();
		bool _isPreExecuteDone;
		bool _isPostExecuteDone;
		Dictionary<string, object> _variables = new Dictionary<string, object>();
		#endregion
	}

	public class PreExecuteAction : IWorkflowExecuteAction
	{
		public PreExecuteAction(BaseActivity activity)
		{
			_activity = activity;
			_actionName = "PreExecute";
		}
		public bool CanExecute(object parameter)
		{
			return _activity.CanExecuteAction(_actionName);
		}
		public event EventHandler CanExecuteChanged;
		public virtual void Execute(object parameter)
		{
			_activity.IsPreExecuteDone = true;
		}
		public string Name { get { return _actionName; } }

		#region Fields
		IList<IWorkflowActor> _actors = new List<IWorkflowActor>();
		BaseActivity _activity;
		string _actionName;
		#endregion
	}

	public class PostExecuteAction : IWorkflowExecuteAction
	{
		public PostExecuteAction(BaseActivity activity)
		{
			_activity = activity;
			_actionName = "PostExecute";
		}
		public bool CanExecute(object parameter)
		{
			return _activity.CanExecuteAction(_actionName);
		}
		public event EventHandler CanExecuteChanged;
		public virtual void Execute(object parameter)
		{
			_activity.IsPostExecuteDone = true;
		}
		public string Name { get { return _actionName; } }

		#region Fields
		IList<IWorkflowActor> _actors = new List<IWorkflowActor>();
		BaseActivity _activity;
		string _actionName;
		#endregion
	}

	public class UserAction : IWorkflowUserAction
	{
		public IList<IWorkflowActor> Actors { get { return _actors; } }
		public UserAction(BaseActivity activity, string actionName)
		{
			_activity = activity;
			_actionName = actionName;
		}
		public bool CanExecute(object parameter)
		{
			bool ok = true;
			Brigade.Models.User user = parameter as Brigade.Models.User;
			if (user != null)
			{
				foreach (var actor in _actors)
				{
					ok = actor.CanExecute(user);
					if (ok)
						break;
				}
			}
			return ok;
		}
		public event EventHandler CanExecuteChanged;
		public virtual void Execute(object parameter)
		{
			_activity.ActionTaken = _actionName;
			_activity.IsDone = true;
		}
		public string Name { get { return _actionName; } }

		#region Fields
		IList<IWorkflowActor> _actors = new List<IWorkflowActor>();
		BaseActivity _activity;
		string _actionName;
		#endregion
	}

	public class AcknowledgeActivity : BaseActivity
	{
		public AcknowledgeActivity()
		{
			Actions[AcknowledgeActionName] = new UserAction(this, AcknowledgeActionName);
		}

		public const string AcknowledgeActionName = "Acknowledge";
	}

	public class AcceptRejectActivity : BaseActivity
	{
		public AcceptRejectActivity()
		{
			Actions[AcceptActionName] = new UserAction(this, AcceptActionName);
			Actions[RejectActionName] = new UserAction(this, RejectActionName);
		}

		public const string AcceptActionName = "Accept";
		public const string RejectActionName = "Reject";
	}

	public class BaseActor : IWorkflowActor
	{
		public BaseActor(BaseActivity activity, string actorName)
		{
			_activity = activity;
			_actionName = actorName;
		}
		public bool CanExecute(User user)
		{
			bool ok = true;

			if (_roles.Count > 0)
			{
				user.GetUserRolesAsync()
					.ContinueWith(x =>
					{
						foreach (var ur in x.Result)
						{
							ok = false;
							foreach (string role in _roles)
							{
								ok = (role == ur.Role.Name);
								if (ok) break;
							}
							if (ok) break;
						}
					});
			}
			return ok;
		}
		public string Name { get { return _actionName; } }
		public IList<string> Roles { get { return _roles; } }

		#region Fields
		IList<string> _roles = new List<string>();
		BaseActivity _activity;
		string _actionName;
		#endregion

	}

	public abstract class WorkflowProcessBase<T> : IWorkflowProcess, System.ComponentModel.INotifyPropertyChanged where T : IEntityId
	{
		public WorkflowProcessBase(ILocalRepositoryService localService)
		{
			_localService = localService;
			_variables.PropertyChanged += OnPropertyChanged;
			_variables.CollectionChanged += OnCollectionChanged;
		}
		public string CurrentStatus { get; }
		void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			_modified = true;
		}
		void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string propertyName = "")
		{
			_modified = true;
			PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
			PropertyChanged?.Invoke(this, e);
		}
		void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_modified = true;
			PropertyChangedEventArgs newE = e;
			if (e != null)
			{
				newE = new PropertyChangedEventArgs("Variables" + "." + e.PropertyName);
			}
			PropertyChanged?.Invoke(sender, newE);
		}
		public T Owner
		{
			get
			{
				if (_owner == null && !string.IsNullOrWhiteSpace(_ownerId))
				{
					if (_localService.TypeSyncTableMap.ContainsKey(typeof(T)))
					{
						IMobileServiceSyncTable<T> table = (IMobileServiceSyncTable<T>)_localService.TypeSyncTableMap[typeof(T)];
						table.LookupAsync(_ownerId).ContinueWith(x =>
						{
							_owner = x.Result;
						});
					}
				}
				return _owner;
			}
			set
			{
				if ((_owner != null && value == null) || (_owner == null && value != null) || (_owner != null && value != null && !_owner.Equals(value)))
				{
					_owner = value;
					_ownerId = AnyContainer.GetDependentContainers(_owner);
					OnPropertyChanged();
				}
			}
		}
		public string OwnerId
		{
			get
			{
				return _ownerId;
			}
			set
			{
				if (value != _ownerId)
				{
					_ownerId = value;
					_owner = default(T);
					OnPropertyChanged();
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		public IList<IWorkflowStatus> Statuses
		{
			get { return _statuses; }
			set
			{
				if (_statuses != value)
				{
					_statuses = value;
					OnPropertyChanged();
				}
			}
		}
		public IDictionary<string, object> Variables { get { return _variables; } }
		public string WorkflowProcessId { get; }

		#region Fields
		[JsonIgnore]
		T _owner;
		string _ownerId;
		[JsonIgnore]
		ILocalRepositoryService _localService;
		bool _modified;
		IList<IWorkflowStatus> _statuses = new List<IWorkflowStatus>();
		ObservableDictionary<string, object> _variables = new ObservableDictionary<string, object>();
		#endregion
	}

	public class Sequential : IWorkflowStatus
	{

	}
	public class WorkflowSequential<T> : WorkflowProcessBase<T>, IWorkflowActivity, IWorkflowProcess where T : IEntityId
	{
		public override bool IsDone
		{
			get
			{
				foreach (var state in _states)
				{
					if (!state.IsDone)
						return false;
				}
				return true;
			}
		}
		public override void ProcessInternal()
		{
			foreach (var state in _states)
			{
				if (!state.IsDone)
					state.Process();
				if (!state.IsDone)
					break;
			}
			if (IsDone)
				base.ProcessInternal();
		}
	}

	public class WorkflowParallelAny<T> : WorkflowProcessBase<T>, IWorkflowActivity, IWorkflowProcess where T : BindableBase
	{
		public override bool IsDone
		{
			get
			{
				foreach (var state in _states)
				{
					if (state.IsDone)
						return true;
				}
				return false;
			}
		}
		public override void ProcessInternal()
		{
			foreach (var state in _states)
			{
				if (!state.IsDone)
					state.Process();
				if (state.IsDone)
					break;
			}
			if (IsDone)
				base.ProcessInternal();
		}
	}

	public class WorkflowParallel<T> : WorkflowProcessBase<T>, IWorkflowActivity, IWorkflowProcess where T : BindableBase
	{
	}

	public class WorkflowEngine
	{
		[Dependency]
		ILocalRepositoryService LocalDB { get; set; }

		[Dependency]
		ILoginService Login { get; set; }

		System.Collections.Generic.Dictionary<string, UserTask> taskList = new Dictionary<string, UserTask>();
		System.Collections.Generic.Dictionary<string, WorkflowActivity> itemList = new Dictionary<string, WorkflowActivity>();
		System.Collections.Generic.Dictionary<string, WorkflowProcess> processList = new Dictionary<string, WorkflowProcess>();

		async Task CollectWorkflowInfoAsync()
		{
			taskList.Clear();
			itemList.Clear();
			processList.Clear();

			string userId = Login.CurrentUser.Id;

			var queryTasks = LocalDB.UserTaskTable.CreateQuery()
				.Where(ut => ut.ContainerId == userId)
				.ToEnumerableAsync()
				.ContinueWith(ut =>
				{
					foreach (var userTask in ut.Result)
					{
						taskList[userTask.Id] = userTask;
					}
				});
			await queryTasks;

			var queryItems = LocalDB.WorkflowActivityTable.CreateQuery()
				.ToEnumerableAsync()
				.ContinueWith(tItems =>
				{
					var results = tItems.Result
						.Where(wi => taskList.ContainsKey(wi.UserTaskId) && !wi.Container.IsDone);

					foreach (var item in results)
					{
						itemList[item.Id] = item;
						processList[item.ContainerId] = item.Container;
					}
				});
			await queryItems;

			var missingItems = LocalDB.WorkflowActivityTable.CreateQuery()
				.Where(wi => processList.ContainsKey(wi.ContainerId) && !itemList.ContainsKey(wi.Id))
				.ToEnumerableAsync()
				.ContinueWith(tItems =>
				{
					foreach (var item in tItems.Result)
					{
						itemList[item.Id] = item;
					}
				});
		}


		async Task<bool> LockProcessesAsync()
		{
			string deviceId = Login.CurrentDevice.Id;
			string userId = Login.CurrentUser.Id;
			var tasks = new List<Task>();

			foreach (var process in processList.Values)
			{
				if (process.ProcessingOnDeviceId == null)
				{
					process.ProcessingOnDeviceId = deviceId;
					tasks.Add(LocalDB.WorkflowProcessTable.UpdateAsync(process));
				}
			}

			foreach (var item in itemList.Values)
			{
				if (!item.IsDone && item.ProcessingOnDeviceId == null)
				{
					item.ProcessingOnDeviceId = deviceId;
					tasks.Add(LocalDB.WorkflowActivityTable.UpdateAsync(item));
				}
			}

			try
			{
				Task.WaitAll(tasks.ToArray());
				bool ok = await LocalDB.PushAsync();
				return ok;
			}
			catch (AggregateException ag)
			{
				foreach (var e in ag.InnerExceptions)
				{

				}
			}
			return false;
		}

		bool InstantiateProcesses()
		{
			string deviceId = Login.CurrentDevice.Id;
			foreach (var process in processList.Values.Where(p => !p.IsDone && p.ProcessingOnDeviceId == deviceId))
			{
				string jsonProcessRepresentation = process.Container.WorkflowDefinitionJson;
				var instantiatedProcess = JsonConvert.DeserializeObject(jsonProcessRepresentation, new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.Auto
				});
			}
			return true;
		}

		async Task<bool> UnLockProcessesAsync()
		{
			string deviceId = Login.CurrentDevice.Id;
			string userId = Login.CurrentUser.Id;
			var tasks = new List<Task>(); 

			foreach (var process in processList.Values.Where(p => !p.Deleted && p.ProcessingOnDeviceId == deviceId))
			{
				process.ProcessingOnDeviceId = null;
				tasks.Add(LocalDB.WorkflowProcessTable.UpdateAsync(process));
				if (process.IsDone)
				{
					tasks.Add(LocalDB.WorkflowProcessTable.DeleteAsync(process));
				}
			}

			foreach (var item in itemList.Values.Where(i => !i.Deleted && i.ProcessingOnDeviceId == deviceId))
			{
				item.ProcessingOnDeviceId = null;
				tasks.Add(LocalDB.WorkflowActivityTable.UpdateAsync(item));
				if (item.IsDone)
				{
					tasks.Add(LocalDB.WorkflowActivityTable.DeleteAsync(item));
				}
			}

			try
			{
				Task.WaitAll(tasks.ToArray());
				bool ok = await LocalDB.PushAsync();
				return ok;
			}
			catch (AggregateException ag)
			{
				foreach (var e in ag.InnerExceptions)
				{

				}
			}
			return false;
		}
	}
}
