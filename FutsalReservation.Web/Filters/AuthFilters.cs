using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FutsalReservation.Web.Filters;

// pastikan user sudah login sebelum membuka halaman
public class ButuhLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var token = context.HttpContext.Session.GetString(SesiKeys.Token);
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}

// khusus halaman yang hanya boleh diakses admin
public class ButuhAdminAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var sesi = context.HttpContext.Session;
        var token = sesi.GetString(SesiKeys.Token);

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (sesi.GetString(SesiKeys.Role) != "Admin")
        {
            context.Result = new RedirectToActionResult("Index", "Home", new { tolak = true });
        }
    }
}
