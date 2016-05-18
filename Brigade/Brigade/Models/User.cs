using System;

namespace Brigade.Models
{
    public class User : EntityBase<Brigade>
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public TimeSpan ExtraReminder { get; set; }

        public override void SetId()
        {
            base.SetId(UserId);
        }
    }
}
