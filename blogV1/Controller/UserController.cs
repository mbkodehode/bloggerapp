using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
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
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User updatedUser)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound(new { Message = $"User with id {id} not found" });
        }
        // Only update fields if new values are provided (keep old values otherwise)
        existingUser.UserName = updatedUser.UserName ?? existingUser.UserName;
        existingUser.Email = updatedUser.Email ?? existingUser.Email;
        existingUser.Password = updatedUser.Password ?? existingUser.Password;

        await _context.SaveChangesAsync();
        return Ok(new { Message = $"User with id {id} updated successfully" });
    }
    [HttpDelete("deleteuser/{id}")]
    public async Task<ActionResult<User>> DeleteUser(int id)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound(new { Message = $"User with id {id} not found" });
        }
        _context.Users.Remove(existingUser);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "User deleted" });
    }

}