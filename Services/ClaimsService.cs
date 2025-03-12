using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SelcukDemo.AppDbContext;

namespace SelcukDemo.Services;

public class ClaimsService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext.SelcukDbContext _context;
    private readonly ILogger<ClaimsService> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;

    // ✅ **Constructor**
    public ClaimsService(
        UserManager<IdentityUser> userManager,
        AppDbContext.SelcukDbContext context,
        ILogger<ClaimsService> logger,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
        _signInManager = signInManager;
    }

    // **📌 Kullanıcının yetkilendirildiği menüleri getirir**
    public async Task<List<Claim>> GetUserClaims(IdentityUser user)
    {
        var claims = new List<Claim>();

        // Kullanıcının rollerini al
        var userRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("Kullanıcının Rolleri: {Roles}", string.Join(", ", userRoles));

        if (!userRoles.Any()) return claims;

        // RoleMenus'dan menü ID'lerini al
        var roleMenuIds = await _context.RoleMenus
            .Where(rm => userRoles.Contains(rm.RoleName))
            .Select(rm => rm.MenuId)
            .ToListAsync();

        _logger.LogInformation("Erişilebilir RoleMenus: {Count} adet bulundu.", roleMenuIds.Count);

        if (!roleMenuIds.Any()) return claims;

        // UserMenus'dan verileri al
        var userMenus = await _context.UserMenus
            .Where(m => roleMenuIds.Contains(m.Id))
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("Erişilebilir UserMenus: {Count} adet bulundu.", userMenus.Count);

        // Menüleri claim olarak ekleyelim
        foreach (var menu in userMenus)
        {
            var claimValue = $"{menu.ControllerName}/{menu.ActionName}/{menu.Name}";
            _logger.LogInformation("Eklenen Menü Claim: {Claim}", claimValue);
            claims.Add(new Claim("Menu", claimValue));
        }

        return claims;
    }

    // **📌 Kullanıcının yetkilerini güncelleyerek claim'leri sıfırlar ve yeniden ekler.**
    public async Task UpdateUserClaims(IdentityUser user)
    {
        if (user == null) return;

        // Kullanıcının mevcut claim'lerini al
        var existingClaims = await _userManager.GetClaimsAsync(user);

        // Eski menü claim'lerini temizle
        var menuClaims = existingClaims.Where(c => c.Type == "Menu").ToList();
        foreach (var claim in menuClaims)
        {
            await _userManager.RemoveClaimAsync(user, claim);
        }

        // Kullanıcının rollerini al
        var userRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("Kullanıcının Güncellenmiş Rolleri: {Roles}", string.Join(", ", userRoles));

        if (!userRoles.Any()) return;

        // RoleMenus'dan menü ID'lerini al
        var roleMenuIds = await _context.RoleMenus
            .Where(rm => userRoles.Contains(rm.RoleName))
            .Select(rm => rm.MenuId)
            .ToListAsync();

        _logger.LogInformation("Güncellenmiş RoleMenus: {Count} adet bulundu.", roleMenuIds.Count);

        if (!roleMenuIds.Any()) return;

        // Güncellenmiş menüleri UserMenus tablosundan çek
        var updatedMenus = await _context.UserMenus
            .Where(m => roleMenuIds.Contains(m.Id))
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("Güncellenmiş UserMenus: {Count} adet bulundu.", updatedMenus.Count);

        // Yeni menüleri claim olarak ekleyelim
        foreach (var menu in updatedMenus)
        {
            var claimValue = $"{menu.ControllerName}/{menu.ActionName}/{menu.Name}";
            _logger.LogInformation("Yeni Eklenen Menü Claim: {Claim}", claimValue);
            await _userManager.AddClaimAsync(user, new Claim("Menu", claimValue));
        }

        // **📌 Kullanıcıyı yeniden oturuma alarak claim'leri güncelle**
        await _signInManager.RefreshSignInAsync(user);
    }
}
