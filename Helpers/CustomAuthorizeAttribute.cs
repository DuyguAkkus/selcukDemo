using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

public class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public CustomAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new RedirectToActionResult("SignIn", "Account", null);
            return;
        }

        // Admin her sayfaya erişebilir
        if (user.IsInRole("Admin"))
        {
            return;
        }

        // Diğer roller için kontrol
        if (_roles.Any(role => user.IsInRole(role)))
        {
            return;
        }

        context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }
}