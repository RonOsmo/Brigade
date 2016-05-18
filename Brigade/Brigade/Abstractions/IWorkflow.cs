using System;
using System.Collections.Generic;
using Brigade.Models;

namespace Brigade.Abstractions
{
	public interface IWorkflowActivity
	{
		string Name { get; }
		string TypeName { get; }
		bool IsDone { get; }
		bool IsInitialised { get; }
		bool IsActivated { get; set; }
		bool IsReady { get; }
		IList<string> Errors { get; }
		void Initialise();
		void Activate();
		void Process();
		bool Validate(ValidationType validationType);
	}

	public enum ValidationType
	{
		Activate = 1,
		Process = 2,
	}

	public enum WorkflowStatus
	{
		New = 0,
		Executing = 1,
		Suspended = 2,
		Finished = 3,
		CleaningUp = 4,
		Failed = 5,
	}

	public interface IWorkflowTask : IWorkflowActivity
	{
		IDictionary<string, Action> Actions { get; }
	}

	public interface IWorkflowProcess : IWorkflowActivity
	{
		IList<IWorkflowActivity> Activities { get; }
		WorkflowStatus CurrentState { get; }
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
		IList<UserTask> Requests();
	}

	public interface IWorkflowBuilder : IWorkflowBuilderCanCallLevel1, IWorkflowBuilderCanCallLevel2, IWorkflowBuilderCanCallLevel3
	{
		IWorkflowBuilderCanCallLevel1 Request(IEntityId entity, string taskDescription, DateTime scheduledTime);
	}
}