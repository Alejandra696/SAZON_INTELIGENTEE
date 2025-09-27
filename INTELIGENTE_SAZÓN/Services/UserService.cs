using INTELIGENTE_SAZÓN.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sazon_Inteligente.Services
{
    public class UserService
    {
        public bool Register(UserDtos User)
        {
            // AQUÍ PODEMOS GUARDARLO EN NUESTRA BASE DE DATOS, POR AHORA LO TENEMOS POR EL CAMINO FELIZ
            return true;
        }
        public bool Login(string username, string password)
        {
            // AQUÍ SE HACE LA VALIDACIÓN REAL DE LAS CREDENCIALES
            return username == "admin" && password == "1234";
        }

    }
}

