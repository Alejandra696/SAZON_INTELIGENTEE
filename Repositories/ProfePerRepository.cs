using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Repositories
{
    public class ProfePerRepository
    {
        private readonly Sazon_inteligenteDBEntities1 _context;

        public ProfePerRepository()
        {
            _context = new Sazon_inteligenteDBEntities1();
        }

        // ============================================================
        // INSERTAR PERFIL PROFESIONAL 
        // ============================================================
        public bool InsertProfessionalProfile(ProfessionalProfileDtos ProfePerDto)
        {
            try
            {
                // 🟣 Crear la entidad del perfil profesional
                var newProfile = new PROFESSIONAL_PROFILE
                {
                    name_User_Profile = ProfePerDto.name_User_Profile,
                    email_User_Profile = ProfePerDto.email_User_Profile,
                    passw_User_Profile = ProfePerDto.passw_User_Profile,
                    cert_Profile = ProfePerDto.cert_Profile,
                    date_Regis_Profile = ProfePerDto.date_Regis_Profile,
                    status_Profile = ProfePerDto.status_Profile ?? false,
                    time_labo = ProfePerDto.time_labo,
                    ID_Role = ProfePerDto.ID_Role
                };

                _context.PROFESSIONAL_PROFILEs.Add(newProfile);
                _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine("✅ Perfil profesional insertado correctamente con ID_User: " + ProfePerDto.email_User_Profile);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error en ProfePerRepository.InsertProfessionalProfile: " + ex.ToString());
                return false;
            }
        }

        // ============================================================
        // OBTENER TODOS LOS PERFILES (DTO)
        // ============================================================
        public List<ProfessionalProfileDtos> GetAllProfiles()
        {
            try
            {
                return _context.PROFESSIONAL_PROFILEs
                    .Select(p => new ProfessionalProfileDtos
                    {
                        ID_Profile = p.ID_Profile,
                        name_User_Profile = p.name_User_Profile,
                        email_User_Profile = p.email_User_Profile,
                        passw_User_Profile = p.passw_User_Profile,
                        cert_Profile = p.cert_Profile,
                        date_Regis_Profile = p.date_Regis_Profile,
                        status_Profile = p.status_Profile ?? false,
                        time_labo = p.time_labo,
                        ID_Role = p.ID_Role
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ProfePerRepository.GetAllProfiles: " + ex.Message);
                return new List<ProfessionalProfileDtos>();
            }
        }

        // ============================================================
        // ACTUALIZAR ESTADO (ACTIVAR / DESACTIVAR)
        // ============================================================
        public bool UpdateProfileStatus(int idProfile, bool status)
        {
            try
            {
                var profile = _context.PROFESSIONAL_PROFILEs.FirstOrDefault(p => p.ID_Profile == idProfile);
                if (profile != null)
                {
                    profile.status_Profile = status;
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ProfePerRepository.UpdateProfileStatus: " + ex.Message);
                return false;
            }
        }
        // ============================================================
        // 🔁 ACTUALIZAR FECHA DE ÚLTIMO LOGIN DEL PERFIL PROFESIONAL
        // ============================================================
        public void UpdateLastLoginProfile(string email)
        {
            try
            {
                var profile = _context.PROFESSIONAL_PROFILEs.FirstOrDefault(p => p.email_User_Profile == email);
                if (profile != null)
                {
                    profile.Last_Login_Profile = DateTime.Now; // 👈 Actualiza con la fecha actual
                    _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Último login actualizado para perfil: {email}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR en UpdateLastLoginProfile: " + ex.Message);
            }
        }
    }
}
