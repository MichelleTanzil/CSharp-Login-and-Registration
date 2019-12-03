using Microsoft.EntityFrameworkCore;
using login_and_registration.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace login_and_registration.Controllers
{
    public class UserController : Controller
    {
        private MyContext dbContext;

        // here we can "inject" our context service into the constructor
        public UserController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Register")]
        public IActionResult Register(User newUser)
        {
          if(ModelState.IsValid)
          {
            if(dbContext.Users.Any(u => u.Email == newUser.Email))
            {
              ModelState.AddModelError("Email", "Email already in use!");
              return View("Index");
            }
            else
            {
              PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
              dbContext.Users.Add(newUser);
              dbContext.SaveChanges();

              int uid = newUser.UserId;
              HttpContext.Session.SetInt32("uid", uid);

              return RedirectToAction("Success");
            }
          }
          else
          {
            return View("Index");
          }
        }

        [HttpGet("login")]
        public IActionResult LoginView()
        {
          return View();
        }

        [HttpPost("loginuser")]
        public IActionResult Login(User currentUser)
        {
          var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == currentUser.Email);
          if(userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("LoginView");
            }
            // Initialize hasher object
            var hasher = new PasswordHasher<User>();
            
            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(currentUser, userInDb.Password, currentUser.Password);
            
            // result can be compared to 0 for failure
            if(result == 0)
            {
                // handle failure (this should be similar to how "existing email" is handled)
                ModelState.AddModelError("Password", "Invalid Email/Password");
                return View("LoginView");
            }
            int uid = userInDb.UserId;
            HttpContext.Session.SetInt32("uid", uid);

            return RedirectToAction("Success");
        }
        [HttpGet("Success")]
        public IActionResult Success()
        {
          int? uid = HttpContext.Session.GetInt32("uid");
          if (uid == null)
          {
            return RedirectToAction("Index");
          }
          else
          {
            User retrivedUser = dbContext.Users.FirstOrDefault(u => u.UserId == uid);
            return View(retrivedUser);
          }
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
          HttpContext.Session.Clear();
          return RedirectToAction("Index");
        }
    }
}