using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BCrypt.Net;

namespace INTELIGENTE_SAZÓN.Services
{
    public class ProfePerService
    {
        private readonly ProfePerRepository _repo;

        public ProfePerService()
        {
            _repo = new ProfePerRepository();
        }

        // ============================================================
        // INSERTAR PERFIL PROFESIONAL 
        // ============================================================
        public bool InsertProfessionalProfile(ProfessionalProfileDtos dto, HttpPostedFileBase file)
        {
            try
            {
                // --- Validaciones de negocio
                if (dto == null) throw new ArgumentNullException(nameof(dto));
                if (string.IsNullOrWhiteSpace(dto.name_User_Profile)) throw new Exception("El nombre es obligatorio.");
                if (string.IsNullOrWhiteSpace(dto.email_User_Profile)) throw new Exception("El correo es obligatorio.");
                if (string.IsNullOrWhiteSpace(dto.passw_User_Profile)) throw new Exception("La contraseña es obligatoria.");
                if (dto.passw_User_Profile != dto.Confirm_Password_Profile) throw new Exception("Las contraseñas no coinciden.");
                if (!(dto.ID_Role == 2 || dto.ID_Role == 3)) throw new Exception("El rol debe ser 2 (Chef) o 3 (Ingeniero).");

                // --- Archivos: validar y guardar (si vienen)
                string savedFilePath = null;
                if (file != null && file.ContentLength > 0)
                {
                    var allowed = new[] { ".pdf" };
                    var ext = Path.GetExtension(file.FileName)?.ToLower();

                    if (!allowed.Contains(ext))
                        throw new Exception("El certificado debe estar en formato PDF.");

                    const int maxBytes = 5 * 1024 * 1024;
                    if (file.ContentLength > maxBytes)
                        throw new Exception("El archivo supera 5 MB.");

                    var folder = HttpContext.Current.Server.MapPath("~/Content/certificates");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var unique = Guid.NewGuid().ToString() + ext;
                    var path = Path.Combine(folder, unique);
                    file.SaveAs(path);
                    savedFilePath = "/Content/certificates/" + unique;
                }

                // --- Asignaciones que corresponden a BD (fecha, estado, ruta)
                dto.cert_Profile = savedFilePath;
                dto.date_Regis_Profile = DateTime.Now;
                dto.status_Profile = true;

                // ============================================================
                // 🔒 Encriptar contraseña con BCrypt antes de guardar
                // ============================================================
                dto.passw_User_Profile = BCrypt.Net.BCrypt.HashPassword(dto.passw_User_Profile);

                // ============================================================
                // ✅ Guardar en base de datos
                // ============================================================
                var ok = _repo.InsertProfessionalProfile(dto);
                if (!ok)
                    throw new Exception("Error al registrar el perfil profesional.");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR ProfePerService.InsertProfessionalProfile: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Devuelve todos los perfiles como DTO (repo hace el mapeo)
        /// </summary>
        public IEnumerable<ProfessionalProfileDtos> GetAllProfiles()
        {
            try
            {
                return _repo.GetAllProfiles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR ProfePerService.GetAllProfiles: " + ex.ToString());
                return new List<ProfessionalProfileDtos>();
            }
        }

        /// <summary>
        /// Actualiza estados a partir de un FormCollection (controller envía el FormCollection).
        /// El service interpreta el form y llama al repo para actualizar cada uno.
        /// </summary>
        public bool UpdateProfileStatus(FormCollection form)
        {
            try
            {
                var keys = form.AllKeys.Where(k => k != null && k.StartsWith("status_")).ToList();
                foreach (var key in keys)
                {
                    var idStr = key.Replace("status_", "");
                    if (int.TryParse(idStr, out int id))
                    {
                        bool newStatus = form[key] != null;
                        _repo.UpdateProfileStatus(id, newStatus);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR ProfePerService.UpdateProfileStatus: " + ex.ToString());
                return false;
            }
        }

        // ============================================================
        // 🔁 ACTUALIZAR FECHA DE ÚLTIMO LOGIN (desde Service)
        // ============================================================
        public void UpdateLastLoginProfile(string email)
        {
            try
            {
                _repo.UpdateLastLoginProfile(email);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR ProfePerService.UpdateLastLoginProfile: " + ex.Message);
            }
        }
    }
}