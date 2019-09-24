using System;

namespace SocialRide.Dtos
{
    public class UserRegistrantDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Prefix { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
