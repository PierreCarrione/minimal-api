using minimal_api.Domain.Entities;

namespace Test.Domain.Entities
{
    [TestClass]
    public class AdministratorTest
    {
        [TestMethod]
        public void TestGetSetProperties()
        {
            var adm = new Administrator();

            adm.Id = 1;
            adm.Email = "test@test.com";
            adm.Password = "password";
            adm.Profile = "Adm";

            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("test@test.com", adm.Email);
            Assert.AreEqual("password", adm.Password);
            Assert.AreEqual("Adm", adm.Profile);
        }
    }
}