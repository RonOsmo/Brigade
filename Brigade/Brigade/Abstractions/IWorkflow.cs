using System;
using System.Collections.Generic;
using Brigade.Models;

namespace Brigade.Abstractions
{
	public interface IWorkflowActivity
	{
		IDictionary<string, IWorkflowUserAction> Actions { get; }
		string ActionTaken { get; }
		bool CanExecuteAction(string actionName);
		IList<string> Errors { get; }
		bool IsDone { get; }
		IWorkflowExecuteAction PreExecuteAction { get; }
		IWorkflowExecuteAction PostExecuteAction { get; }
		IDictionary<string, object> Variables { get; }
		string WorkflowActivityId { get; }
		string WorkflowProcessId { get; }
	}

	public interface IWorkflowProcess
	{
		IDictionary<string, IWorkflowActor> Actors { get; }
		string CurrentStatus { get; }
		string OwnerId { get; }
		IList<IWorkflowStatus> Statuses { get; }
		IDictionary<string, object> Variables { get; }
		string WorkflowProcessId { get; }
	}

	public interface IWorkflowStatus
	{
		IDictionary<string, IWorkflowActivity> Activities { get; }
		bool IsDone { get; }
		string Name { get; }
	}

	public interface IWorkflowUserAction : System.Windows.Input.ICommand
	{
		IList<IWorkflowActor> Actors { get; }
		string Name { get; }
	}

	public interface IWorkflowExecuteAction : System.Windows.Input.ICommand
	{
		string Name { get; }
	}

	public interface IWorkflowActor 
	{
		bool CanExecute(User user);
		string Name { get; }
		IList<string> Roles { get; }
	}

	public interface IWorkflowBuilderCanCallLevel1
	{
		IWorkflowBuilderCanCallLevel2 With(string description);
		IWorkflowBuilderCanCallLevel3 User(string user);
		IWorkflowBuilderCanCallLevel3 Role(string role);
		IWorkflowBuilderCanCallLevel3 User(User user);
		IWorkflowBuilderCanCallLevel3 Role(Role role);
	}

	public interface IWorkflowBuilderCanCallLevel2
	{
		IWorkflowBuilderCanCallLevel3 User(string user);
		IWorkflowBuilderCanCallLevel3 Role(string role);
		IWorkflowBuilderCanCallLevel3 User(User user);
		IWorkflowBuilderCanCallLevel3 Role(Role role);
	}

	public interface IWorkflowBuilderCanCallLevel3
	{
		IWorkflowBuilderCanCallLevel3 User(string user);
		IWorkflowBuilderCanCallLevel3 Role(string role);
		IWorkflowBuilderCanCallLevel3 User(User user);
		IWorkflowBuilderCanCallLevel3 Role(Role role);
		IList<UserTask> Tasks();
	}

	public interface IWorkflowBuilder : IWorkflowBuilderCanCallLevel1, IWorkflowBuilderCanCallLevel2, IWorkflowBuilderCanCallLevel3
	{
		IWorkflowBuilderCanCallLevel1 Request(IEntityId entity);
		IWorkflowBuilderCanCallLevel1 Request(IEntityId entity, DateTime scheduledTime);
	}
}