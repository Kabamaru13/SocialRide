using System;
using System.Collections.Generic;

namespace SocialRide.Models
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public string Username { get; set; }
        public string Email { get; set; }
        public string Prefix { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public double PassengerRate { get; set; }
        public double DriverRate { get; set; }
        public List<User> Vehicles { get; set; }
        public bool IsDriver { get; set; }
        //public byte[] PasswordHash { get; set; }
        //public byte[] PasswordSalt { get; set; }
    }
}
