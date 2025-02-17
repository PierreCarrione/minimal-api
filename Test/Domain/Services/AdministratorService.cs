using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Domain.Entities;
using minimal_api.Infrastructure.Db;

namespace Test.Domain.Services
{
    [TestClass]
    public class AdministratorService
    {
        private ApplicationDbContext CreateTestContext()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            return new ApplicationDbContext(configuration);
        }

        [TestMethod]
        public void TestSaveAdministrator()
        {
            var context = CreateTestContext();
            var administratorService = new minimal_api.Domain.Services.AdministratorService(context);
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administrators");
            var adm = new Administrator();

            adm.Id = 1;
            adm.Email = "test@test.com";
            adm.Password = "password";
            adm.Profile = "Adm";

            administratorService.Add(adm);

            Assert.AreEqual(1, administratorService.GetAll().Count());
        }
    }
}