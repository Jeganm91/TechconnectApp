using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechConnect.Models;

namespace TechConnect.Controllers
{
    public class LotteryController : Controller
    {
        TechDBContext _context;
        public LotteryController(TechDBContext context)
        {
            _context = context;
        }

        public IActionResult UserLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UserLogin(TechLogin login)
        {
            if (ModelState.IsValid)
            {
                var r = _context.Registrants.Where
                        (l => l.Email == login.Email).FirstOrDefault();

                if (r != null)
                    return View("End", r);

                return View();
            }
            return View();
        }

        public IActionResult AddRegistrant()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddRegistrant(Registrant registrant)
        {
            if (ModelState.IsValid)
            {
                if (registrant.Email == registrant.Password)
                {
                    ViewBag.Message = "Email Id and Password should not be the same";
                }
                else
                {
                    _context.Registrants.Add(registrant);
                    _context.SaveChanges();
                    return RedirectToAction("UserLogin");
                }

                return View();
            }

            return View();
        }

        public IActionResult End()
        {
            return View();
        }
    }
}