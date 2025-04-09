using System.ComponentModel.DataAnnotations;

namespace TechConnect.Models
{
    public class Registrant
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the user name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter the password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please re-enter the password")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        
        [Required(ErrorMessage = "Please enter the email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter the mobile number")]
        public long MobileNumber { get; set; }
    }
}
