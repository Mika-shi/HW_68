using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

public class MainController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}