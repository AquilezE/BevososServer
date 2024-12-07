using System;

using System.Net;
using System.Net.Mail;


namespace BevososService.Utils
{
    public class EmailUtils
    {

        private const string SenderEmail = "bevososthegame@gmail.com";
        private const string SenderPassword = "lkzz bghz bwol leiz";
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;

        protected EmailUtils()
        {

        }

        public static bool SendInvitationByEmail(string recipientEmail, int lobbyId)
        {
            var subject = "Bevosos Invite";
            var body = $"You've been invited to play! Join this lobby before it's too late: {lobbyId}";

            return SendEmail(recipientEmail, subject, body);
        }

        public static bool SendTokenByEmail(string recipientEmail, string token)
        {
            var subject = "Bevosos";
            var body = $"Your verification token is: {token}";

            return SendEmail(recipientEmail, subject, body);
        }

        private static bool SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                using (var mail = new MailMessage())
                using (var smtpClient = new SmtpClient(SmtpServer, SmtpPort))
                {
                    mail.From = new MailAddress(SenderEmail);
                    mail.To.Add(recipientEmail);
                    mail.Subject = subject;
                    mail.Body = body;

                    smtpClient.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(mail);
                }

                return true;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                Console.WriteLine($"Failed to deliver email to one or more recipients: {ex.Message}");
                return false;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP error occurred: {ex.Message}");
                return false;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Invalid email format: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while sending email: {ex.Message}");
                return false;
            }
        }
    }


}
