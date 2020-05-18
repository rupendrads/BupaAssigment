using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnrollmentAPI.Services;
using EnrollmentAPI.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentAPITests
{
    [TestClass]
    public class UserServiceUnitTests
    {
        private UserService userService;

        private void StartUp()
        {
            var settings = new EnrollmentAPI.AppSettings(){
                Secret = "THIS IS USED TO SIGN AND VERIFY JWT TOKENS, REPLACE IT WITH YOUR OWN SECRET, IT CAN BE ANY STRING"
            };

            IOptions<EnrollmentAPI.AppSettings> appSettingsOptions = Options.Create(settings);

            var optionsBuilder = new DbContextOptionsBuilder<EnrollmentContext>().UseInMemoryDatabase("client");
            EnrollmentContext context = new EnrollmentContext(optionsBuilder.Options);
            userService = new UserService(appSettingsOptions, context);

            if(userService.GetById(1) == null)
            {
                userService.RegisterUser(new User {
                    Id = 1,
                    FirstName = "admin",
                    LastName = "admin",
                    Username = "admin",
                    Password = "admin",
                    Role = Role.Admin,
                    Token = null
                });
            }

            if(userService.GetById(2) == null)
            {
                userService.RegisterUser(
                new User {
                    Id = 2,
                    FirstName = "test",
                    LastName = "test",
                    Username = "test",
                    Password = "test",
                    Role = Role.User,
                    Token = null
                });
            }            
        }
    
        [TestMethod]
        public void Authenticate_Should_Return_Valid_User()
        {
            //Arrange        
            StartUp();

            //Act
            User user = userService.Authenticate("test", "test");

            //Assert            
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void Authenticate_Should_Not_Allow_Invalid_Credentials()
        {
            //Arrange        
            StartUp();

            //Act
            User user = userService.Authenticate("xyz", "abc");

            //Assert            
            Assert.IsNull(user);
        }

        [TestMethod]
        public void Authenticate_Should_Generate_JWT_Token_for_Valid_Credentials()
        {
            //Arrange        
            StartUp();

            //Act
            User user = userService.Authenticate("test", "test");

            //Assert            
            Assert.AreEqual(false, string.IsNullOrEmpty(user.Token));
        }

        [TestMethod]
        public void GetUserById_Should_Return_User_for_Valid_UserId()
        {
            //Arrange        
            StartUp();

            //Act
            User user = userService.GetById(2);

            //Assert            
            Assert.IsNotNull(user);
            Assert.AreEqual("test", user.Username);
            Assert.AreEqual(Role.User, user.Role);
        }

        [TestMethod]
        public void GetUserById_Should_Return_User_for_Valid_AdminId()
        {
            //Arrange        
            StartUp();

            //Act
            User user = userService.GetById(1);

            //Assert            
            Assert.IsNotNull(user);
            Assert.AreEqual("admin", user.Username);
            Assert.AreEqual(Role.Admin, user.Role);
        }

        [TestMethod]
        public void RegisterUser_Should_Add_User_to_Users_List()
        {
            //Arrange        
            StartUp();

            //Act
            int oldCount = userService.GetAll().Count();
            userService.RegisterUser(new User {
                Id = 3,
                FirstName = "Rakesh",
                LastName = "Shah",
                Username = "rakesh",
                Password = "rakesh",
                Role = Role.User,
                Token = null
            });
            int newCount = userService.GetAll().Count();

            //Assert                        
            Assert.AreEqual(oldCount + 1, newCount);            
        }
                
        [TestMethod]
        public void UpdateUser_Should_modify_User_in_Users_List()
        {
            //Arrange        
            StartUp();

            //Act
            User user = userService.GetById(2);
            user.FirstName ="Test1";                        
            user.LastName = "Test1";
            userService.UpdateUser(user.Id, user);
            User updatedUser = userService.GetById(2);

            // Arrange
            Assert.AreEqual("Test1", updatedUser.FirstName);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void UpdateUser_Should_Throw_Exception_When_Id_Not_Found()
        {
            //Arrange        
            StartUp();

            //Act
            User user = userService.GetById(2);
            user.FirstName ="R";                        
            user.LastName = "S";
            userService.UpdateUser(10, user);                        
        }

        [TestMethod]
        public void DeleteUser_Should_remove_User_in_Users_List()
        {
            //Arrange        
            StartUp();

            // Act
            User user = userService.DeleteUser(3);

            // Arrange
            Assert.IsNotNull(user);  
        }

        [TestMethod]
        public void DeleteUser_Should_return_Null_User_for_Invalid_User_Id()
        {
            //Arrange        
            StartUp();

            // Act
            User user = userService.DeleteUser(10);

            // Arrange
            Assert.IsNull(user);  
        }
    }
}
