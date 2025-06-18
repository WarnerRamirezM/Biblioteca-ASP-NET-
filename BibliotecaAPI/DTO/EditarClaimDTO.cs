using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTO
{
    public class EditarClaimDTO
    {
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
    }
}
