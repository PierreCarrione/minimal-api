using minimal_api.Domain.Enuns;
using System.ComponentModel.DataAnnotations;

namespace minimal_api.Domain.DTOs
{
    public class AdministratorDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Profile Profile { get; set; }
    }
}
