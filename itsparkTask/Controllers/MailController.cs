using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using itsparkTask.Models.EmailViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MimeKit;
using Newtonsoft.Json;
using NuGet.Packaging;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace itsparkTask.Controllers
{
    [Authorize]
    public class MailController : Controller
    {
        private readonly GmailService _gmailService;
        readonly IDistributedCache cache;
        static int messageCount;

        public MailController(GmailService gmailService, IDistributedCache cache)
        {
            this.cache = cache;
            _gmailService = gmailService;
        }
        public async Task<IActionResult> Index(int _messageCount = 10)
        {
            messageCount = _messageCount;
            var emails = await FetchLatestEmails(messageCount);
            cache.SetString("emails", JsonConvert.SerializeObject(emails));
            ViewBag.Emails = emails;
            return View();
        }
        public async Task<IActionResult> RefreshData()
        {
            var emails = await FetchLatestEmails(messageCount);
            cache.SetString("emails", JsonConvert.SerializeObject(emails));
            ViewBag.Emails = emails;
            return PartialView("_EmailTable", emails);
        }

        public async Task<IActionResult> Details(string Id)
        {
            var emails = JsonConvert.DeserializeObject<List<EmailViewModel>>(cache.GetString("emails"));
            var email = emails.FirstOrDefault(x => x.Id == Id);
            if (email == null)
                return NotFound();
            return View(email);
        }


        private async Task<List<EmailViewModel>> FetchLatestEmails(int messageCount)
        {
            List<EmailViewModel> emails = new List<EmailViewModel>();

            try
            {

                var request = _gmailService.Users.Messages.List("me");
                request.MaxResults = messageCount;

                var response = await request.ExecuteAsync();
                if (response != null && response.Messages != null)
                {
                    foreach (var message in response.Messages)
                    {
                        var email = await _gmailService.Users.Messages.Get("me", message.Id).ExecuteAsync();
                        emails.Add(new EmailViewModel
                        {
                            Id = email.Id,
                            Subject = GetHeader(email, "Subject"),
                            From = GetHeader(email, "From"),
                            Date = GetHeader(email, "Date"),
                            Body = GetBody(email),
                            OrginalEmail = email,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // يمكنك إدراج معالجة للأخطاء هنا
                Console.WriteLine($"An error occurred while fetching emails: {ex.Message}");
            }

            return emails;
        }

        private string GetHeader(Message message, string name)
        {
            var header = message.Payload.Headers.FirstOrDefault(h => h.Name == name);
            return header?.Value;
        }
        private string GetBody(Message message)
        {
            var parts = message.Payload.Parts;
            if (parts == null || !parts.Any())
            {
                return message.Payload.Body.Data;
            }

            var part = parts.FirstOrDefault(p => p.MimeType == "text/plain");
            var data = part?.Body?.Data ?? string.Empty;
            var body = Encoding.UTF8.GetString(Convert.FromBase64String(data.Replace('-', '+').Replace('_', '/')));
            return body;
        }
        public IActionResult SendEmail() { return View(); }
        [HttpPost]
        public IActionResult SendEmail(SendEmailViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var mailMessage = new MailMessage
            {
                Subject = model.Subject,
                Body = model.Body,
                IsBodyHtml = true,
                From = new MailAddress(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value)
            };
            mailMessage.To.Add(model.To);

            mailMessage.CC.AddRange(SplitEmails(model.Cc));
            mailMessage.Bcc.AddRange(SplitEmails(model.Bcc));

            // Add attachments
            if (Request.Form.Files != null)
            {
                foreach (var file in Request.Form.Files)
                {
                    var attachment = new Attachment(file.OpenReadStream(), file.FileName);
                    mailMessage.Attachments.Add(attachment);
                }
            }

            // Convert MailMessage to MimeMessage
            var mimeMessage = ConvertToMimeMessage(mailMessage);

            var message = new Message
            {
                Raw = Base64UrlEncode(mimeMessage.ToString())
            };

            try
            {
                _gmailService.Users.Messages.Send(message, "me").Execute();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }

        }
        private MailAddress[] SplitEmails(string emails)
        {
            List<MailAddress> emailsList = new List<MailAddress>();
            if (string.IsNullOrWhiteSpace(emails)) return emailsList.ToArray();
            emails.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(x =>
            {
                if (Regex.Match(x, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase).Success)
                    emailsList.Add(new MailAddress(x));
            });
            return emailsList.ToArray();
        }

        private static string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }


        private MimeMessage ConvertToMimeMessage(MailMessage mailMessage)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(mailMessage.From.DisplayName, mailMessage.From.Address));

            foreach (var to in mailMessage.To)
            {
                mimeMessage.To.Add(new MailboxAddress(to.DisplayName, to.Address));
            }

            foreach (var cc in mailMessage.CC)
            {
                mimeMessage.Cc.Add(new MailboxAddress(cc.DisplayName, cc.Address));
            }

            foreach (var bcc in mailMessage.Bcc)
            {
                mimeMessage.Bcc.Add(new MailboxAddress(bcc.DisplayName, bcc.Address));
            }

            mimeMessage.Subject = mailMessage.Subject;
            return mimeMessage;
        }
    }
}

