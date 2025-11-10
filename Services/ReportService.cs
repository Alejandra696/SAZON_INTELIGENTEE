using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories;
using INTELIGENTE_SAZÓN.Utilities; // Asegúrate de que este namespace sea correcto
using System;
using System.Collections.Generic;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Services
{
    public class ReportService
    {
        private readonly ReportRepository _reportRepository;

        public ReportService()
        {
            _reportRepository = new ReportRepository();
        }

        // ============================================================
        // GENERAR PDF SOLO DE USUARIOS NUEVOS (último mes)
        // ============================================================
        public byte[] GenerateNewUsersPdf(string usuarioActual = null)
        {
            try
            {
                // Calcula rango de fechas del último mes
                DateTime startDate = DateTime.Now.AddMonths(-1);
                DateTime today = DateTime.Now;

                // Filtramos los usuarios nuevos según la fecha de registro
                var usuariosNuevos = _reportRepository
                    .GetAllUsersCombined()
                    .Where(u => u.DateRegisUser >= startDate && u.DateRegisUser <= today)
                    .ToList();

                // Si no se pasa el nombre del usuario, usamos el actual logueado
                if (string.IsNullOrWhiteSpace(usuarioActual))
                    usuarioActual = System.Web.HttpContext.Current?.User?.Identity?.Name ?? "Administrador";

                // Generar el PDF usando el formato de tu diseño (ReportPdfGenerator)
                return ReportPdfGenerator.GenerateNewUsersPdf(usuariosNuevos, usuarioActual);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ReportService.GenerateNewUsersPdf: " + ex.Message);
                return new byte[0];
            }
        }
        // ============================================================
        // REPORTE: TODOS LOS USUARIOS REGISTRADOS
        // ============================================================
        public byte[] GenerateAllRegisteredUsersReport(string usuario)
        {
            try
            {
                // ✅ Crea instancia local del repositorio (igual que en el otro método)
                var repo = new ReportRepository();

                // ✅ Obtiene todos los usuarios (de USER y PROFESSIONAL_PROFILE)
                var allUsers = repo.GetAllRegisteredUsers();

                if (allUsers == null || !allUsers.Any())
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No se encontraron usuarios registrados.");
                    return new byte[0];
                }

                // ✅ Genera el PDF (mismo estilo que el de usuarios nuevos)
                return ReportPdfGenerator.GenerateRegisteredUsersPdf(allUsers, usuario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR ReportService.GenerateAllRegisteredUsersReport: " + ex.ToString());
                return new byte[0];
            }
        }
        // ============================================================
        // REPORTE: USUARIOS RETIRADOS (sin login en 1 mes)
        // ============================================================
        public byte[] GenerateRetiredUsersReport(string usuario)
        {
            try
            {
                var repo = new ReportRepository();
                var retiredUsers = repo.GetRetiredUsers(1); // 1 mes sin login

                if (retiredUsers == null) 
                {
                    retiredUsers = new List<UserCombinedDtos>();
                }
                System.Diagnostics.Debug.WriteLine($"INFO: Usuarios retirados encontrados: {retiredUsers.Count}");


                return ReportPdfGenerator.GenerateRetiredUsersPdf(retiredUsers, usuario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR ReportService.GenerateRetiredUsersReport: " + ex.ToString());
                return new byte[0];
            }
        }
        // ============================================================
        // REPORTE: USUARIOS ACTIVOS (con login en 1 mes)
        // ============================================================
        public byte[] GenerateActiveUsersReport(string usuario)
        {
            try
            {
                var repo = new ReportRepository();
                var cutoff = DateTime.Now.AddMonths(-1); // usuarios con login en el último mes
                var activeUsers = repo.GetActiveUsers(cutoff);

                if (activeUsers == null)
                    activeUsers = new List<UserCombinedDtos>();

                // ✅ Generar PDF con el mismo formato
                return ReportPdfGenerator.GenerateActiveUsersPdf(activeUsers, usuario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR ReportService.GenerateActiveUsersReport: " + ex.ToString());
                return new byte[0];
            }
        }


    }
}