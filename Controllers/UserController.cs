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
        public IActionResult Register(UserRegistration newUser)
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
              PasswordHasher<UserRegistration> Hasher = new PasswordHasher<UserRegistration>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
              User NewUser = new User
                {
                  FirstName = newUser.FirstName,
                  LastName = newUser.LastName,
                  Email = newUser.Email,
                  Password = Hasher.HashPassword(newUser, newUser.Password),
                };
              dbContext.Users.Add(NewUser);
              dbContext.SaveChanges();

              int uid = NewUser.UserId;
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
        public IActionResult Login(UserLogin currentUser)
        {
          var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == currentUser.LoginEmail);
          if(userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                return View("LoginView");
            }
            // Initialize hasher object
            var hasher = new PasswordHasher<UserLogin>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(currentUser, userInDb.Password, currentUser.LoginPassword);

            // result can be compared to 0 for failure
            if(result == 0)
            {
                // handle failure (this should be similar to how "existing email" is handled)
                ModelState.AddModelError("LoginPassword", "Invalid Email/Password");
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