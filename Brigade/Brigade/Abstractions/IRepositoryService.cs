using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;
using Brigade;
using System;

namespace Brigade.Abstractions
{
	public interface ILocalRepositoryService
	{
		Task InitializeAsync();

		Dictionary<System.Type, IMobileServiceSyncTable<IEntityId>> TypeSyncTableMap { get; }
		Dictionary<string, System.Type> TableNameTypeMap { get; }
		Task<bool> PushAsync();
		IMobileServiceSyncTable<Models.Asset> AssetTable { get; }
		IMobileServiceSyncTable<Models.Authority> AuthorityTable { get; }
		IMobileServiceSyncTable<Models.Availability> AvailabilityTable { get; }
		IMobileServiceSyncTable<Models.Brigade> BrigadeTable { get; }
		IMobileServiceSyncTable<Models.Certificate> CertificateTable { get; }
		IMobileServiceSyncTable<Models.Device> DeviceTable { get; }
		IMobileServiceSyncTable<Models.Event> EventTable { get; }
		IMobileServiceSyncTable<Models.EventAttendee> AttendeeTable { get; }
		IMobileServiceSyncTable<Models.EventType> EventTypeTable { get; }
		IMobileServiceSyncTable<Models.EventRole> EventRoleTable { get; }
		IMobileServiceSyncTable<Models.Role> RoleTable { get; }
		IMobileServiceSyncTable<Models.TrainingPrerequisite> TrainingPrerequisiteTable { get; }
		IMobileServiceSyncTable<Models.User> UserTable { get; }
		IMobileServiceSyncTable<Models.UserCertificate> UserCertificateTable { get; }
		IMobileServiceSyncTable<Models.UserRole> UserRoleTable { get; }
		IMobileServiceSyncTable<Models.UserTask> UserTaskTable { get; }
		IMobileServiceSyncTable<Models.WorkflowActivity> WorkflowActivityTable { get; }
		IMobileServiceSyncTable<Models.WorkflowProcess> WorkflowProcessTable { get; }
		IMobileServiceSyncTable<Models.WorkflowType> WorkflowTypeTable { get; }

	}

	public interface IRepository<T> where T : IEntityId
	{
		IMobileServiceTableQuery<T> SetSyncQuery { get; set; }
		IMobileServiceSyncTable<T> Table { get; }
		IMobileServiceTableQuery<T> CreateQuery();
		Task DeleteAsync(T instance);
		Task InsertAsync(T instance);
		Task<T> LookupAsync(string id);
		Task SynchAsync();
		Task UpdateAsync(T instance);
	}
}
