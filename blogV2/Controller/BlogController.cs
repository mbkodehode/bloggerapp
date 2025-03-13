using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("blogs")]

[ApiController]

public class BlogController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BlogController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("allblogs")]
    public async Task<ActionResult<Blog>> AllBlogs()
    {
        //we had to add Include to show author 
        var allBlogs = await _context.Blogs
            .Include(b => b.Author)
            .ThenInclude(a => a.Blogs)

        .ToListAsync();
        return Ok(allBlogs);
    }
    [HttpPost("addblog")]
    [Authorize]
    public async Task<ActionResult<Blog>> AddBlog([FromBody] Blog blog)
    {

        // get the user role from the token
        var userRoleFromTokin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        System.Console.WriteLine("userRoleFromTokin: -.-.-.-.-.-.-.-.-" + userRoleFromTokin);
        // allow only the author and admin to add a blog
        if (userRoleFromTokin != "admin" && userRoleFromTokin != "Writer")
        {
            return StatusCode(403, new { Message = "You are not allowed to add a blog" });
        }


        await _context.Blogs.AddAsync(blog);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "blog added" });
    }
    // update
    // update
    [HttpPut("updateblog/{id}")]
    //only writer=1 can do it 
    [Authorize(Roles = "Writer")]
    public async Task<ActionResult> UpdateBlog(int id, [FromBody] Blog updatedBlog)
    {
        var existingBlog = await _context.Blogs.FindAsync(id);
        if (existingBlog == null)
        {
            return NotFound(new { Message = $"Blog with id {id} not found" });
        }
        var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        // Check if the logged-in user is the author of the blog
        if (existingBlog.AuthorId != userIdFromToken)
        {
            return StatusCode(403, new { Message = "You can only update your own blog." });
        }

        //  Update only provided fields (keep others unchanged)
        existingBlog.Title = updatedBlog.Title ?? existingBlog.Title;
        existingBlog.Content = updatedBlog.Content ?? existingBlog.Content;

        await _context.SaveChangesAsync();
        return Ok(new { Message = $"Blog with id {id} updated successfully" });
    }

    //delete
    [HttpDelete("deleteblog/{id}")]
    public async Task<ActionResult> DeleteBlog(int id)
    {
        var existingBlog = await _context.Blogs.FindAsync(id);
        if (existingBlog == null)
        {
            return NotFound(new { Message = $"Blog with id {id} not found" });
        }
        var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        // Check if the logged-in user is the author of the blog or an admin
        var UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (UserRole != "admin" && existingBlog.AuthorId != userIdFromToken)
        {
            return StatusCode(403, new { Message = "You can only delete your own blog or you must be an admin." });
        }

        _context.Blogs.Remove(existingBlog);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Blog with id {id} deleted successfully" });
    }


}
