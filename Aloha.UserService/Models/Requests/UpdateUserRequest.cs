using System.ComponentModel.DataAnnotations;

namespace Aloha.UserService.Models.Requests
{
    public class UpdateUserRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [StringLength(100, ErrorMessage = "User Name not exceed 100 characters")]
        public string UserName { get; set; }

        [StringLength(20, ErrorMessage = "User Name not exceed 20 characters")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? BirthDate { get; set; }
    }
}
