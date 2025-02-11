using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using minimal_api.Domain.DTOs;
using minimal_api.Infrastructure.Db;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    options.UseMySql(
//        builder.Configuration.GetConnectionString("MySql"),
//        ServerVersion.AutoDetect("MySql")
//    );
//});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Password == "123456")
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
});

app.Run();
