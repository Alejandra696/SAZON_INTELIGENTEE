using System;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace INTELIGENTE_SAZÓN.Utilities
{
    public class MailManager
    {
        private SmtpClient _cliente;
        private MailMessage mail;

        private string Host = "smtp.gmail.com";
        private int Port = 587;
        private string User = "noreplysazoninteligente@gmail.com"; // Correo de envío
        private string Password = "bhsinjdvjziybcqf"; // Contraseña de aplicación
        private bool EnabledSSL = true;

        // ============================
        // 🔹 Constructor
        // ============================
        public MailManager()
        {
            _cliente = new SmtpClient(Host, Port)
            {
                EnableSsl = EnabledSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(User, Password)
            };
        }

        // ============================
        // 🔹 Función Enviar Correo (con manejo de errores)
        // ============================
        public void SendMail(string addressee, string subject, string messagebody, bool html = true)
        {
            try
            {
                mail = new MailMessage(User, addressee, subject, messagebody);
                mail.IsBodyHtml = html;

                _cliente.Send(mail);
            }
            catch (Exception ex)
            {
                // Aquí podrías registrar el error o lanzar una excepción controlada
                throw new Exception("Error al enviar el correo: " + ex.Message);
            }

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities", "RecoveryPassword.html");
            string body = File.ReadAllText(templatePath);

            // Reemplazar la clave temporal
            body = body.Replace("{{bhsinjdvjziybcqf}}", Password);

            // Luego configuramos el mensaje
            MailMessage email = new MailMessage(User, addressee, subject, body);
            email.IsBodyHtml = true;
        }
    }
}
