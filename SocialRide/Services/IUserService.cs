using System;
using System.Collections.Generic;
using System.Linq;
using SocialRide.Models;
using SocialRide.Helpers;

namespace SocialRide.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User GetById(string id);
        User Create(User user);
        User Update(User user);
        void Delete(string id);
    }

    public class UserService : IUserService
    {
        private DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(string id)
        {
            return _context.Users.Find(id);
        }

        public User Create(User user)
        {
            if (_context.Users.Any(x => x.Id == user.Id))
            {
                //if user exists I will update his newly provided data
                var _user = _context.Users.Find(user.Id);
                _user.FirstName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : _user.FirstName;
                _user.LastName = !string.IsNullOrEmpty(user.LastName) ? user.LastName : _user.LastName;
                _user.Email = !string.IsNullOrEmpty(user.Email) ? user.Email : _user.Email;
                _user.Prefix = !string.IsNullOrEmpty(user.Prefix) ? user.Prefix : _user.Prefix;
                _user.Phone = !string.IsNullOrEmpty(user.Phone) ? user.Phone : _user.Phone;
                _user.Avatar = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : _user.Avatar;
                
                Update(_user);
            }
            else
            { 
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            return user;
        }

        public User Update(User userParam)
        {
            var user = _context.Users.Find(userParam.Id);

            if (user == null)
                throw new AppException("User not found");

            // update user properties
            user.FirstName = userParam.FirstName;
            user.LastName = userParam.LastName;
            user.Email = userParam.Email;
            user.Prefix = userParam.Prefix;
            user.Phone = userParam.Phone;
            user.Avatar = userParam.Avatar;
            user.BirthDate = userParam.BirthDate;
            user.Gender = userParam.Gender;
            user.PassengerRate = userParam.PassengerRate;
            user.DriverRate = userParam.DriverRate;
            user.IsDriver = userParam.IsDriver;

            try
            {
                _context.Users.Update(user);
                _context.SaveChanges();
                return user;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        public void Delete(string id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                try
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new AppException(ex.Message);
                }
            }
        }
    }
}
