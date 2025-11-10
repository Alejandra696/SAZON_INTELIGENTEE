using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Repositories
{
    public class ReportRepository
    {
        private readonly Sazon_inteligenteDBEntities1 _context;

        public ReportRepository()
        {
            _context = new Sazon_inteligenteDBEntities1();
        }

        /// <summary>
        /// Devuelve los usuarios (tanto USER como PROFESSIONAL_PROFILE) registrados
        /// en el último mes como UserCombinedDtos.
        /// </summary>
        public List<UserCombinedDtos> GetAllUsersCombined()
        {
            try
            {
                DateTime start = DateTime.Now.Date.AddMonths(-1);

                // Usuarios de la tabla USER
                var users = _context.USERs
                    .Where(u => u.date_Regis_User != null && u.date_Regis_User >= start)
                    .Select(u => new UserCombinedDtos
                    {
                        Id = u.ID_User,
                        Name = u.name_User,
                        Email = u.email_User,
                        DateRegisUser = u.date_Regis_User,
                        LastLogin = null, // si tienes campo de último login, reemplaza
                        RoleId = u.ID_Role,
                        RoleName = u.ID_Role == 1 ? "Usuario" : (u.ID_Role == 2 ? "Chef" : (u.ID_Role == 3 ? "Ingeniero de Alimentos" : (u.ID_Role == 4 ? "Administrador" : "Otro"))),

                    });

                // Perfiles profesionales (tabla PROFESSIONAL_PROFILE)
                var profs = _context.PROFESSIONAL_PROFILEs
                    .Where(p => p.date_Regis_Profile != null && p.date_Regis_Profile >= start)
                    .Select(p => new UserCombinedDtos
                    {
                        Id = p.ID_Profile,
                        Name = p.name_User_Profile,
                        Email = p.email_User_Profile,
                        DateRegisUser = p.date_Regis_Profile,
                        LastLogin = null,
                        RoleId = p.ID_Role,
                        RoleName = p.ID_Role == 2 ? "Chef" : (p.ID_Role == 3 ? "Ingeniero de Alimentos" : "Otro"),
                        IsActive = p.status_Profile ?? false
                    });

                // Unir y devolver
                var result = users.Union(profs).OrderBy(u => u.DateRegisUser).ToList();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error ReportRepository.GetNewUsersLastMonth: " + ex.Message);
                return new List<UserCombinedDtos>();
            }
        }
        // ============================================================
        // REPORTE: TODOS LOS USUARIOS REGISTRADOS (NO SOLO DEL MES)
        // ============================================================
        public List<UserCombinedDtos> GetAllRegisteredUsers()
        {
            try
            {
                // Usuarios normales de la tabla USER
                var users = _context.USERs
                    .Select(u => new UserCombinedDtos
                    {
                        Id = u.ID_User,
                        Name = u.name_User,
                        Email = u.email_User,
                        DateRegisUser = u.date_Regis_User,
                        LastLogin = null,
                        RoleId = u.ID_Role,
                        RoleName =
                            u.ID_Role == 1 ? "Usuario" :
                            u.ID_Role == 2 ? "Chef" :
                            u.ID_Role == 3 ? "Ingeniero de Alimentos" :
                            u.ID_Role == 4 ? "Administrador" :
                            "Otro",
                        IsActive = true
                    });

                // Profesionales (Chef / Ingeniero) de la tabla PROFESSIONAL_PROFILE
                var profs = _context.PROFESSIONAL_PROFILEs
                    .Select(p => new UserCombinedDtos
                    {
                        Id = p.ID_Profile,
                        Name = p.name_User_Profile,
                        Email = p.email_User_Profile,
                        DateRegisUser = p.date_Regis_Profile,
                        LastLogin = null,
                        RoleId = p.ID_Role,
                        RoleName =
                            p.ID_Role == 2 ? "Chef" :
                            p.ID_Role == 3 ? "Ingeniero de Alimentos" :
                            "Otro",
                        IsActive = p.status_Profile ?? false
                    });

                // Unimos ambos conjuntos y los ordenamos por ID
                var allUsers = users.Union(profs).OrderBy(u => u.Id).ToList();

                return allUsers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR en GetAllRegisteredUsers: " + ex.ToString());
                return new List<UserCombinedDtos>();
            }
        }
        // ============================================================
        // OBTENER USUARIOS RETIRADOS (más de 1 mes sin iniciar sesión)
        // ============================================================
        public List<UserCombinedDtos> GetRetiredUsers(int monthsWithoutLogin = 1)
        {
            try
            {
                DateTime threshold = DateTime.Now.AddMonths(-monthsWithoutLogin);

                // 🔹 Usuarios normales
                var retiredUsers = _context.USERs
                    .Where(u => u.last_Login_User != null && u.last_Login_User <= threshold)
                    .Select(u => new UserCombinedDtos
                    {
                        Id = u.ID_User,
                        Name = u.name_User,
                        Email = u.email_User,
                        DateRegisUser = u.date_Regis_User,
                        LastLogin = u.last_Login_User,
                        RoleId = u.ID_Role,
                        RoleName = u.ID_Role == 1 ? "Usuario" :
                                   u.ID_Role == 2 ? "Chef" :
                                   u.ID_Role == 3 ? "Ingeniero de Alimentos" :
                                   u.ID_Role == 4 ? "Administrador" : "Otro",
                        IsActive = true
                    });

                // 🔹 Perfiles profesionales (Chef / Ingeniero)
                var retiredProfs = _context.PROFESSIONAL_PROFILEs
                    .Where(p => p.Last_Login_Profile != null && p.Last_Login_Profile <= threshold)
                    .Select(p => new UserCombinedDtos
                    {
                        Id = p.ID_Profile,
                        Name = p.name_User_Profile,
                        Email = p.email_User_Profile,
                        DateRegisUser = p.date_Regis_Profile,
                        LastLogin = p.Last_Login_Profile,
                        RoleId = p.ID_Role,
                        RoleName = p.ID_Role == 2 ? "Chef" :
                                   p.ID_Role == 3 ? "Ingeniero de Alimentos" : "Otro",
                        IsActive = p.status_Profile ?? false
                    });

                var result = retiredUsers.Union(retiredProfs).OrderBy(u => u.LastLogin).ToList();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error ReportRepository.GetRetiredUsers: " + ex.Message);
                return new List<UserCombinedDtos>();
            }
        }
        // ============================================================
        // OBTENER USUARIOS ACTIVOS (login en el ultimo mes)
        // ============================================================
        public List<UserCombinedDtos> GetActiveUsers(DateTime cutoff)
        {
            try
            {
                // Usuarios (tabla USER)
                var users = _context.USERs
                    .Where(u => u.last_Login_User != null && u.last_Login_User >= cutoff)
                    .Select(u => new UserCombinedDtos
                    {
                        Id = u.ID_User,
                        Name = u.name_User,
                        Email = u.email_User,
                        DateRegisUser = u.date_Regis_User,
                        LastLogin = u.last_Login_User,
                        RoleId = u.ID_Role,
                        RoleName = u.ID_Role == 1 ? "Usuario" :
                                   (u.ID_Role == 2 ? "Chef" :
                                   (u.ID_Role == 3 ? "Ingeniero de Alimentos" :
                                   (u.ID_Role == 4 ? "Administrador" : "Otro"))),
                        IsActive = true
                    });

                // Perfiles profesionales (tabla PROFESSIONAL_PROFILE)
                var profs = _context.PROFESSIONAL_PROFILEs
                    .Where(p => p.Last_Login_Profile != null && p.Last_Login_Profile >= cutoff)
                    .Select(p => new UserCombinedDtos
                    {
                        Id = p.ID_Profile,
                        Name = p.name_User_Profile,
                        Email = p.email_User_Profile,
                        DateRegisUser = p.date_Regis_Profile,
                        LastLogin = p.Last_Login_Profile,
                        RoleId = p.ID_Role,
                        RoleName = p.ID_Role == 2 ? "Chef" :
                                   (p.ID_Role == 3 ? "Ingeniero de Alimentos" : "Otro"),
                        IsActive = true
                    });

                var result = users.Union(profs).OrderByDescending(u => u.LastLogin).ToList();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error ReportRepository.GetActiveUsers: " + ex.Message);
                return new List<UserCombinedDtos>();
            }
        }
    }
}