using System.ComponentModel.DataAnnotations;

namespace Aloha.MicroService.User.Models.Requests
{
    public class UpdateUserStatusRequest
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
