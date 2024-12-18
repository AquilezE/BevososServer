using System;
using System.Net;
using System.Net.Mail;
using DataAccess.Utils;


namespace BevososService.Utils
{

    public class EmailUtils
    {

        private const string SenderEmail = "bevososthegame@gmail.com";
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;

        protected EmailUtils()
        {
        }

        public static bool SendInvitationByEmail(string recipientEmail, int lobbyId)
        {
            string subject = "Bevosos Invite";
            string body = $"You've been invited to play! Join this lobby before it's too late: {lobbyId}";

            return SendEmail(recipientEmail, subject, body);
        }

        public static bool SendTokenByEmail(string recipientEmail, string token)
        {
            string subject = "Bevosos";
            string body = $"Your verification token is: {token}";

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

                    smtpClient.Credentials =
                        new NetworkCredential(SenderEmail, Environment.GetEnvironmentVariable("EmailPassword"));
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(mail);
                }

                return true;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (SmtpException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (FormatException ex)
            {
                ExceptionManager.LogErrorException(ex);
                return false;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogFatalException(ex);
                return false;
            }
        }

    }

}