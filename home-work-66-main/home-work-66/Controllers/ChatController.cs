using home_work_66.Models;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly HeadHunterContext _db;

    public ChatController(UserManager<User> userManager, HeadHunterContext db)
    {
        _userManager = userManager;
        _db = db;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChatCreateViewModel model)
    {
        int userId = int.Parse(_userManager.GetUserId(User)!);
        if (ModelState.IsValid)
        {
            var existingChat = await _db.Chats.FirstOrDefaultAsync(c =>
                c.UserId == userId && c.VacancyId == model.VacancyId);
            if (existingChat != null)
            {
                return RedirectToAction("Index", new { id = existingChat.Id, companyId = existingChat.CompanyId });
            }
            Chat chat = new Chat()
            {
                UserId = userId,
                CompanyId = model.CompanyId,
                VacancyId = model.VacancyId,
                ResumeId = model.ResumeId.Value,
            };
            await _db.Chats.AddAsync(chat);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = chat.Id, companyId = chat.CompanyId });
        }
        return NotFound();
    }

    public async Task<IActionResult> Index(int id, int companyId)
    {
        var chat = await _db.Chats
            .Include(x => x.Vacancy)
            .Include(x => x.Resume)
            .Include(x => x.User)
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (chat == null)
        {
            return NotFound();
        }

        var messages = await _db.Messages
            .Where(x => x.ChatId == id)
            .Include(x => x.User)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        ViewBag.UserId = int.Parse(_userManager.GetUserId(User)!);
        ViewBag.CompanyId = companyId;
        ViewBag.ChatId = id;
        return View(new ChatPageViewModel
        {
            Chat = chat,
            Messages = messages
        });
    }
    
    public async Task<IActionResult> UpdateListAsync(int? id)
    {
        var messages = await _db.Messages
            .Where(x => x.ChatId == id)
            .Include(x => x.User)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        ViewBag.UserId = int.Parse(_userManager.GetUserId(User)!);
        return PartialView("_ChatPartialView", messages);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessageAsync(MessageCreateViewModel model)
    {
        int userId = int.Parse(_userManager.GetUserId(User)!);
        if (ModelState.IsValid)
        {
            Message message = new Message()
            {
                Content = model.Content,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ChatId = model.ChatId
            };
            var result = await _db.Messages.AddAsync(message);
            if (result.State == EntityState.Added)
                await _db.SaveChangesAsync();

            var messages = await _db.Messages
                .Where(x => x.ChatId == model.ChatId)
                .Include(x => x.User)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            ViewBag.UserId = userId;
            return PartialView("_ChatPartialView", messages);
        }
        return Json(new { success = false });
    }
    
    [HttpPost]
    [Authorize(Roles = "company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromResume(int resumeId, int vacancyId)
    {
        int companyId = int.Parse(_userManager.GetUserId(User)!);

        Resume? resume = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.IsPublished);

        if (resume == null)
        {
            return NotFound();
        }

        Vacancy? vacancy = await _db.Vacancies
            .FirstOrDefaultAsync(v => v.Id == vacancyId && v.CompanyId == companyId);

        if (vacancy == null)
        {
            return NotFound();
        }

        Chat? existingChat = await _db.Chats
            .FirstOrDefaultAsync(c =>
                c.UserId == resume.UserId &&
                c.CompanyId == companyId &&
                c.VacancyId == vacancyId);

        if (existingChat != null)
        {
            return RedirectToAction("Index", new
            {
                id = existingChat.Id,
                companyId = existingChat.CompanyId
            });
        }

        Chat chat = new Chat
        {
            UserId = resume.UserId,
            CompanyId = companyId,
            ResumeId = resume.Id,
            VacancyId = vacancy.Id
        };

        await _db.Chats.AddAsync(chat);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index", new
        {
            id = chat.Id,
            companyId = chat.CompanyId
        });
    }
}