using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.IO;

namespace ent.manager.WebApi.Helpers
{
    public class EmailHelper
    {
        private static IConfigurationRoot _configuration;

         static EmailHelper()
        {
            _configuration = CommonHelper.GetConfigurationObject();
        }

        public static bool SendWelcomeEmail(string from, string to, string subject, string welcomelink, string fname, string lname, string partnerenterprisetext,string partnerenterprisename)
        {
            var result = false;

            try
            {

                SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(from)); 
                mailMessage.To.Add(new MailboxAddress(to));
                    

     
                var bodyBuilder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText("Edm/welcome_email_template.html"))
                {
                    bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
                }

                bodyBuilder.HtmlBody =  bodyBuilder.HtmlBody.Replace("##username##", to);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##link##", welcomelink);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##partnerenterprisetext##", partnerenterprisetext);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##partnerenterprisename##", partnerenterprisename);

                if(string.IsNullOrEmpty(fname) && string.IsNullOrEmpty(lname))
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", to);
                }
                else
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", fname + " " + lname);
                }

                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.Subject = subject;
                client.Send(mailMessage);

                result = true;
            }
            catch 
            {   
            }
            return result;
           
        }

        public static bool SendPasswordSetEmail(string from, string to, string subject, string link, string fname, string lname)
        {
         

            var result = false;

            try
            {

                SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(from));
                mailMessage.To.Add(new MailboxAddress(to));



                var bodyBuilder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText("Edm/password_set_template.html"))
                {
                    bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
                }

                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##username##", to);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##link##", link);

                if (string.IsNullOrEmpty(fname) && string.IsNullOrEmpty(lname))
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", to);
                }
                else
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", fname + " " + lname);
                }


                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.Subject = subject;
                client.Send(mailMessage);

                result = true;
            }
            catch  
            {
            }
            return result;

        }

        public static bool SendInstructionsEmail(string from, string to, string subject, string subusername,string subpin, string downloadLocation, string productName, string fname, string lname, string substartdate, string maxseatcount)
        {
           

            var result = false;

            try
            {

                //SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(from));
                mailMessage.To.Add(new MailboxAddress(to));



                var bodyBuilder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText("Edm/instructions_email_template.html"))
                {
                    bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
                }

                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##subun##", subusername);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##subpin##", subpin);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##dl##", downloadLocation);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##product##", productName);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##username##", to);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##substartdate##", substartdate);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##maxseatcount##", maxseatcount);

                if (string.IsNullOrEmpty(fname) && string.IsNullOrEmpty(lname))
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", to);
                }
                else
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", fname + " " + lname);
                }

                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.Subject = subject;

                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect(_configuration["smtp:client"]);
                    client.Authenticate(_configuration["smtp:username"], _configuration["smtp:password"]);
                    client.Send(mailMessage);
                }

                result = true;
            }
            catch 
            {
            }
            return result;

        }


        public static bool SendInstructionsEmail(string from, string to, string subject, string subusername, string subpin, string downloadLocation, string productName, string fname, string lname, string substartdate, string maxseatcount, SmtpClient client)
        {


            var result = false;

            try
            {

                //SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(from));
                mailMessage.To.Add(new MailboxAddress(to));



                var bodyBuilder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText("Edm/instructions_email_template.html"))
                {
                    bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
                }

                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##subun##", subusername);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##subpin##", subpin);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##dl##", downloadLocation);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##product##", productName);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##username##", to);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##substartdate##", substartdate);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##maxseatcount##", maxseatcount);

                if (string.IsNullOrEmpty(fname) && string.IsNullOrEmpty(lname))
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", to);
                }
                else
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", fname + " " + lname);
                }

                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.Subject = subject;

                client.Send(mailMessage);
               

                result = true;
            }
            catch
            {
            }
            return result;

        }

        public static bool SendInstructionsEmailLegacy(string from, string to, string subject, string licenceKey,string downloadLocation, string productName, string fname, string lname, string substartdate, string maxseatcount)
        {


            var result = false;

            try
            {

                //SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(from));
                mailMessage.To.Add(new MailboxAddress(to));



                var bodyBuilder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText("Edm/instructions_legacy_email_template.html"))
                {
                    bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
                }

                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##lk##", licenceKey);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##dl##", downloadLocation);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##product##", productName);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##username##", to);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##substartdate##", substartdate);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##maxseatcount##", maxseatcount);

                if (string.IsNullOrEmpty(fname) && string.IsNullOrEmpty(lname))
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", to);
                }
                else
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", fname + " " + lname);
                }

                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.Subject = subject;

                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect(_configuration["smtp:client"]);
                    client.Authenticate(_configuration["smtp:username"], _configuration["smtp:password"]);
                    client.Send(mailMessage);
                }
              
                 
                result = true;
            }
            catch
            {
            }
            return result;

        }


        public static bool SendInstructionsEmailLegacy(string from, string to, string subject, string licenceKey, string downloadLocation, string productName, string fname, string lname, string substartdate, string maxseatcount, SmtpClient client)
        {


            var result = false;

            try
            {

                //SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(from));
                mailMessage.To.Add(new MailboxAddress(to));



                var bodyBuilder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText("Edm/instructions_legacy_email_template.html"))
                {
                    bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
                }

                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##lk##", licenceKey);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##dl##", downloadLocation);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##product##", productName);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##username##", to);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##substartdate##", substartdate);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##maxseatcount##", maxseatcount);

                if (string.IsNullOrEmpty(fname) && string.IsNullOrEmpty(lname))
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", to);
                }
                else
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", fname + " " + lname);
                }

                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.Subject = subject;

               
                client.Send(mailMessage);
              


                result = true;
            }
            catch
            {
            }
            return result;

        }

        public static bool SendSubscriptionCancellationEmail(string from, string to, string subject, string licenceKey, string productName, string fname, string lname)
        {
            var result = false;

            try
            {

                SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(from));
                mailMessage.To.Add(new MailboxAddress(to));

                var bodyBuilder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText("Edm/subscription_cancellation_email_template.html"))
                {
                    bodyBuilder.HtmlBody = SourceReader.ReadToEnd();
                }

                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##lk##", licenceKey);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##product##", productName);
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##username##", to);

                if (string.IsNullOrEmpty(fname) && string.IsNullOrEmpty(lname))
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", to);
                }
                else
                {
                    bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace("##fullname##", fname + " " + lname);
                }

                mailMessage.Body = bodyBuilder.ToMessageBody();
                mailMessage.Subject = subject;
                client.Send(mailMessage);

                result = true;
            }
            catch
            {
            }
            return result;

        }

        public static bool SendReportFailed(string from, string toCSL, string subject, string body)
        {
            
            var result = false;

                try
                {

                SmtpClient client = GetClient();

                MimeMessage mailMessage = new MimeMessage();
                    mailMessage.From.Add(new MailboxAddress(from));


                    var toSplit = toCSL.Split(',');

                    foreach (var item in toSplit)
                    {
                        mailMessage.To.Add(new MailboxAddress(item));
                    }

                    var bodyBuilder = new BodyBuilder();

                    bodyBuilder.TextBody = body;
                    mailMessage.Subject = subject;
                    mailMessage.Body = bodyBuilder.ToMessageBody();
                    client.Send(mailMessage);

                    result = true;

                }
                catch  
                {
                }
          

            return result;

        }

        private static SmtpClient GetClient()
        {
            SmtpClient client = new SmtpClient();

            client.Connect(_configuration["smtp:client"]);

            client.Authenticate(_configuration["smtp:username"], _configuration["smtp:password"]);

            return client;
        }
    }
}
