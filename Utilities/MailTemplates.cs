using System;
using System.IO;

namespace INTELIGENTE_SAZÓN.Utilities
{
    public static class MailTemplates
    {
        public static string BuildPasswordRecoveryEmail(string temporaryPassword)
        {
            try
            {
                // Ruta del HTML base
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utilities","RecoveryPassword.html");

                // Leer el contenido del HTML
                string body = File.ReadAllText(templatePath);

                // Reemplazar el marcador por la contraseña temporal
                body = body.Replace("{{claveTemporal}}", temporaryPassword);

                return body;
            }
            catch (Exception ex)
            {
                throw new Exception("Error building email template: " + ex.Message);
            }
        }
    }
}