using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.Services;
using minimal_api.Domain.ViewModels;
using minimal_api.Infrastructure.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace minimal_api
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        private string Key;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Key = Configuration.GetSection("Jwt:Key").Value;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(
                    Configuration.GetConnectionString("MySql"),
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
                );
            });

            services.AddAuthentication(option =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                };
            });

            services.AddAuthorization();

            services.AddScoped<IAdministratorService, AdministratorService>();
            services.AddScoped<IVehicleService, VehicleService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
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

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {

                #region Administrator
                string GenerateTokenJwt(Administrator administrator)
                {
                    if (string.IsNullOrEmpty(Key)) return string.Empty;

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes((string)Key));
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

                endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous();

                endpoints.MapPost("/adm/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
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

                endpoints.MapPost("/adm", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
                {
                    List<string> errors = new List<string>();

                    if (string.IsNullOrEmpty(administratorDTO.Email))
                        errors.Add("Email can't be empty.");

                    if (string.IsNullOrEmpty(administratorDTO.Password))
                        errors.Add("Password can't be empty.");

                    if (string.IsNullOrEmpty(administratorDTO.Profile.ToString()))
                        errors.Add("Profilecan't be empty.");

                    if (errors.Count > 0)
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

                endpoints.MapGet("/adm", ([FromQuery] int pagina, IAdministratorService administratorService) =>
                {
                    var adms = administratorService.GetAll(pagina);

                    return Results.Ok(adms);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrators");

                endpoints.MapGet("/adm/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
                {
                    var adm = administratorService.GetById(id);

                    if (adm == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(adm);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrators");

                endpoints.MapPut("/adm/{id}", ([FromRoute] int id, [FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
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

                endpoints.MapDelete("/adm/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
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
                endpoints.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
                {
                    var errors = ErrorsValidator.ValidateDTO(vehicleDTO);

                    if (errors.Count > 0)
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

                endpoints.MapGet("/vehicles", ([FromQuery] int pagina, IVehicleService vehicleService) =>
                {
                    var vehicles = vehicleService.GetAll(pagina);

                    return Results.Ok(vehicles);
                }).RequireAuthorization().WithTags("Vehicle");

                endpoints.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.GetById(id);

                    if (vehicle == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(vehicle);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicle");

                endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, [FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
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

                endpoints.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
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
            });
        }
    }
}
