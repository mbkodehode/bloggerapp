//namespaces 
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// identity model
using Microsoft.IdentityModel.Tokens;
//jwt token 
using System.IdentityModel.Tokens.Jwt;
//security claims
using System.Security.Claims;
using System.Text;

//Route Name 
[Route("auth")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    //Iconfiguration explanation 
    private readonly IConfiguration _config;

    public AuthController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;

    }

    //common end points 
    // we want to get request from a logic request 
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            //This line checks if a user exists in the database with the provided email and password.
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

        //check user
        if (user == null)
            return Unauthorized(new { Message = "Invalid email or password", Error = 401 });
        //if user exists 
        //create  a token 
        //create instance of GenerateJWTTOken 
        // calls the GenerateJwtToken(user) method to generate a JWT token for the authenticated user.
        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });

    }

    //generate token 
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");

        //check key is empty or not 
        var keyString = jwtSettings["Key"];
        if (string.IsNullOrEmpty(keyString))
        {
            throw new Exception("JWT Key is missing from configuration");
        }
        var key = Encoding.UTF8.GetBytes(keyString);

        //Claims are key-value pairs that store user identity details inside the JWT token.
        var claims = new List<Claim> {
            //ClaimTypes.NameIdentifier: Stores the user's ID.
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //ClaimTypes.Name: Stores the username.
            new Claim(ClaimTypes.Name, user.UserName),
            // Stores the user’s role (Admin, Writer, or Reader) to manage access control.
        
            new Claim(ClaimTypes.Role, user.UserRole.ToString()) // Store role in token


        };
        //Defines how the token should be structured.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Subject = new ClaimsIdentity(claims): Assigns user identity details (claims) to the token.
            Subject = new ClaimsIdentity(claims),
            // Expires: Sets the expiration time for the token.
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["ExpirationInMinutes"])),
            // SigningCredentials: Signs the token using HMAC SHA-256 encryption for security.
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            // Issuer: Defines the entity that issues the token (from appsettings.json).
            Issuer = jwtSettings["Issuer"],
            //Audience: Defines the expected recipient of the token (from appsettings.json)
            Audience = jwtSettings["Audience"]

        };
        //JwtSecurityTokenHandler

        var tokenHandler = new JwtSecurityTokenHandler();
        // CreateToken(tokenDescriptor): Generates the token based on the defined properties.
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // WriteToken(token): Converts the token into a string format that can be sent to the client.
        return tokenHandler.WriteToken(token);

    }
}

//define Login Request Model 
public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}