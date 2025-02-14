using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;
using System.Linq;

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

        public Administrator Add(Administrator administrator)
        {
            _context.Administrators.Add(administrator);
            _context.SaveChanges();
            return administrator;
        }

        public string Delete(Administrator administrator)
        {
            string message;

            try
            {
                _context.Administrators.Remove(administrator);
                _context.SaveChanges();
                message = "Administrator deleted successfully.";
            }
            catch (Exception ex)
            {
                message = "Something wrong happened : " + ex.Message;
            }

            return message;
        }

        public List<Administrator> GetAll(int page = 1)
        {
            int pageSize = 10;
            var query = _context.Administrators.AsQueryable();
            var result = query.ToList();

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public Administrator? GetById(int id)
        {
            return _context.Administrators.Where(x => x.Id == id).FirstOrDefault();
        }

        public Administrator Update(Administrator administrator)
        {
            _context.Administrators.Update(administrator);
            _context.SaveChanges();
            return administrator;
        }
    }
}
