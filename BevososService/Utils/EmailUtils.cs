using System;

using System.Net;
using System.Net.Mail;


namespace BevososService.Utils
{
    internal class EmailUtils
    {
        public static int GenerateToken()
        {
            Random random = new Random();
            int token = random.Next(100000, 999999);
            return token;
        }

        public static bool SendTokenByEmail(string recipientEmail, string token)
        {

            bool emailSent = true;

            string senderEmail = "bevososthegame@gmail.com";
            string senderPassword = "lkzz bghz bwol leiz";
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;

            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);

            mail.From = new MailAddress(senderEmail);
            mail.To.Add(recipientEmail);
            mail.Subject = "Bevosos";
            mail.Body = $"{token}";

            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;


            try
            {
                smtpClient.Send(mail);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                emailSent = false;
            }

            return emailSent;
        }
    }


}
