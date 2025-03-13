using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
[Route("users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet("allusers")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<User>> AllUsers()
    {
        var allUsers = await _context.Users.ToListAsync();
        return Ok(allUsers);
    }


    [HttpPost("adduser")]
    public async Task<ActionResult<User>> AddUser([FromBody] User user)
    {
        var existedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existedUser != null)
            return BadRequest(new { Message = "User already exists" });
        //add
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "User Added" });
    }

    [HttpPut("updateuser/{id}")]
    //add authorize
    [Authorize]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User updatedUser)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound(new { Message = $"User with id {id} not found" });
        }

        //----------add code block for authorization
        var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        //admin can update all users but also user can update their own account
        if (userIdFromToken != id && userRole != "Admin")
        {
            return StatusCode(403, new { Message = "You cannot update other people's account" });
        }

        // Only update fields if new values are provided (keep old values otherwise)
        existingUser.UserName = updatedUser.UserName ?? existingUser.UserName;
        existingUser.Email = updatedUser.Email ?? existingUser.Email;
        existingUser.Password = updatedUser.Password ?? existingUser.Password;

        await _context.SaveChangesAsync();
        return Ok(new { Message = $"User with id {id} updated successfully" });
    }
    [HttpDelete("deleteuser/{id}")]
    //ADD AUTHORIZATION
    [Authorize]
    public async Task<ActionResult<User>> DeleteUser(int id)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound(new { Message = $"User with id {id} not found" });
        }

        //  Get the user ID and role from JWT token
        var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRoleFromToken = User.FindFirst(ClaimTypes.Role)?.Value;

        System.Console.WriteLine($"userIdFromToken: {userIdFromToken}");
        System.Console.WriteLine($"userRoleFromToken: {userRoleFromToken}");

        //  Only allow deletion if:
        // - The user is an Admin (userRoleFromToken == "Admin")
        // - OR The user is deleting their own account (userIdFromToken == id)
        if (userIdFromToken != id && userRoleFromToken != "Admin")
        {
            return StatusCode(403, new { Message = "You can only delete your own account" });
        }

        // Perform deletion

        _context.Users.Remove(existingUser);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "User deleted" });

    }


}