using System.ComponentModel.DataAnnotations;

namespace TechConnect.Models
{
    public class TechLogin
    {
        [Required(ErrorMessage = "Please enter the email")]
        public string Email { set; get; }

        [Required(ErrorMessage = "Please enter the password")]
        [DataType(DataType.Password)]
        public string Password { set; get; }
    }
}
