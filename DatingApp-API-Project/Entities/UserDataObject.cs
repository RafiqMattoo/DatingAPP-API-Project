using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Entities
{
    public class UserDataObject
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(8,MinimumLength=4,ErrorMessage="Password Must be 4 and 8 Charecters")]      
        public string Password { get; set; }
    }
}