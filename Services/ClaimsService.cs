using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SelcukDemo.AppDbContext;

namespace SelcukDemo.Services;

/// <summary>
/// Kullanıcı yetkilendirme işlemleri için servis.
/// Kullanıcının yetkili olduğu menüleri claim olarak ekler ve günceller.
/// </summary>
public class ClaimsService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SelcukDbContext _context;
    private readonly ILogger<ClaimsService> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;

    /// <summary>
    /// ClaimsService constructor'ı. Kullanıcı yönetimi ve veritabanı işlemleri için bağımlılıkları enjekte eder.
    /// </summary>
    public ClaimsService(
        UserManager<IdentityUser> userManager,
        SelcukDbContext context,
        ILogger<ClaimsService> logger,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
        _signInManager = signInManager;
    }

    public ClaimsService(UserManager<IdentityUser> userManager, object contextFactory, object logger)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Kullanıcının yetkili olduğu menüleri claim olarak getirir.
    /// </summary>
    /// <param name="user">Kimliği alınacak kullanıcı.</param>
    /// <returns>Menü claim'leri içeren bir liste.</returns>
    public async Task<List<Claim>> GetUserClaims(IdentityUser user)
    {
        if (user == null)
        {
            _logger.LogError("GetUserClaims çağrıldı ancak kullanıcı nesnesi null.");
            return new List<Claim>();
        }

        var claims = new List<Claim>();

        // Kullanıcının rollerini getir
        var userRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("Kullanıcının rolleri: {Roles}", string.Join(", ", userRoles));

   
        if (!userRoles.Any()) return claims;

        // Kullanıcının rollerine bağlı olan menü ID'lerini RoleMenus tablosundan al
        var roleMenuIds = await _context.RoleMenus
            .Where(rm => userRoles.Contains(rm.RoleName))
            .Select(rm => rm.MenuId)
            .ToListAsync();

        _logger.LogInformation("RoleMenus tablosunda {Count} adet erişilebilir menü bulundu.", roleMenuIds.Count);
        if (!roleMenuIds.Any()) return claims;

        // Menü ID'lerine karşılık gelen menü bilgilerini UserMenus tablosundan al
        var userMenus = await _context.UserMenus
            .Where(m => roleMenuIds.Contains(m.Id))
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("UserMenus tablosunda {Count} adet yetkili menü bulundu.", userMenus.Count);

        // Her menüyü bir claim olarak ekle
        foreach (var menu in userMenus)
        {
            var claimValue = $"{menu.ControllerName}/{menu.ActionName}/{menu.Name}";
            _logger.LogInformation("Eklenen Menü Claim: {Claim}", claimValue);
            claims.Add(new Claim("Menu", claimValue));
        }

        return claims;
    }

    /// <summary>
    /// Kullanıcının yetkilerini sıfırlar ve günceller.
    /// </summary>
    /// <param name="user">Yetkileri güncellenecek kullanıcı.</param>
    public async Task UpdateUserClaims(IdentityUser user)
    {
        if (user == null)
        {
            _logger.LogError("UpdateUserClaims çağrıldı ancak kullanıcı nesnesi null.");
            return;
        }

        // Kullanıcının mevcut claim'lerini al
        var existingClaims = await _userManager.GetClaimsAsync(user);

        // Önce eski menü claim'lerini temizle
        var menuClaims = existingClaims.Where(c => c.Type == "Menu").ToList();
        foreach (var claim in menuClaims)
        {
            await _userManager.RemoveClaimAsync(user, claim);
        }

        // Kullanıcının rollerini getir
        var userRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("Güncellenmiş Kullanıcı Rolleri: {Roles}", string.Join(", ", userRoles));

        if (!userRoles.Any())
        {
            _logger.LogWarning("Kullanıcının hiçbir rolü bulunmuyor, claim eklenmeyecek.");
            return;
        }

        // Kullanıcının rollerine bağlı olan menü ID'lerini RoleMenus tablosundan al
        var roleMenuIds = await _context.RoleMenus
            .Where(rm => userRoles.Contains(rm.RoleName))
            .Select(rm => rm.MenuId)
            .ToListAsync();

        _logger.LogInformation("RoleMenus tablosunda {Count} adet yetkili menü bulundu.", roleMenuIds.Count);
        if (!roleMenuIds.Any())
        {
            _logger.LogWarning("Kullanıcı için yetkilendirilmiş menü bulunamadı.");
            return;
        }

        // Menü ID'lerine karşılık gelen menü bilgilerini UserMenus tablosundan al
        var updatedMenus = await _context.UserMenus
            .Where(m => roleMenuIds.Contains(m.Id))
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("UserMenus tablosunda {Count} adet menü bulundu.", updatedMenus.Count);
        if (!updatedMenus.Any())
        {
            _logger.LogWarning("RoleMenus tablosunda menü ID'leri var ama UserMenus içinde eşleşen menü bulunamadı!");
            return;
        }

        // Yeni menüleri claim olarak ekle
        foreach (var menu in updatedMenus)
        {
            var claimValue = $"{menu.ControllerName}/{menu.ActionName}/{menu.Name}";
            _logger.LogInformation("Yeni Eklenen Menü Claim: {Claim}", claimValue);
            await _userManager.AddClaimAsync(user, new Claim("Menu", claimValue));
        }

        // 📌 Kullanıcının oturumunu yenileyerek claim'leri güncelle
        await _signInManager.RefreshSignInAsync(user);
    }
}
