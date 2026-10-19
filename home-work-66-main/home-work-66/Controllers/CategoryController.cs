using home_work_66.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

[Authorize(Roles = "admin")]
public class CategoryController : Controller
{
    private readonly HeadHunterContext _db;

    public CategoryController(HeadHunterContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return Json(new { success = false, message = "Название категории не может быть пустым" });
        }

        string trimmedName = category.Name.Trim();
        bool exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower());
        if (exists)
        {
            return Json(new { success = false, message = "Категория с таким названием уже существует" });
        }

        category.Name = trimmedName;
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Json(new { success = true, id = category.Id, name = category.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
        {
            return Json(new { success = false, message = "Категория не найдена" });
        }

        bool hasResumes = await _db.Resumes.AnyAsync(r => r.CategoryId == id);
        if (hasResumes)
        {
            return Json(new { success = false, message = "Нельзя удалить категорию, к которой привязаны резюме" });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return Json(new { success = true, id });
    }
}