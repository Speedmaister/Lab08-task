using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab08.Task.Controllers
{
    public class VehicleRegistrationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
