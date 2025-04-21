using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using SelcukDemo.Models;
using System.Security.Claims;

namespace SelcukDemo.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireCompleteProfileAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Kullanıcının ID'sini al
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                await next(); // Giriş yapılmamış
                return;
            }

            // UserManager'ı al
            var userManager = context.HttpContext.RequestServices.GetService(typeof(UserManager<AppUser>)) as UserManager<AppUser>;
            var user = await userManager.FindByIdAsync(userId);

            // Eğer kullanıcı bilgilerinden biri eksikse ve şu anda Settings sayfasında değilse => yönlendir
            var currentPath = context.HttpContext.Request.Path;
            bool isOnSettingsPage = currentPath.HasValue && currentPath.Value.Contains("/Account/Settings");

            if (user != null && !isOnSettingsPage &&
                (user.BirthDate == null || string.IsNullOrEmpty(user.Gender) || string.IsNullOrEmpty(user.City)))
            {
                context.Result = new RedirectToActionResult("Settings", "Account", null);
                return;
            }

            await next();
        }
    }
}