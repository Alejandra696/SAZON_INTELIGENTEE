using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories.Models;
using System;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Repositories
{
    public class UserRepository
    {
        private readonly Sazon_inteligenteDBEntities1 _context;

        public UserRepository()
        {
            _context = new Sazon_inteligenteDBEntities1();
        }

        public bool Register(UserDtos userDto)
        {
            try
            {
                using (var ctx = new Sazon_inteligenteDBEntities1())
                {
                    INVENTORY invent = new INVENTORY
                    {
                        ID_User = userDto.ID_User,
                        date_Reg_Invent = DateTime.Now,
                        delete_Food_Invent = false
                    };
                    ctx.INVENTORies.Add(invent);
                    ctx.SaveChanges();

                    userDto.ID_Invent = invent.ID_Invent;
                    ctx.SaveChanges();

                    var user = new USER
                    {
                        name_User = userDto.name_User,
                        email_User = userDto.email_User,
                        phone_User = userDto.phone_User,
                        passw_User = userDto.passw_User,
                        date_Regis_User = userDto.date_Regis_User ?? DateTime.Now,
                        ID_Role = userDto.ID_Role == 0 ? 1 : userDto.ID_Role,
                    };

                    ctx.USERs.Add(user);
                    ctx.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("DEBUG: SaveChanges succeeded.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Muestra en Output de Visual Studio el error completo
                System.Diagnostics.Debug.WriteLine("ERROR UserRepository.Register: " + ex.ToString());
                // opcional: rethrow para que veas la excepción en el debugger
                throw;
            }
        }

        public USER GetUserByEmailAndPassword(string email, string password)
        {
            return _context.USERs.FirstOrDefault(u => u.email_User == email && u.passw_User == password);
        }

        // ============================================================
        // OBTENER USUARIO POR EMAIL (para validaciones del login)
        // ============================================================
        public USER GetUserByEmail(string email)
        {
            try
            {
                return _context.USERs.FirstOrDefault(u => u.email_User.Trim().ToLower() == email.Trim().ToLower());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR en GetUserByEmail (Repository): " + ex.Message);
                return null;
            }
        }

        // ============================================================
        // ACTUALIZAR LA FECHA DEL ÚLTIMO LOGIN DE UN USUARIO
        // ============================================================
        public void UpdateLastLoginUser(string email)
        {
            try
            {
                var user = _context.USERs.FirstOrDefault(u => u.email_User == email);
                if (user != null)
                {
                    user.last_Login_User = DateTime.Now; // 👈 se actualiza la fecha actual
                    _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Último login actualizado para: {email}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR en UpdateLastLoginUser: " + ex.Message);
            }
        }


        // Update user's encrypted password
        public void UpdatePassword(int userId, string encryptedPassword)
        {
            var user = _context.USERs.FirstOrDefault(u => u.ID_User == userId);
            if (user != null)
            {
                user.passw_User = encryptedPassword;
                _context.SaveChanges();
            }
        }
    }
}

