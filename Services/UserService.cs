using INTELIGENTE_SAZÓN.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace INTELIGENTE_SAZÓN.Services
{
    public class UserService
    {
        public bool Register(UserDtos user)
        {
            // AQUÍ GUARDAREMOS TODO LO DE LA BASE DE DATOS, POR AHORA ESTA POR EL CAMINO FELIZ
            return true;

        }

        public bool Login(string username, string password)
        {
            // AQUI VAN A IR TODAS LAS VALIDACIONES REALES
            return username == "admin" && password == "1234";
        }


        public bool CreateProfessionalProfile(ProfePerDtos dto)
        {
            try
            {
                string hashedPassword = HashPassword(dto.Password);

                string folderPath = System.Web.HttpContext.Current.Server.MapPath("~/Content/Certificates/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = Path.GetFileName(dto.Certificate.FileName);
                string savePath = Path.Combine(folderPath, fileName);
                dto.Certificate.SaveAs(savePath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}