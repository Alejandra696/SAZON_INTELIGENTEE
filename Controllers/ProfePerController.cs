using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories.Models;
using INTELIGENTE_SAZÓN.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;

namespace INTELIGENTE_SAZÓN.Controllers
{
    public class ProfePerController : Controller
    {
        private readonly ProfePerService _profePerService = new ProfePerService();
        private readonly ReportService _reportService = new ReportService();

        // ============================================================
        // VISTA: CREACIÓN DE PERFILES PROFESIONALES (GET)
        // ============================================================
        [HttpGet]
        public ActionResult AdmiProfePerUser()
        {
            if (Session["UserRoleId"] == null || Session["UserRoleId"].ToString() != "4")
                return RedirectToAction("LoginUser");

            return View(new ProfessionalProfileDtos());
        }

        // ============================================================
        // CREAR PERFIL PROFESIONAL (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProfessionalProfile(ProfessionalProfileDtos model)
        {
            if (Session["UserRoleId"] == null || Session["UserRoleId"].ToString() != "4")
                return RedirectToAction("LoginUser");

            HttpPostedFileBase file = null;
            if (Request.Files.Count > 0)
                file = Request.Files[0];

            bool created = _profePerService.InsertProfessionalProfile(model, file);

            if (created)
            {
                TempData["SuccessMessage"] = "✅ Perfil profesional creado correctamente.";
                return RedirectToAction("AdmiProfePerUser");
            }

            ModelState.AddModelError("", "❌ Error al crear el perfil. Revisa los datos e intenta nuevamente.");
            return View("AdmiProfePerUser", model);
        }

        // ============================================================
        // VISTA: ACTIVAR / DESACTIVAR PERFILES PROFESIONALES (GET)
        // ============================================================
        [HttpGet]
        public ActionResult AdmiActiveUser()
        {
            if (Session["UserRoleId"] == null || Session["UserRoleId"].ToString() != "4")
                return RedirectToAction("LoginUser", "User");

            var profiles = _profePerService.GetAllProfiles();
            return View(profiles);
        }

        // ============================================================
        // ACTUALIZAR ESTADO DE LOS PERFILES (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdmiActivePerUser(FormCollection form)
        {
            if (Session["UserRoleId"] == null || Session["UserRoleId"].ToString() != "4")
                return RedirectToAction("Login", "User");

            bool updated = _profePerService.UpdateProfileStatus(form);

            if (updated)
            {
                TempData["SuccessMessage"] = "✅ Cambios guardados correctamente.";
                return RedirectToAction("AdmiActivePerUser");
            }

            TempData["ErrorMessage"] = "❌ Error al guardar los cambios. Intenta nuevamente.";
            var profiles = _profePerService.GetAllProfiles();
            return View(profiles);
        }

        // ============================================================
        // 📊 VISTA DE REPORTES DEL ADMINISTRADOR
        // ============================================================
        [HttpGet]
        public ActionResult AdmiReportProfePer()
        {
            if (Session["UserRoleId"] == null || Session["UserRoleId"].ToString() != "4")
                return RedirectToAction("LoginUser", "User");

            var model = new DashboardReportDtos();

            try
            {
                using (var db = new Sazon_inteligenteDBEntities1())
                {
                    DateTime haceUnMes = DateTime.Now.AddMonths(-1);

                    // 🔹 Total de usuarios registrados (USER + PROFESSIONAL_PROFILE)
                    int totalUsers = db.USERs.Count() + db.PROFESSIONAL_PROFILEs.Count();

                    // 🔹 Usuarios nuevos (registrados en el último mes)
                    int newUsers =
                        db.USERs.Count(u => u.date_Regis_User >= haceUnMes) +
                        db.PROFESSIONAL_PROFILEs.Count(p => p.date_Regis_Profile >= haceUnMes);

                    // 🔹 Usuarios activos (último login en el último mes)
                    int activeUsers =
                        db.USERs.Count(u => u.last_Login_User >= haceUnMes) +
                        db.PROFESSIONAL_PROFILEs.Count(p => p.Last_Login_Profile >= haceUnMes);

                    // ✅ Asignar al modelo (coincidiendo con tus nombres en inglés)
                    model.TotalUsers = totalUsers;
                    model.NewUsers = newUsers;
                    model.ActiveUsers = activeUsers;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Error al cargar los contadores: " + ex.Message);
            }

            return View(model);
        }

        // ============================================================
        // DESCARGAR PDF (Selector general por tipo)
        // ============================================================
        [HttpGet]
        public FileResult DownloadReport(string type)
        {
            string usuario = System.Web.HttpContext.Current.User?.Identity?.Name ?? "Administrador";

            // ✅ Reporte de usuarios nuevos
            if (type == "usuariosnuevos")
            {
                var pdfBytes = _reportService.GenerateNewUsersPdf(usuario);
                return File(pdfBytes, "application/pdf", "UsuariosNuevos.pdf");
            }

            // ✅ Reporte de usuarios registrados
            else if (type == "usuariosregistrados")
            {
                var pdfBytes = _reportService.GenerateAllRegisteredUsersReport(usuario);
                return File(pdfBytes, "application/pdf", "UsuariosRegistrados.pdf");
            }

            // ✅ Reporte de usuarios retirados
            else if (type == "retirados")
            {
                var pdfBytes = _reportService.GenerateRetiredUsersReport(usuario);
                return File(pdfBytes, "application/pdf", "UsuariosRetirados.pdf");
            }

            // ✅ Reporte de usuarios activos
            else if (type == "activos")
            {
                var pdfBytes = _reportService.GenerateActiveUsersReport(usuario);
                return File(pdfBytes, "application/pdf", "UsuariosActivos.pdf");
            }

            // Si el parámetro no coincide, devolvemos PDF vacío
            return File(new byte[0], "application/pdf", "ReporteVacio.pdf");
        }

        // ============================================================
        // DESCARGAR PDF DE USUARIOS NUEVOS (directo)
        // ============================================================
        [HttpGet]
        public ActionResult DownloadNewUsersReport()
        {
            var reportService = new ReportService();
            var bytes = reportService.GenerateNewUsersPdf();

            if (bytes == null || bytes.Length == 0)
                return new HttpStatusCodeResult(500, "Error generando el PDF");

            return File(bytes, "application/pdf", "UsuariosNuevos.pdf");
        }

        // ============================================================
        // DESCARGAR PDF DE USUARIOS REGISTRADOS (directo)
        // ============================================================
        [HttpGet]
        public ActionResult DownloadRegisteredUsersReport()
        {
            var reportService = new ReportService();
            var bytes = reportService.GenerateAllRegisteredUsersReport("Administrador");

            if (bytes == null || bytes.Length == 0)
                return new HttpStatusCodeResult(500, "Error generando el PDF");

            return File(bytes, "application/pdf", "UsuariosRegistrados.pdf");
        }
        // ============================================================
        // DESCARGAR PDF DE USUARIOS RETIRADOS (directo)
        // ============================================================
        [HttpGet]
        public ActionResult DownloadRetiredUsersReport()
        {
            var reportService = new ReportService();
            var bytes = reportService.GenerateRetiredUsersReport("Administrador");

            if (bytes == null || bytes.Length == 0)
                return new HttpStatusCodeResult(500, "Error generando el PDF");

            return File(bytes, "application/pdf", "UsuariosRetirados.pdf");
        }
        // ============================================================
        // DESCARGAR PDF DE USUARIOS ACTIVOS (directo)
        // ============================================================
        [HttpGet]
        public ActionResult DownloadActiveUsersReport()
        {
            var reportService = new ReportService();
            var bytes = reportService.GenerateActiveUsersReport("Administrador");

            if (bytes == null || bytes.Length == 0)
                return new HttpStatusCodeResult(500, "Error generando el PDF");

            return File(bytes, "application/pdf", "UsuariosActivos.pdf");
        }
    }
}
