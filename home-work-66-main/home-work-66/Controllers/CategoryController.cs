using home_work_66.Models;
using home_work_66.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

[Authorize(Roles = "admin")]
public class CategoryController : Controller
{
    private readonly IHeadHunterService _service;

    public CategoryController(IHeadHunterService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        List<Category> categories = await _service.GetCategoriesOrderedAsync();

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

        Category? createdCategory = await _service.CreateCategoryAsync(category.Name);

        if (createdCategory == null)
        {
            return Json(new { success = false, message = "Категория с таким названием уже существует" });
        }

        return Json(new
        {
            success = true,
            id = createdCategory.Id,
            name = createdCategory.Name
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        string? error = await _service.DeleteCategoryAsync(id);

        if (error != null)
        {
            return Json(new
            {
                success = false,
                message = error
            });
        }

        return Json(new { success = true, id });
    }
}