using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;

        public VehicleService(ApplicationDbContext context)
        {
            _context = context;
        }
        public Vehicle Add(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
            return vehicle;
        }

        public string Delete(Vehicle vehicle)
        {
            string message;

            try
            {
                _context.Vehicles.Remove(vehicle);
                _context.SaveChanges();
                message = "Vehicle deleted successfully.";
            }
            catch (Exception ex) { 
                message = "Something wrong happened : " + ex.Message;
            }

            return message;
        }

        public List<Vehicle> GetAll(int page = 1, string? name = null, string? brand = null)
        {
            int pageSize = 10;
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(v => EF.Functions.Like(v.Name.ToLower(), name.ToLower())); 

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(v => EF.Functions.Like(v.Brand.ToLower(), brand.ToLower())); 

            var result = query.ToList();

            return query
                .Skip((page - 1) * pageSize) 
                .Take(pageSize)              
                .ToList();
        }

        public Vehicle? GetById(int id)
        {
            return _context.Vehicles.Where(x => x.Id == id).FirstOrDefault();
        }

        public Vehicle Update(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
            return vehicle;
        }
    }
}
