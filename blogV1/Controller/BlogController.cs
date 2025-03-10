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
    public async Task<ActionResult<Blog>> AddBlog([FromBody] Blog blog)
    {
        await _context.Blogs.AddAsync(blog);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "blog added" });
    }
    // update
    [HttpPut("updateblog/{id}")]
    public async Task<ActionResult> UpdateBlog(int id, [FromBody] Blog updatedBlog)
    {
        var existingBlog = await _context.Blogs.FindAsync(id);
        if (existingBlog == null)
        {
            return NotFound(new { Message = $"Blog with id {id} not found" });
        }

        // ✅ Update only provided fields (keep others unchanged)
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

        _context.Blogs.Remove(existingBlog);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Blog with id {id} deleted successfully" });
    }


}
