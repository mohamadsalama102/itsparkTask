using System.ComponentModel.DataAnnotations;

namespace itsparkTask.Models.EmailViewModels
{
    public class SendEmailViewModel
    {
        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "To is required")]
        public string To { get; set; }

        public string Cc { get; set; } = string.Empty;
        public string Bcc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body is required")]
        public string Body { get; set; }
    }
}