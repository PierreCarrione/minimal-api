using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enuns;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.Services;
using minimal_api.Domain.ViewModels;
using minimal_api.Infrastructure.Db;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration.GetSection("Jwt:Key").Value;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),   
    };
});

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insert your JWT Token this way: Bearer {your token}."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


#region Administrator
string GenerateTokenJwt(Administrator administrator)
{
    if(string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes((string)key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    var claims = new List<Claim>()
    {
        new Claim("Email", administrator.Email),
        new Claim("Perfil", administrator.Profile),
        new Claim(ClaimTypes.Role, administrator.Profile),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}


app.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous();

app.MapPost("/adm/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    var adm = administratorService.Login(loginDTO);
    if (adm != null)
    {
        string token = GenerateTokenJwt(adm);
        return Results.Ok(new AdmLogged
        {
            Email = adm.Email,
            Profile = adm.Profile,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administrators");

app.MapPost("/adm", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
{
    List<string> errors = new List<string>();

    if (string.IsNullOrEmpty(administratorDTO.Email))
        errors.Add("Email can't be empty.");

    if (string.IsNullOrEmpty(administratorDTO.Password))
        errors.Add("Password can't be empty.");

    if (string.IsNullOrEmpty(administratorDTO.Profile.ToString()))
        errors.Add("Profilecan't be empty.");

    if(errors.Count > 0)
        return Results.BadRequest(errors);

    var admin = new Administrator
    {
        Email = administratorDTO.Email,
        Password = administratorDTO.Password,
        Profile = administratorDTO.Profile.ToString()
    };

    administratorService.Add(admin);

    return Results.Created($"/adm/{admin.Id}", admin);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrators");

app.MapGet("/adm", ([FromQuery] int pagina, IAdministratorService administratorService) =>
{
    var adms = administratorService.GetAll(pagina);

    return Results.Ok(adms);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"}).WithTags("Administrators");

app.MapGet("/adm/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
{
    var adm = administratorService.GetById(id);

    if (adm == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(adm);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrators");

app.MapPut("/adm/{id}", ([FromRoute] int id, [FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
{
    var adm = administratorService.GetById(id);

    if (adm == null)
    {
        return Results.NotFound();
    }

    adm.Email = administratorDTO.Email;
    adm.Password = administratorDTO.Password;
    adm.Profile = administratorDTO.Profile.ToString();
    administratorService.Update(adm);

    return Results.Ok(adm);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrators");

app.MapDelete("/adm/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
{
    var adm = administratorService.GetById(id);

    if (adm == null)
    {
        return Results.NotFound();
    }

    var result = administratorService.Delete(adm);

    if (result.Contains("Something wrong"))
    {
        return Results.Problem(result);
    }

    return Results.Ok(result);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrators");

#endregion

#region Vehicles
app.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
{
    var errors = ErrorsValidator.ValidateDTO(vehicleDTO);

    if(errors.Count > 0)
        return Results.BadRequest(errors);

    var _vehicle = new Vehicle
    {
        Name = vehicleDTO.Name,
        Brand = vehicleDTO.Brand,
        Year = vehicleDTO.Year
    };

    vehicleService.Add(_vehicle);

    return Results.Created($"/veiculo/{_vehicle.Id}", _vehicle);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicle");

app.MapGet("/vehicles", ([FromQuery] int pagina, IVehicleService vehicleService) =>
{
   var vehicles = vehicleService.GetAll(pagina);

    return Results.Ok(vehicles);
}).RequireAuthorization().WithTags("Vehicle");

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);

    if (vehicle == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(vehicle);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicle");

app.MapPut("/vehicles/{id}", ([FromRoute] int id,[FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);

    if (vehicle == null)
    {
        return Results.NotFound();
    }

    vehicle.Name = vehicleDTO.Name;
    vehicle.Brand = vehicleDTO.Brand;   
    vehicle.Year = vehicleDTO.Year;
    vehicleService.Update(vehicle);

    return Results.Ok(vehicle);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicle");

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);

    if (vehicle == null)
    {
        return Results.NotFound();
    }

    var result = vehicleService.Delete(vehicle);

    if (result.Contains("Something wrong"))
    {
        return Results.Problem(result);
    }

    return Results.Ok(result);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicle");
#endregion

app.UseAuthentication();
app.UseAuthorization();

app.Run();
