using System;
using System.Web.Mvc;
using CentroCapacitacionEmergencias.Helpers;
using CentroCapacitacionEmergencias.Models;
using CentroCapacitacionEmergencias.Repositories;

namespace CentroCapacitacionEmergencias.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountRepository _repo = new AccountRepository();

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["User"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string username = (model.Username ?? "").Trim();
            string password = (model.Password ?? "").Trim();

            var user = _repo.GetByUsername(username);

            if (user == null)
            {
                ViewBag.Error = "Credenciales inválidas.";
                return View(model);
            }

            if (!user.Activo)
            {
                ViewBag.Error = "Usuario inactivo.";
                return View(model);
            }

            if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTime.Now)
            {
                ViewBag.Error = "Usuario bloqueado temporalmente. Intente más tarde.";
                return View(model);
            }

            string hash = PasswordHelper.Hash(password);

            if (!string.Equals(hash, user.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                int maxAttempts = JsonConfigHelper.GetInt("MaxLoginAttempts");
                int lockMinutes = JsonConfigHelper.GetInt("LockMinutes");

                _repo.RegisterFailedAttempt(user.Id, maxAttempts, lockMinutes);

                ViewBag.Error = "Credenciales inválidas.";
                return View(model);
            }

            _repo.ResetFailedAttempts(user.Id);

            Session["User"] = new SessionUser
            {
                Id = user.Id,
                NombreCompleto = user.NombreCompleto,
                Username = user.Username,
                Rol = user.RolNombre
            };

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}