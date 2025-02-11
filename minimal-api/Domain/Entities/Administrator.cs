using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace minimal_api.Domain.Entities
{
    public class Administrator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Password { get; set; }

        [StringLength(10)]
        public string Profile { get; set; }
    }
}
