using Microsoft.AspNetCore.Mvc;
using PlantDashboard.Models;

namespace PlantDashboard.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewBag.OptimalConfig = new PlantRoomConfig();
        return View();
    }
}