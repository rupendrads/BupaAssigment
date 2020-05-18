using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using EnrollmentAPI.Services;
using EnrollmentAPI.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // only allow admins to access other user records
            var currentUserId = int.Parse(User.Identity.Name);
            if (id != currentUserId && !User.IsInRole(Role.Admin))
                return Forbid();

            var user =  _userService.GetById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public ActionResult<User> PostUser(User user)
        {
            User newUser = _userService.RegisterUser(user);

            return CreatedAtAction("GetById", new { id = newUser.Id }, newUser);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPut("{id}")]
        public ActionResult PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }            

            try
            {
                _userService.UpdateUser(id, user);
            }
            catch(KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;                
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Role.Admin)]
        public ActionResult<User> DeleteUser(int id)
        {
            var user = _userService.DeleteUser(id);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
    }
}