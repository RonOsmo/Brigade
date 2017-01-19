using Brigade.Abstractions;
using Brigade.Models;
using Brigade.Views;
using Brigade.Services;
using Prism.Unity;
using Microsoft.Practices.Unity;
using System;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Brigade
{
	public class App : PrismApplication
	{
		public const string AppName = "Brigade";
		public const string LocalDbFilename = "BrigadeDb.sqlite";
		//private const string AllAlbumsQueryString = "allAlbums";
		//private const string AllImagesQueryString = "allImages";
		//public MobileServiceClient MobileService;
		//public MobileServiceUser AuthenticatedUser;

		//public IMobileServiceSyncTable<Models.Album> albumTableSync;
		//public IMobileServiceSyncTable<Models.Image> imageTableSync;

		public static App Instance;
		//public static object UIContext { get; set; }

		//private static Object currentDownloadTaskLock = new Object();
		//private static Task currentDownloadTask = Task.FromResult(0);

		public string DataFilesPath { get; set; }

		public App()
		{
			Instance = this;
		}

		protected override void OnInitialized()
		{
			throw new NotImplementedException();
		}

		protected override void OnStart()
		{
			base.OnStart();
		}

		protected override void RegisterTypes()
		{
			Container
				.RegisterType<ILoginService, MockLoginService>(new ContainerControlledLifetimeManager())
				.RegisterType<ILocalRepositoryService, RepositoryService>(new ContainerControlledLifetimeManager())
				.RegisterType<IWorkflowBuilder, WorkflowBuilder>()
				.RegisterType<Asset>()
				.RegisterType<Authority>()
				.RegisterType<Availability>()
				.RegisterType<Models.Brigade>()
				.RegisterType<Certificate>()
				.RegisterType<Device>()
				.RegisterType<Event>()
				.RegisterType<EventAttendee>()
				.RegisterType<EventRole>()
				.RegisterType<EventType>()
				.RegisterType<Role>()
				.RegisterType<TrainingPrerequisite>()
				.RegisterType<User>()
				.RegisterType<UserCertificate>()
				.RegisterType<UserRole>()
				.RegisterType<UserTask>()
				;
			Container
				.RegisterTypeForNavigation<AssetCountView>()
				;
		}
	}

}
