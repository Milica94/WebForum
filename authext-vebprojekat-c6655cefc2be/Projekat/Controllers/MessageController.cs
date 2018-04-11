using Projekat.Dao;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class MessageController : Controller
    {
        [Route("InMessages")]
        public ActionResult InMessages()
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            ViewBag.Messages = Dao.GetMessagesTo(LoggedUserName);

            return View();
        }

        [Route("OutMessages")]
        public ActionResult OutMessages()
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            ViewBag.Messages = Dao.GetMessagesFrom(LoggedUserName);

            return View();
        }

        [Route("Messages/{id:int}")]
        public ActionResult Message(int id)
        {
            if (id < 0)
                return HttpNotFound();

            if (LoggedUserName == null)
                return View("NotLoggedIn");

            var msg = Dao.GetMessage((uint)id);

            if (msg == null)
                return HttpNotFound();

            if (msg.FromName != LoggedUserName && msg.ToName != LoggedUserName)
                return HttpNotFound();

            ViewBag.Message = msg;

            return View();
        }

        [Route("WriteMessage")]
        public ActionResult WriteMessage()
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (Request.HttpMethod == "GET")
            {
                return View();
            }

            var from = LoggedUserName;
            var to = Request.Params["to"];
            var content = Request.Params["content"];

            if (Dao.SendMessage(from, to, content))
                ViewBag.Title = "Sending Successful";
            else
                ViewBag.Title = "Sending Failed";

            return View("SendResult");
        }


        private string LoggedUserName => Request.Cookies[CookieKeys.Login]?.Value;
        private IDao Dao => (IDao) HttpContext.Application[ApplicationKeys.Dao];
    }
}