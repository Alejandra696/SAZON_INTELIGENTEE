using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace INTELIGENTE_SAZÓN.Dtos
{
    public class ProfessionalProfileDtos
    {
        public int ID_Profile { get; set; }          // Autoincremental en la BD
        public string name_User_Profile { get; set; }
        public string email_User_Profile { get; set; }
        public string passw_User_Profile { get; set; }
        public string Confirm_Password_Profile { get; set; }
        public string cert_Profile { get; set; }     // Ruta o nombre del PDF del certificado
        public DateTime ? date_Regis_Profile { get; set; }  // Fecha de registro del perfil
        public bool ? status_Profile { get; set; }     // Activo (1) o Inactivo (0)
        public string time_labo { get; set; }        // Ejemplo: "30 días" (varchar(10))
        public int  ? ID_Role { get; set; }
        public DateTime? Last_Login_Profile { get; set; }
    }
}