using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using EnrollmentAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentAPI.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User RegisterUser(User user);
        void UpdateUser(int id, User user);

        User DeleteUser(int id);
    }

    public class UserService : IUserService
    {
        private readonly EnrollmentContext _context;

        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {             
            new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test", Role = Role.User },
            new User { Id = 2, FirstName = "Admin", LastName = "User", Username = "admin", Password = "admin", Role = Role.Admin }
        };

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings, EnrollmentContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public User Authenticate(string username, string password)
        {
            //var user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);
            var user = _context.Users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            //return _users;
            return _context.Users.ToList();
        }

        public User GetById(int id) 
        {            
            var user = _context.Users.Find(id);
            return user;
        }

        public User RegisterUser(User user)
        {
            //_users.Add(user);
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public void UpdateUser(int id, User user)
        {
            if(!_context.Users.Any(e => e.Id == id))
            {
                throw new KeyNotFoundException();
            }

            User userInDb = _context.Users.Find(id);
            userInDb.FirstName = user.FirstName;
            userInDb.LastName = user.LastName;
            
            _context.Entry(userInDb).State = EntityState.Modified;
            _context.SaveChanges();                 
        }

        public User DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return null;
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return user;
        }
    }
}