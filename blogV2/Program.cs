using Microsoft.EntityFrameworkCore;
//tokens namespace
using Microsoft.IdentityModel.Tokens;
//jwt namespace
//need to be installed manually 
//dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
// or if you use net 8
// dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1- we configure JWT 
// add JWT section in appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");

//2- we get key 

var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "");

//3- add Authentication service 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true

    };
});

//4- enable [Authorize] attribute 
builder.Services.AddAuthorization();

//register db context 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

//inorder to stop json cyle 
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true; // Pretty JSON output 

});


var app = builder.Build();

//5- we enable authentication and authorization middleware 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();