using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;

        public HomeController(ApplicationDbContext context)
        {
            this.context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login(string ReturnUrl)
        {
            if ((HttpContext.User != null) && HttpContext.User.Identity.IsAuthenticated)
                return Redirect("Home/Index");

            if (ReturnUrl != null)
                ViewData["ReturnUrl"] = ReturnUrl;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginVM vm, string ReturnUrl)
        {
            var user = context.Users.SingleOrDefault(m => m.Email == vm.Email && SecurePasswordHasher.Verify(vm.Password, m.PasswordHash));

            if(user != null)
            {
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email)
                    };
                ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignInAsync(principal);
                if (ReturnUrl != null)
                    return Redirect(ReturnUrl);
                else
                    return Redirect("/");
            }
            else
            {
                ViewData["Error"] = "Login attempt failed.";
            }

            return View();
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        public ActionResult Register()
        {
            if ((HttpContext.User != null) && HttpContext.User.Identity.IsAuthenticated)
                return Redirect("Home/Index");
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterVM vm)
        {
            ViewData.Remove("confirm");
            if (ModelState.IsValid)
            {
                if(vm.Password != vm.ConfirmPassword)
                {
                    ViewData["confirm"] = "has-error";
                    return View("Register");
                }

                var user = new ApplicationUser()
                {
                    Email = vm.Email,
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    PasswordHash = SecurePasswordHasher.Hash(vm.Password)
                };

                context.Users.Add(user);
                context.SaveChanges();
            }
            return RedirectToAction("Login","Home");
        }
    }
}
