using System;
using Brigade.Abstractions;

namespace Brigade.Models
{
    public class UserTask : EntityBase<User>
    {
		public DateTime? ActualDate { get; set; }
		public string Description { get; set; }
		public TimeSpan? RemindBefore { get; set; }
		public TimeSpan? RemindersAfter { get; set; }
		public IEntityId Owner { get; set; }
		public AnyContainer WorkflowItem { get; set; }

		public override void SetId()
		{
			base.SetId((ActualDate.HasValue ? ActualDate.Value.ToString("yyyyMMddhhmm") : "next") + "." + WorkflowItem.Id);
		}
	}
}
