using System.ComponentModel.DataAnnotations;

namespace SlidableLive.Models.AccountViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Handle can contain letters, numbers and underscores")]
        public string Handle { get; set; }
    }
}
