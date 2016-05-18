using Brigade.Models;
using Brigade.Abstractions;

namespace Brigade.Services
{

	public class MockLoginService : ILoginService
    {
		public MockLoginService()
		{
            Authority cfa = new Authority { Id = "cfa", Name = "Victorian Country Fire Authority" };
            CurrentBrigade = new Models.Brigade { Container = cfa, BrigadeId = "maccy", Name = "Macclesfield Country Fire Brigade" };
            CurrentBrigade.SetId();
            CurrentUser = new User { UserId = "gizmo", FirstName = "Ron", LastName = "Osmo", Email = "ronosmo@hotmail.com", Container = CurrentBrigade };
            CurrentUser.SetId();
			CurrentDevice = new Device { DeviceId = "RosmoQ", DeviceType = "win10", Container = CurrentUser };
			CurrentDevice.SetId();
		}

		public Models.Brigade CurrentBrigade { get; set; }
		public User CurrentUser { get; set; }
		public Device CurrentDevice { get; set; }

        public string StorageConnectionString
        {
            get
            {
                return "UseDevelopmentStorage=true;";
            }
        }
    }
}
