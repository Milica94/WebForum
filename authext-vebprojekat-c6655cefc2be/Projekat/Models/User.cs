using System;

namespace Projekat.Models
{
    public class User
    {
        public enum ForumRole
        {
            Normal,
            Moderator,
            Administrator
        }


        public string Name { get; set; }
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ForumRole Role { get; set; }

        public string PhoneNo { get; set; }
        public string Email { get; set; }

        public DateTime RegisteredOn { get; set; }
    }
}