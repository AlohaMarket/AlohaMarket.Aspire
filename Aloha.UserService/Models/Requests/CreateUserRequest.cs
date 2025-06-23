using System.ComponentModel.DataAnnotations;

namespace Aloha.UserService.Models.Requests
{
    public class CreateUserRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [StringLength(100, ErrorMessage = "User Name not exceed 100 characters")]
        public string UserName { get; set; }
    }
}
