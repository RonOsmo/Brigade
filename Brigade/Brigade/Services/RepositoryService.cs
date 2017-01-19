using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Brigade.Abstractions;
using System;

namespace Brigade.Services
{
	/// <summary>
	/// implementation of a repository service that uses
	/// the Azure MobileServices SDK for syncing with a 
	/// local SqLite repository to Azure Storage in the cloud.
	/// </summary>
	public class RepositoryService : ILocalRepositoryService
	{
		private IMobileServiceClient _mobileService;
		public RepositoryService(IMobileServiceClient mobileService)
		{
			_mobileService = mobileService;
		}

		public async Task InitializeAsync()
		{
			if (!_mobileService.SyncContext.IsInitialized)
			{
				var store = new MobileServiceSQLiteStore("Brigade.db");

				CreateLocalTables(store);

				// Initialize file sync
				// _mobileService.InitializeFileSyncContext(new FileSyncHandler(this), store, new FileSyncTriggerFactory(_mobileService, true));

				// Uses the default conflict handler, which fails on conflict
				await _mobileService.SyncContext.InitializeAsync(store, StoreTrackingOptions.NotifyLocalAndServerOperations);
			}

			GetLocalTables();
		}

		public async Task<bool> PushAsync()
		{
			await _mobileService.SyncContext
				.PushAsync()
				.ContinueWith(t =>
				{
					return true;
				});
			return false;
		}

		private void GetLocalTables()
		{
			AssetTable = _mobileService.GetSyncTable<Models.Asset>();
			AddEntry(typeof(Models.Asset), (IMobileServiceSyncTable<IEntityId>)AssetTable);

			AttendeeTable = _mobileService.GetSyncTable<Models.EventAttendee>();
			AddEntry(typeof(Models.EventAttendee), (IMobileServiceSyncTable<IEntityId>)AttendeeTable);

			AuthorityTable = _mobileService.GetSyncTable<Models.Authority>();
			AddEntry(typeof(Models.Authority), (IMobileServiceSyncTable<IEntityId>)AuthorityTable);

			AvailabilityTable = _mobileService.GetSyncTable<Models.Availability>();
			AddEntry(typeof(Models.Availability), (IMobileServiceSyncTable<IEntityId>)AvailabilityTable);

			BrigadeTable = _mobileService.GetSyncTable<Models.Brigade>();
			AddEntry(typeof(Models.Brigade), (IMobileServiceSyncTable<IEntityId>)BrigadeTable);

			CertificateTable = _mobileService.GetSyncTable<Models.Certificate>();
			AddEntry(typeof(Models.Certificate), (IMobileServiceSyncTable<IEntityId>)CertificateTable);

			DeviceTable = _mobileService.GetSyncTable<Models.Device>();
			AddEntry(typeof(Models.Device), (IMobileServiceSyncTable<IEntityId>)DeviceTable);

			EventTable = _mobileService.GetSyncTable<Models.Event>();
			AddEntry(typeof(Models.Event), (IMobileServiceSyncTable<IEntityId>)EventTable);

			EventTypeTable = _mobileService.GetSyncTable<Models.EventType>();
			AddEntry(typeof(Models.EventType), (IMobileServiceSyncTable<IEntityId>)EventTypeTable);

			EventRoleTable = _mobileService.GetSyncTable<Models.EventRole>();
			AddEntry(typeof(Models.Asset), (IMobileServiceSyncTable<IEntityId>)EventRoleTable);

			RoleTable = _mobileService.GetSyncTable<Models.Role>();
			AddEntry(typeof(Models.Role), (IMobileServiceSyncTable<IEntityId>)RoleTable);

			TrainingPrerequisiteTable = _mobileService.GetSyncTable<Models.TrainingPrerequisite>();
			AddEntry(typeof(Models.TrainingPrerequisite), (IMobileServiceSyncTable<IEntityId>)TrainingPrerequisiteTable);

			UserTable = _mobileService.GetSyncTable<Models.User>();
			AddEntry(typeof(Models.User), (IMobileServiceSyncTable<IEntityId>)UserTable);

			UserCertificateTable = _mobileService.GetSyncTable<Models.UserCertificate>();
			AddEntry(typeof(Models.UserCertificate), (IMobileServiceSyncTable<IEntityId>)UserCertificateTable);

			UserRoleTable = _mobileService.GetSyncTable<Models.UserRole>();
			AddEntry(typeof(Models.UserRole), (IMobileServiceSyncTable<IEntityId>)UserRoleTable);

			UserTaskTable = _mobileService.GetSyncTable<Models.UserTask>();
			AddEntry(typeof(Models.UserTask), (IMobileServiceSyncTable<IEntityId>)UserTaskTable);

			WorkflowActivityTable = _mobileService.GetSyncTable<Models.WorkflowActivity>();
			AddEntry(typeof(Models.WorkflowActivity), (IMobileServiceSyncTable<IEntityId>)WorkflowActivityTable);

			WorkflowProcessTable = _mobileService.GetSyncTable<Models.WorkflowProcess>();
			AddEntry(typeof(Models.WorkflowProcess), (IMobileServiceSyncTable<IEntityId>)WorkflowProcessTable);

			WorkflowTypeTable = _mobileService.GetSyncTable<Models.WorkflowType>();
			AddEntry(typeof(Models.WorkflowType), (IMobileServiceSyncTable<IEntityId>)WorkflowTypeTable);
		}

		private static void CreateLocalTables(MobileServiceSQLiteStore store)
		{
			store.DefineTable<Models.Asset>();
			store.DefineTable<Models.Authority>();
			store.DefineTable<Models.Availability>();
			store.DefineTable<Models.Brigade>();
			store.DefineTable<Models.Certificate>();
			store.DefineTable<Models.Device>();
			store.DefineTable<Models.Event>();
			store.DefineTable<Models.EventAttendee>();
			store.DefineTable<Models.EventType>();
			store.DefineTable<Models.EventRole>();
			store.DefineTable<Models.Role>();
			store.DefineTable<Models.TrainingPrerequisite>();
			store.DefineTable<Models.User>();
			store.DefineTable<Models.UserCertificate>();
			store.DefineTable<Models.UserRole>();
			store.DefineTable<Models.UserTask>();
			store.DefineTable<Models.WorkflowActivity>();
			store.DefineTable<Models.WorkflowProcess>();
			store.DefineTable<Models.WorkflowType>();
		}

		private void AddEntry(System.Type type, IMobileServiceSyncTable<IEntityId> table)
		{
			string tableName = type.Name;

			if (!_typeSyncTableMap.ContainsKey(type))
			{
				_typeSyncTableMap[type] = table;
			}

			if (!_tableNameTypeMap.ContainsKey(tableName))
			{
				_tableNameTypeMap[tableName] = type;
			}
		}

		private Dictionary<System.Type, IMobileServiceSyncTable<IEntityId>> _typeSyncTableMap = new Dictionary<System.Type, IMobileServiceSyncTable<IEntityId>>();
		private Dictionary<string, System.Type> _tableNameTypeMap = new Dictionary<string, System.Type>();

		public Dictionary<System.Type, IMobileServiceSyncTable<IEntityId>> TypeSyncTableMap { get { return _typeSyncTableMap; } }
		public Dictionary<string, System.Type> TableNameTypeMap { get { return _tableNameTypeMap; } }

		public Dictionary<string, IMobileServiceTableQuery<IEntityId>> SyncQueries = new Dictionary<string, IMobileServiceTableQuery<IEntityId>>();

		public IMobileServiceSyncTable<Models.Asset> AssetTable { get; private set; }
		public IMobileServiceSyncTable<Models.EventAttendee> AttendeeTable { get; private set; }
		public IMobileServiceSyncTable<Models.Authority> AuthorityTable { get; private set; }
		public IMobileServiceSyncTable<Models.Availability> AvailabilityTable { get; private set; }
		public IMobileServiceSyncTable<Models.Brigade> BrigadeTable { get; private set; }
		public IMobileServiceSyncTable<Models.Certificate> CertificateTable { get; private set; }
		public IMobileServiceSyncTable<Models.Device> DeviceTable { get; private set; }
		public IMobileServiceSyncTable<Models.Event> EventTable { get; private set; }
		public IMobileServiceSyncTable<Models.EventType> EventTypeTable { get; private set; }
		public IMobileServiceSyncTable<Models.EventRole> EventRoleTable { get; private set; }
		public IMobileServiceSyncTable<Models.Role> RoleTable { get; private set; }
		public IMobileServiceSyncTable<Models.TrainingPrerequisite> TrainingPrerequisiteTable { get; private set; }
		public IMobileServiceSyncTable<Models.User> UserTable { get; private set; }
		public IMobileServiceSyncTable<Models.UserCertificate> UserCertificateTable { get; private set; }
		public IMobileServiceSyncTable<Models.UserRole> UserRoleTable { get; private set; }
		public IMobileServiceSyncTable<Models.UserTask> UserTaskTable { get; private set; }
		public IMobileServiceSyncTable<Models.WorkflowActivity> WorkflowActivityTable { get; private set; }
		public IMobileServiceSyncTable<Models.WorkflowProcess> WorkflowProcessTable { get; private set; }
		public IMobileServiceSyncTable<Models.WorkflowType> WorkflowTypeTable { get; private set; }
	}
}
