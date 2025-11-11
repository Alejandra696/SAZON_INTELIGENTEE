using INTELIGENTE_SAZÓN.Dtos;
using INTELIGENTE_SAZÓN.Repositories.Models;
using INTELIGENTE_SAZÓN.Services;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INTELIGENTE_SAZÓN.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController()
        {
            _userService = new UserService();
        }

        // ============================================================
        // REGISTRO DE USUARIOS
        // ============================================================

        [HttpGet]
        public ActionResult RegisterUser()
        {
            return View(new UserDtos());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserDtos model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool registrado = _userService.Register(model);

                    if (registrado)
                    {
                        TempData["SuccessMessage"] = "✅ Registro realizado correctamente.";
                        return RedirectToAction("RegisterUser");
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "❌ Error al registrar el usuario. Intenta nuevamente.");
            }
            return View("RegisterUser", model);
        }

        public ActionResult SucessUser()
        {
            return View();
        }

        // ============================================================  
        // LOGIN DE USUARIOS  
        // ============================================================  

        [HttpGet]
        public ActionResult LoginUser()
        {
            return View(new LoginDtos());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDtos model)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG Login - Usuario: {model.email_User}, Contraseña: {model.passw_User}");

            if (ModelState.IsValid)
            {
                bool loginOk = _userService.Login(model.email_User, model.passw_User);

                // 🔹 Si no encontró el usuario en la tabla USER, intentamos en PROFESSIONAL_PROFILE
                if (!loginOk)
                {
                    using (var db = new Sazon_inteligenteDBEntities1())
                    {
                        var prof = db.PROFESSIONAL_PROFILEs
                            .FirstOrDefault(p => p.email_User_Profile == model.email_User);

                        if (prof != null)
                        {
                            // Verificamos con BCrypt
                            bool passOk = BCrypt.Net.BCrypt.Verify(model.passw_User, prof.passw_User_Profile);
                            if (passOk)
                            {
                                loginOk = true;

                                // Crear objeto temporal para manejar sesión con los mismos campos
                                Session["UserEmail"] = prof.email_User_Profile;
                                Session["UserRoleId"] = prof.ID_Role;

                                // Actualizamos último login en PROFESSIONAL_PROFILE
                                prof.Last_Login_Profile = DateTime.Now;
                                db.SaveChanges();

                                System.Diagnostics.Debug.WriteLine($"✅ Login exitoso (profesional): {prof.email_User_Profile}");
                            }
                        }
                    }
                }

                if (loginOk)
                {
                    // Solo si fue login normal (no profesional) necesitamos buscar usuario
                    var user = _userService.GetUserByEmail(model.email_User);

                    if (user != null)
                    {
                        Session["UserEmail"] = user.email_User;
                        Session["UserRoleId"] = user.ID_Role;

                        // ✅ Actualizar último login en tabla USER
                        try
                        {
                            using (var db = new Sazon_inteligenteDBEntities1())
                            {
                                var usuarioDb = db.USERs.FirstOrDefault(u => u.email_User == user.email_User);
                                if (usuarioDb != null)
                                {
                                    usuarioDb.last_Login_User = DateTime.Now;
                                    db.SaveChanges();
                                    System.Diagnostics.Debug.WriteLine($"✅ Último login actualizado en USER: {usuarioDb.email_User}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ Error actualizando la fecha de último login: " + ex.Message);
                        }

                        // 🔁 Redirección por roles
                        if (user.ID_Role == 4)
                            return RedirectToAction("AdmiPrincipalUser", "User");
                        else if (user.ID_Role == 3)
                            return RedirectToAction("EngineerPrincipalUser", "User");
                        else if (user.ID_Role == 2)
                            return RedirectToAction("ChefPrincipalUser", "User");
                        else
                            return RedirectToAction("ClientPrincipalUser", "User");
                    }
                    else
                    {
                        // 🔁 Redirección para perfil profesional
                        int rol = Convert.ToInt32(Session["UserRoleId"]);
                        if (rol == 3)
                            return RedirectToAction("EngineerPrincipalUser", "User");
                        else if (rol == 2)
                            return RedirectToAction("ChefPrincipalUser", "User");
                        else
                            return RedirectToAction("CLientPrincipalUser", "User");
                    }
                }

                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }

            return View("LoginUser", model);
        }

        // ============================================================
        // VISTA PRINCIPAL DEL ADMINISTRADOR
        // ============================================================
        [HttpGet]
        public ActionResult AdmiPrincipalUser()
        {
            try
            {
                if (Session["UserEmail"] == null)
                {
                    TempData["ErrorMessage"] = "Debes iniciar sesión para acceder a esta sección.";
                    return RedirectToAction("LoginUser", "User");
                }

                var userRoleId = Session["UserRoleId"] != null ? Convert.ToInt32(Session["UserRoleId"]) : 0;

                if (userRoleId != 4)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la vista del administrador: " + ex.Message;
                return RedirectToAction("LoginUser", "User");
            }
        }

        // ============================================================
        // VISTA PRINCIPAL DEL INGENIERO DE ALIMENTOS
        // ============================================================
        [HttpGet]
        public ActionResult EngineerPrincipalUser()
        {
            try
            {
                if (Session["UserEmail"] == null)
                {
                    TempData["ErrorMessage"] = "Debes iniciar sesión para acceder a esta sección.";
                    return RedirectToAction("LoginUser", "User");
                }

                var userRoleId = Session["UserRoleId"] != null ? Convert.ToInt32(Session["UserRoleId"]) : 0;

                if (userRoleId != 3)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                    return RedirectToAction("LoginUser", "User");
                }

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la vista del Ingeniero: " + ex.Message;
                return RedirectToAction("LoginUser", "User");
            }
        }

        // ============================================================
        // 👨‍🍳 VISTA PRINCIPAL DEL CHEF
        // ============================================================
        [HttpGet]
        public ActionResult ChefPrincipalUser()
        {
            try
            {
                if (Session["UserEmail"] == null)
                {
                    TempData["ErrorMessage"] = "Debes iniciar sesión para acceder a esta sección.";
                    return RedirectToAction("LoginUser", "User");
                }

                var userRoleId = Session["UserRoleId"] != null ? Convert.ToInt32(Session["UserRoleId"]) : 0;

                // Solo permite acceso al rol del chef (ID_Role = 2 o el que definas)
                if (userRoleId != 2)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                    return RedirectToAction("LoginUser", "User");
                }

                return View(); // Carga chefPrincipalUser.cshtml
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la vista del Chef: " + ex.Message;
                return RedirectToAction("LoginUser", "User");
            }
        }
        // ============================================================
        // 🌟 VISTA PRINCIPAL DEL USUARIO GENERAL
        // ============================================================
        [HttpGet]
        public ActionResult ClientPrincipalUser()
        {
            // ⚙️ Verificamos que el usuario esté logueado antes de mostrar la vista
            if (Session["UserEmail"] == null)
                return RedirectToAction("LoginUser", "User");

            // 📧 Obtenemos el correo del usuario actual desde la sesión
            string email = Session["UserEmail"].ToString();

            // 🧠 Si necesitas traer datos del usuario desde la base de datos
            // puedes hacerlo aquí (usa tu UserService o ClientService)
            var user = _userService.GetUserByEmail(email);
            if (user == null)
            {
                // Si no se encuentra el usuario, redirige al login
                return RedirectToAction("LoginUser", "User");
            }

            // ✅ Carga la vista principal del usuario
            // (asegúrate de que esté en Views/User/ClientPrincipalUser.cshtml)
            return View("~/Views/User/ClientPrincipalUser.cshtml");
        }
    }
}