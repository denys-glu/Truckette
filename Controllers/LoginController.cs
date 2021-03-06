using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Truckette.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Truckette.Controllers
{
    public class LoginController : Controller
    {
                private MyContext dbContext;

        public LoginController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("registration")]
        public ViewResult Registration()
        {
            return View ("Registration");
        }

        [HttpPost("register")]
        public IActionResult RegisterUser(User user)
        {
            if (ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "This Email is already being used");
                    return View("registration");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);

                dbContext.Add(user);
                dbContext.SaveChanges();
                User OurUser = dbContext.Users
                        .FirstOrDefault(u => u.Email == user.Email); 
                    HttpContext.Session.SetInt32("UserId", OurUser.UserId);
                return RedirectToAction("Index", "Home");
            }
            return View("Registration");
        }
        
        [HttpGet("SetSession/{pagefrom}")]
        public ViewResult SetSession(string pagefrom)
        {
            HttpContext.Session.SetString("LastPage", pagefrom);
            return View("Login");
        }

        [HttpGet("Login")]
        public ViewResult Login()
        {
            return View();
        }

        [HttpPost("LoginUser")]
        public IActionResult LoginUser(LoginUser submission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == submission.LoginEmail);

                if(userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "This email is not registered");
                    return View("Login");
                }

                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(submission, userInDb.Password, submission.LoginPassword);

                if(result == 0)
                {
                    ModelState.AddModelError("LoginPassword", "Invalid Password");
                    return View("Login");
                }
                User OurUser = dbContext.Users
                    .FirstOrDefault(u => u.Email == submission.LoginEmail); 
                HttpContext.Session.SetInt32("UserId", OurUser.UserId);
                
                if(submission.LoginEmail == "admin@email.com")
                {
                    HttpContext.Session.SetInt32("Admin", 1411);
                    return RedirectToAction("AdminDash", "Admin");
                }

                string page = HttpContext.Session.GetString("LastPage");
                return RedirectToAction(page, "Home");
            }
            else
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == submission.LoginEmail);
            }
            return View("Login");
        }
        
        //Log Off
        [HttpGet("logoff/{page}")]
        public IActionResult LogOff(string page)
        {
            HttpContext.Session.Clear();
            return RedirectToAction(page, "Home");
        }
    }
}