using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentroCapacitacionEmergencias.Models;

namespace CentroCapacitacionEmergencias.Filters
{
    public class SessionAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.Session["User"] as SessionUser;
            if (user == null)
                return false;

            if (_roles == null || _roles.Length == 0)
                return true;

            return _roles.Contains(user.Rol, StringComparer.OrdinalIgnoreCase);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Account/Login");
        }
    }
}