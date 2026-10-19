using home_work_66.Models;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private readonly HeadHunterContext _db;
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment environment,  HeadHunterContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _environment = environment;
        _db = db;
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
    {
        ValidationController validation = new ValidationController(_db);
        if (!string.IsNullOrWhiteSpace(model.Email) && !await validation.CheckEmail(model.Email, null))
            ModelState.AddModelError(nameof(model.Email), "В базе уже существует аккаунт с данным адресом почты");
        if (!string.IsNullOrWhiteSpace(model.Password) && !validation.CheckPassword(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Пароль должен содержать в себе 1 букву верхнего регистра, 1 букву нижнего регистра, а также минимум 1 цифру");
        string avatarPath = "/default.jpg";
        if (ModelState.IsValid)
        {
            if (model.Avatar != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Avatar.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(fileStream);
                }
                avatarPath = $"/uploads/avatars/{uniqueFileName}";   
            }
            User userToCreate;
            string roleToAssign;
            if (model.IsCompany)
            {
                userToCreate = new User
                {
                    Email = model.Email,
                    UserName = model.Email, 
                    CompanyName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    AvatarUrl = avatarPath
                };
                roleToAssign = "company";
            }
            else
            {
                userToCreate = new User
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    AvatarUrl = avatarPath
                };
                roleToAssign = "user";
            }
            var result = await _userManager.CreateAsync(userToCreate, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(userToCreate, roleToAssign);
                await _signInManager.SignInAsync(userToCreate, isPersistent: false);
                if(model.IsCompany)
                    return RedirectToAction("Index", "Company");
                else 
                    return RedirectToAction("Index", "User");
                
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
    
    public IActionResult Login(string? returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    false
                );

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    if(User.IsInRole("company"))
                        return RedirectToAction("Index", "Company");
                    else 
                        return RedirectToAction("Index", "User");
                }
            }
            ModelState.AddModelError("", "Неправильный логин и (или) пароль");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOffAsync()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
    
}