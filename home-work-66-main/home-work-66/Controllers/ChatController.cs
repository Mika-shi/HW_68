using System.Security.Claims;
using home_work_66.Models;
using home_work_66.Services;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly IHeadHunterService _service;

    public ChatController(IHeadHunterService service)
    {
        _service = service;
    }

    private int? GetCurrentUserId()
    {
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserIdString, out int userId) ? userId : null;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChatCreateViewModel model)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return NotFound();
        }

        Chat chat = await _service.CreateChatAsync(model, userId.Value);

        return RedirectToAction("Index", new
        {
            id = chat.Id,
            companyId = chat.CompanyId
        });
    }

    public async Task<IActionResult> Index(int id, int companyId)
    {
        ChatPageViewModel? model = await _service.GetChatPageAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        ViewBag.UserId = userId.Value;
        ViewBag.CompanyId = companyId;
        ViewBag.ChatId = id;

        return View(model);
    }

    public async Task<IActionResult> UpdateListAsync(int? id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        List<Message> messages = await _service.GetMessagesAsync(id);

        ViewBag.UserId = userId.Value;

        return PartialView("_ChatPartialView", messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessageAsync(MessageCreateViewModel model)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return Json(new { success = false });
        }

        List<Message> messages = await _service.SendMessageAsync(model, userId.Value);

        ViewBag.UserId = userId.Value;

        return PartialView("_ChatPartialView", messages);
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromResume(int resumeId, int vacancyId)
    {
        int? companyId = GetCurrentUserId();

        if (companyId == null)
        {
            return Unauthorized();
        }

        Chat? chat = await _service.CreateChatFromResumeAsync(resumeId, vacancyId, companyId.Value);

        if (chat == null)
        {
            return NotFound();
        }

        return RedirectToAction("Index", new
        {
            id = chat.Id,
            companyId = chat.CompanyId
        });
    }
}