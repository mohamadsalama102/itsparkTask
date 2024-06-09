using Google.Apis.Gmail.v1.Data;

namespace itsparkTask.Models.EmailViewModels
{

    public class EmailViewModel
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string Date { get; set; }
        public string Body { get; set; }
        public Message OrginalEmail { get; set; }
    }
}