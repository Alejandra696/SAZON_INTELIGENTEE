using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace INTELIGENTE_SAZÓN.Dtos
    {
        public class UserDtos
        {
            public int ID_User { get; set; }             // Llave primaria
            public string name_User { get; set; }
            public string email_User { get; set; }
            public string phone_User { get; set; }
            public string passw_User { get; set; }
            public string ConfirmPassword { get; set; }  // Campo temporal para vista (no se guarda en BD)
            public System.DateTime? date_Regis_User { get; set; }

            public int ID_Role { get; set; }             // Llave foránea
            public int ID_Invent { get; set; }           // Llave foránea
        }

        public class LoginDtos
        {
            public string name_User { get; set; }
            public string passw_User { get; set; }
            public string email_User { get; set; }
            public DateTime? date_Regis_User { get; set; }
        }
    }
