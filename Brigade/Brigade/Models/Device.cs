
namespace Brigade.Models
{
    public class Device : EntityBase<User>
    {
        public string DeviceId { get; set; }
		public string DeviceType { get; set; }

        public override void SetId()
        {
            base.SetId(DeviceId);
        }
    }
}
