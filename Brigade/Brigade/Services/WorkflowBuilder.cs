using System;
using System.Linq;
using System.Collections.Generic;
using Brigade.Abstractions;
using Brigade.Models;
using System.Threading.Tasks;

namespace Brigade.Services
{
	public class WorkflowBuilder : IWorkflowBuilder
	{
		// services (handled by IOC) :-
		ILoginService _loginService;
		ILocalRepositoryService _localService;

		// private members :-
		User _currentUser = null;
		string _description = "";
		DateTime? _when;
		IEntityId _entity = null;
		Dictionary<string, User> _targetUsers = new Dictionary<string, User>();

		public WorkflowBuilder(ILoginService loginService, ILocalRepositoryService localService)
		{
			_loginService = loginService;
			_currentUser = loginService.CurrentUser;
			_localService = localService;
		}

		void FormulateDescription(IEntityId entity)
		{
			string temp;
			switch (entity.GetType().Name)
			{
				default:
					temp = $"{entity.GetType().Name}: '{entity.Id}'";
					break;

				case "Event":
					Event e = (Event)entity;
					temp = $"Event: {e.Name}";
					break;

				case "EventRole":
					EventRole er = (EventRole)entity;
					temp = $"Invitation: Event: '{er.Container.Name}' in Role:{er.Name}";
					break;

				case "EventType":
					EventType et = (EventType)entity;
					temp = $"Availability: {et.Name}";
					break;

				case "UserRole":
					UserRole ur = (UserRole)entity;
					temp = $"Approve: Role '{ur.Role.Name}' for User: '{ur.Container.UserId}'";
					break;

				case "UserCertificate":
					UserCertificate uc = (UserCertificate)entity;
					temp = $"Approve: Certificate '{uc.Certificate.Name}' for User: '{uc.Container.UserId}'";
					break;
			}
			_description = temp;
		}

		public IWorkflowBuilderCanCallLevel1 Request(IEntityId entity)
		{
			_entity = entity;
			FormulateDescription(entity);
			return (IWorkflowBuilderCanCallLevel1)this;
		}
		public IWorkflowBuilderCanCallLevel1 Request(IEntityId entity, DateTime when)
		{
			_entity = entity;
			_when = when;
			FormulateDescription(entity);
			return (IWorkflowBuilderCanCallLevel1)this;
		}

		public IWorkflowBuilderCanCallLevel2 With(string fullDescription)
		{
			_description = fullDescription;
			return (IWorkflowBuilderCanCallLevel2)this;
		}

		public IWorkflowBuilderCanCallLevel3 User(string user)
		{
			var query = _localService.UserTable.CreateQuery()
				.Where(u => u.UserId == user)
				.ToEnumerableAsync()
				.ContinueWith(users =>
				{
					foreach (User u in users.Result)
					{
						_targetUsers.Add(u.Id, u);
					}
				});

			return (IWorkflowBuilderCanCallLevel3)this;
		}

		public IWorkflowBuilderCanCallLevel3 Role(string role)
		{
			if (!role.Contains("."))
			{
				role = _currentUser.Container.Id + "." + role;
			}

			_localService
				.UserRoleTable
				.Where(ur => ur.Role.Name == role)
				.ToEnumerableAsync()
				.ContinueWith(users =>
				{
					foreach (var user in users.Result)
					{
						User(user.Container);
					}
				});

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
			role.GetUserRolesAsync().ContinueWith(roles =>
			{
				foreach (UserRole userRole in roles.Result)
				{
					User(userRole.Container);
				}
			});
			return (IWorkflowBuilderCanCallLevel3)this;
		}

		public IList<UserTask> Tasks()
		{
			List<UserTask> userReminders = new List<UserTask>();
			foreach (User user in _targetUsers.Values)
			{
				// just a reminder - there is no workflow record required
				UserTask newRequest = new UserTask { ActualDate = _when, Container = user, Description = _description, Owner = _entity, SentByUser = _currentUser };
				userReminders.Add(newRequest);
			}
			return (IList<UserTask>)userReminders;
		}
	}
}
