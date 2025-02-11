using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Services
{
    public class AdministratorService : IAdministratorService
    {
        private readonly ApplicationDbContext _context;

        public AdministratorService(ApplicationDbContext context)
        {
            _context = context; 
        }

        public Administrator? Login(LoginDTO loginDTO)
        {
            return _context.Administrators.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).FirstOrDefault();
        }
    }
}
