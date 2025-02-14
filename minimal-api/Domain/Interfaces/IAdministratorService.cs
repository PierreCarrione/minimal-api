using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IAdministratorService
    {
        Administrator? Login(LoginDTO loginDTO);
        List<Administrator> GetAll(int page);
        Administrator? GetById(int id);
        Administrator Add(Administrator administrator);
        Administrator Update(Administrator administrator);
        string Delete(Administrator administrator);
    }
}
