using Brigade.Abstractions;

namespace Brigade.Models
{
    public class Device : EntityBase<User>
    {
		public string DeviceId { get; set; }
		public string DeviceType { get; set; }
    }
}
