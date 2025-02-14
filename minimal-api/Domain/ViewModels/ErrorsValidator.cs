using minimal_api.Domain.DTOs;

namespace minimal_api.Domain.ViewModels
{
    public class ErrorsValidator
    {
        public List<string> Messages { get; set; }

        public static List<string> ValidateDTO(VehicleDTO vehicleDTO)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(vehicleDTO.Name))
                errors.Add("Name can't be empty.");

            if (string.IsNullOrEmpty(vehicleDTO.Brand))
                errors.Add("Brand can't be empty.");

            if (vehicleDTO.Year < 1950)
                errors.Add("Very old vehicle; only vehicles from after 1950 are accepted.");

            return errors;
        }
    }
}
