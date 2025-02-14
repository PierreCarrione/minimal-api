using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.Services;
using minimal_api.Domain.ViewModels;
using minimal_api.Infrastructure.Db;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


#region Administrator
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");

app.MapPost("/adm/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    if (administratorService.Login(loginDTO) != null)
    {
        return Results.Ok("Login com sucesso.");
    }
    else
        return Results.Unauthorized();
}).WithTags("Administrators");

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
}).WithTags("Administrators");

app.MapGet("/adm", ([FromQuery] int pagina, IAdministratorService administratorService) =>
{
    var adms = administratorService.GetAll(pagina);

    return Results.Ok(adms);
}).WithTags("Administrators");

app.MapGet("/adm/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
{
    var adm = administratorService.GetById(id);

    if (adm == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(adm);
}).WithTags("Administrators");

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
}).WithTags("Administrators");

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
}).WithTags("Administrators");

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
}).WithTags("Vehicle");

app.MapGet("/vehicles", ([FromQuery] int pagina, IVehicleService vehicleService) =>
{
   var vehicles = vehicleService.GetAll(pagina);

    return Results.Ok(vehicles);
}).WithTags("Vehicle");

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);

    if (vehicle == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(vehicle);
}).WithTags("Vehicle");

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
}).WithTags("Vehicle");

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
}).WithTags("Vehicle");
#endregion

app.Run();
