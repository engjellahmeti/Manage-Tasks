using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace Manage_Tasks.Models
{
    class MessageServices
    {
        public async static Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var _email = "engjell.ahmeti.work@outlook.com";
                var _displayName = "Engjëll Ahmeti";
                MailMessage msg = new MailMessage();
                msg.To.Add(email);
                msg.From = new MailAddress(_email, _displayName);
                msg.Subject = subject;
                msg.Body = message;
                msg.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.EnableSsl = true;
                    smtp.Host = "smtp.live.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_email, "Private.Account");
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.SendCompleted += (s, e) => { smtp.Dispose(); };
                    await smtp.SendMailAsync(msg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}