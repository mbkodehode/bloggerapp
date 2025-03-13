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
using Microsoft.OpenApi.Models;

using Microsoft.OpenApi;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//     {
//         Name = "authorization",
//         In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//         Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         Description = "enter your jwt token in the format 'Bearer {your token here}'"
//     });
//     c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//         {
//             {
//                 new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//                 {
//                     Reference = new Microsoft.OpenApi.Models.OpenApiReference
//                     {
//                         Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//                         Id = "Bearer"
//                     }
//                 },
//                 new string[] {}
//             }
//         });
// });

// builder.Services.AddSwaggerGen();
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
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//5- we enable authentication and authorization middleware 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// app.MapGet("/", () => "Hello World!");

app.MapGet("/", (context) =>
{
    // context.Response.Redirect("index.html");
    context.Response.Redirect("/vite/index.html");
    return Task.CompletedTask;
});

app.Run();