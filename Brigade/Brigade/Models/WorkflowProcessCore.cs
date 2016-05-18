using System;
using System.Collections.Generic;
using Brigade.Abstractions;

namespace Brigade.Models
{
	public class WorkflowActivity<T> : IWorkflowActivity where T : IViewModel
	{
		public virtual string Name { get; set; }
		public virtual string TypeName { get { return "Activity"; } }
		public virtual bool IsDone { get; set; }
		public virtual bool IsReady
		{
			get
			{
				if (IsInitialised && IsActivated)
				{
					return Validate(ValidationType.Activate);
				}
				return false;
			}
		}
		public virtual bool IsInitialised { get; set; }
		public virtual bool IsActivated { get; set; }
		public IList<string> Errors
		{
			get
			{
				return (IList<string>)_errors;
			}
		}
		public T Owner { get; set; }
		public virtual void Initialise()
		{
			IsInitialised = true;
		}
		public virtual void Activate()
		{
			ActivateInternal();

			if (Validate(ValidationType.Activate))
			{
				IsActivated = true;
			}
		}
		public virtual void ActivateInternal() { }
		public virtual void Process()
		{
			if (!IsInitialised)
				Initialise();
			if (!IsActivated)
				Activate();

			if (Validate(ValidationType.Process))
			{
				ProcessInternal();
			}
		}
		public virtual void ProcessInternal() { }
		public bool Validate(ValidationType validationType)
		{
			ValidateInternal(validationType);
			return (_errors.Count == 0);
		}
		public void ValidateInternal(ValidationType validationType) { }

		List<string> _errors = new List<string>();
	}

	public class WorkflowState<T> : WorkflowActivity<T>, IWorkflowActivity where T : IViewModel
	{
		public override string TypeName { get { return "State"; } }
		public bool IsFinal { get; set; }
		public bool IsInitial { get; set; }
	}

	public class WorkflowProcessStateMachine<T> : WorkflowActivity<T>, IWorkflowActivity, IWorkflowProcess where T : IViewModel
	{
		public override string TypeName { get { return "StateMachine"; } }
		public override bool IsDone
		{
			get
			{
				foreach (var state in _states.Values)
				{
					if (state.IsDone && state.IsFinal)
						return true;
				}
				return false;
			}
		}
		public IList<IWorkflowActivity> Activities { get { return (IList<IWorkflowActivity>)_states; } }
		public WorkflowStatus CurrentState { get; set; }

		Dictionary<string, WorkflowState<T>> _states = new Dictionary<string, WorkflowState<T>>();
	}

	public abstract class WorkflowProcessBase<T> : WorkflowActivity<T>, IWorkflowActivity, IWorkflowProcess where T : IViewModel
	{
		protected List<WorkflowActivity<T>> _states = new List<WorkflowActivity<T>>();

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
		public virtual IList<IWorkflowActivity> Activities { get { return (IList<IWorkflowActivity>)_states; } }
		public WorkflowStatus CurrentState { get; set; }
	}

	public class WorkflowSequential<T> : WorkflowProcessBase<T>, IWorkflowActivity, IWorkflowProcess where T : IViewModel
	{
		public override string TypeName { get { return "Sequential"; } }
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
	}

	public class WorkflowParallelAny<T> : WorkflowProcessBase<T>, IWorkflowActivity, IWorkflowProcess where T : IViewModel
	{
		public override string TypeName { get { return "ParallelAny"; } }
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
	}

	public class WorkflowParallel<T> : WorkflowProcessBase<T>, IWorkflowActivity, IWorkflowProcess where T : IViewModel
	{
		public override string TypeName { get { return "Parallel"; } }
	}

	public class WorkflowProcess 
    {
		public IWorkflowActivity Task { get; set; }
	}

	public class WorkflowBuilder : IWorkflowBuilder
	{
		// services (handled by Autofac):-
		ILoginService _loginService;
		IRepositoryService<User> _userService;
		IRepositoryService<Role> _roleService;

		// private members :-
		User _currentUser = null;
		string _description = "";
		DateTime _when = DateTime.Now;
		IEntityId _entity = null;
		Dictionary<string, User> _targetUsers = new Dictionary<string, User>();

		public WorkflowBuilder(ILoginService loginService, IRepositoryService<User> userService, IRepositoryService<Role> roleService)
		{
			_loginService = loginService;
			_currentUser = loginService.CurrentUser;
			_userService = userService;
			_roleService = roleService;
		}

		public IWorkflowBuilderCanCallLevel1 Request(IEntityId entity, string taskDescription, DateTime when)
		{
			_entity = entity;
			_when = when;
			_description = $"{_currentUser.UserId} requests that you '{taskDescription}' {when.ToString()}"; 
			return (IWorkflowBuilderCanCallLevel1)this;
		}

		public IWorkflowBuilderCanCallLevel2 With(string fullDescription)
		{
			_description = fullDescription;
			return (IWorkflowBuilderCanCallLevel2)this;
		}


		public IWorkflowBuilderCanCallLevel3 User(string user)
		{
			if (!user.Contains("."))
			{
				user = _currentUser.Container.Id + "." + user;
			}
			
			User(_userService.GetById(user));

			return (IWorkflowBuilderCanCallLevel3)this;
		}

		public IWorkflowBuilderCanCallLevel3 Role(string role)
		{
			if (!role.Contains("."))
			{
				role = _currentUser.Container.Id + "." + role;
			}

			Role(_roleService.GetById(role));

			return (IWorkflowBuilderCanCallLevel3)this;
		}

		public IWorkflowBuilderCanCallLevel3 User(User user)
		{
			if (!_targetUsers.ContainsKey(user.Id))
			{
				_targetUsers.Add(user.Id, user);
			}
			return (IWorkflowBuilderCanCallLevel3)this;
		}

		public IWorkflowBuilderCanCallLevel3 Role(Role role)
		{
			foreach (UserRole userRole in role.Users)
			{
				User(userRole.Container);
			}
			return (IWorkflowBuilderCanCallLevel3)this;
		}

		public IList<UserTask> Requests()
		{
			List<UserTask> userReminders = new List<UserTask>();
			foreach (var user in _targetUsers)
			{
				UserTask newRequest = new UserTask { Container = _currentUser, Owner = _entity, Description = _description, ActualDate = _when };
				newRequest.SetId();
			}
			return (IList<UserTask>)userReminders;
		}
	}
}
