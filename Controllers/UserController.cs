using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ms_admin.Dbconnection;
using ms_admin.model;
using ms_admin.Dbconnection;

namespace ms_admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly userdbconnection _context;

        public UserController(userdbconnection context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Register user)
        {
            // Validate user input
            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Invalid registration data");
            }

            // Check if the username is unique
            if (_context.Register.Any(u => u.Username == user.Username))
            {
                return Conflict("Username already exists");
            }

            _context.Register.Add(user);
            _context.SaveChanges();

            return Ok("Registration successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            if (model == null)
            {
                return BadRequest("Invalid login model");
            }

            // Validate the model
            if (model.Mobile == 0)
            {
                ModelState.AddModelError("Mobile", "The Mobile field is required.");
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "The Password field is required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = ModelState
                        .Where(e => e.Value.Errors.Any())
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            // Perform the login logic here, using model.Mobile and model.Password
            var user = await _context.Register.SingleOrDefaultAsync(u => u.Mobile == model.Mobile);

            // Check if the user exists
            if (user == null)
            {
                return BadRequest(new { message = "Invalid mobile number" });
            }

            // Validate the password (use a proper password hashing and validation library)
            if (!ValidatePassword(model.Password, user.Password))
            {
                return BadRequest(new { message = "Invalid password" });
            }

            // Get products associated with the logged-in user
            var userProducts = _context.ProductDtos
                .Where(p => p.UserName == user.Username)
                .ToList();

            // For demonstration purposes, let's assume login is successful
            return Ok(new
            {
                message = "Login successful",
                username = user.Username,
                products = userProducts
            });
        }


        private bool ValidatePassword(string enteredPassword, string storedPassword)
        {
            // Implement your password validation logic (e.g., using a password hashing library)
            // For simplicity, let's assume a basic string comparison
            return enteredPassword == storedPassword;
        }


        [HttpGet("GetUserDetails")]
        public IActionResult GetUserDetails()
        {
            try
            {
                var userDetails = _context.Register.Select(r => new
                {
                    r.Id,
                    r.Username,
                    r.Mobile
                }).ToList();

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "Internal server error");
            }
        }


        ///for admin pannel // / // // // // // // // // // //
        [HttpGet("GetRegisteredUserDetails")]
        public IActionResult GetUsers()
        {
            var users = _context.Register.ToList();
            return Ok(users);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] Register updatedUser)
        {
            var user = await _context.Register.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Username = updatedUser.Username;
            user.Password = updatedUser.Password;
            user.Mobile = updatedUser.Mobile;

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // Delete method
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Register.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Register.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
