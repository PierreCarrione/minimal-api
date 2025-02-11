using System.ComponentModel.DataAnnotations;

namespace minimal_api.Domain.Entities
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Brand { get; set; }

        [Required]
        public int Year { get; set; }
    }
}
