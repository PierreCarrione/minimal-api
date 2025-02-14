using System.ComponentModel.DataAnnotations;

namespace minimal_api.Domain.DTOs
{
    public class VehicleDTO
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public int Year { get; set; }
    }
}
