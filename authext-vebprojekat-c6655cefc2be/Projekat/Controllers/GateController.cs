using Projekat.Dao;
using Projekat.Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class GateController : Controller
    {
        [Route("Login")]
        public ActionResult Login()
        {
            if (Request.HttpMethod == "GET")
                return View();

            var username = Request.Params["username"];
            var password = Request.Params["password"];
            if (Dao.CanLogIn(username, password))
            {
                Response.Cookies.Add(new HttpCookie(CookieKeys.Login)
                {
                    Value = username,
                    Expires = DateTime.Now.AddHours(1)
                });
                ViewBag.Title = "Login Successful";
            }
            else
            {
                ViewBag.Title = "Login Failed";
            }

            return View("LoginResult");
        }

        [Route("Register")]
        public ActionResult Register()
        {
            if (Request.Cookies[CookieKeys.Login] != null)
                return View("AlreadyLoggedIn");

            if (Request.HttpMethod == "GET")
                return View();

            var user = new User()
            {
                Name = Request.Params["username"],
                Password = Request.Params["password"],
                FirstName = Request.Params["first-name"],
                LastName = Request.Params["last-name"],
                PhoneNo = Request.Params["phone-no"],
                Email = Request.Params["email"],
                Role = Models.User.ForumRole.Normal,
                RegisteredOn = DateTime.Now
            };

            if (Dao.Register(user))
                ViewBag.Title = "Registration Successful";
            else
                ViewBag.Title = "Registration Failure";

            return View("RegisterResult");
        }

        [Route("Logout")]
        public ActionResult Logout()
        {
            Response.Cookies.Add(new HttpCookie(CookieKeys.Login)
            {
                Value = null,
                Expires = DateTime.Now.AddDays(-1)
            });

            return View();
        }


        private IDao Dao => (IDao) HttpContext.Application[ApplicationKeys.Dao];
    }
}