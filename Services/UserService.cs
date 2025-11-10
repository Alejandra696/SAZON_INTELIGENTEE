using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories;
using INTELIGENTE_SAZÓN.Repositories.Models;
using System;
using System.Linq;
using BCrypt.Net;

namespace INTELIGENTE_SAZÓN.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService()
        {
            _userRepository = new UserRepository();
        }

        // ============================================================
        // REGISTRO DE USUARIOS
        // ============================================================
        public bool Register(UserDtos userDto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Entering UserService.Register");

                // ✅ Validaciones básicas — todos los campos obligatorios
                if (string.IsNullOrWhiteSpace(userDto.name_User) ||
                    string.IsNullOrWhiteSpace(userDto.email_User) ||
                    string.IsNullOrWhiteSpace(userDto.phone_User) ||
                    string.IsNullOrWhiteSpace(userDto.passw_User) ||
                    string.IsNullOrWhiteSpace(userDto.ConfirmPassword))
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Missing required fields.");
                    throw new Exception("Todos los campos son obligatorios.");
                }

                // ✅ Limpiar teléfono y validar
                userDto.phone_User = userDto.phone_User?.Trim(); // quitar espacios
                string phoneDigits = new string(userDto.phone_User.Where(char.IsDigit).ToArray());

                if (phoneDigits.Length != 10)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Teléfono inválido ({phoneDigits}).");
                    throw new Exception("El teléfono debe tener exactamente 10 dígitos numéricos.");
                }

                userDto.phone_User = phoneDigits; // guardar limpio en BD

                // ✅ Validación: contraseñas deben coincidir
                if (userDto.passw_User != userDto.ConfirmPassword)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Passwords do not match.");
                    throw new Exception("Las contraseñas no coinciden.");
                }

                // ✅ Encriptar contraseña antes de guardar (usando BCrypt)
                userDto.passw_User = HashPassword(userDto.passw_User);
                System.Diagnostics.Debug.WriteLine("DEBUG: Password hashed -> length: " + userDto.passw_User.Length);

                // ✅ Llamar al repositorio para guardar en BD
                var result = _userRepository.Register(userDto);
                System.Diagnostics.Debug.WriteLine("DEBUG: Repository.Register returned: " + result);

                // ✅ Si el registro fue exitoso
                if (!result)
                    throw new Exception("Error al guardar el usuario en la base de datos.");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR UserService.Register: " + ex.ToString());
                throw; // ⚠️ Deja este throw para que el Controller capture el mensaje
            }
        }

        // ============================================================
        // LOGIN DE USUARIOS
        // ============================================================
        public bool Login(string email, string password)
        {
            try
            {
                // Buscar el usuario por email
                USER user = _userRepository.GetUserByEmail(email);

                if (user == null)
                    return false;

                // ✅ Verificar la contraseña usando BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.passw_User);

                return isPasswordValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en Login: " + ex.Message);
                return false;
            }
        }

        // ============================================================
        // OBTENER USUARIO POR EMAIL
        // ============================================================
        public USER GetUserByEmail(string email)
        {
            try
            {
                return _userRepository.GetUserByEmail(email);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR en GetUserByEmail (Service): " + ex.Message);
                return null;
            }
        }

        // ============================================================
        // ENCRIPTACIÓN DE CONTRASEÑA (BCrypt)
        // ============================================================
        private string HashPassword(string password)
        {
            // ✅ Genera el hash con salt automático (más seguro)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // ============================================================
        // 🔁 ACTUALIZAR FECHA DE ÚLTIMO LOGIN (USUARIOS NORMALES)
        // ============================================================
        public void UpdateLastLoginUser(string email)
        {
            try
            {
                var user = _userRepository.GetUserByEmail(email);
                if (user != null)
                {
                    user.last_Login_User = DateTime.Now;

                    using (var ctx = new Sazon_inteligenteDBEntities1())
                    {
                        var dbUser = ctx.USERs.FirstOrDefault(u => u.email_User == email);
                        if (dbUser != null)
                        {
                            dbUser.last_Login_User = DateTime.Now;
                            ctx.SaveChanges();
                            System.Diagnostics.Debug.WriteLine($"✅ Último login actualizado para usuario: {email}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR en UpdateLastLoginUser: " + ex.Message);
            }
        }
    }
}
