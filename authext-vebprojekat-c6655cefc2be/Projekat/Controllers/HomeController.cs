using Projekat.Dao;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            ViewBag.Names = Dao.GetSubforumNames();

            return View();
        }

        [Route("User/{user}")]
        public ActionResult ShowUser(string user)
        {
            ViewBag.User = Dao.GetUser(user);
            if (ViewBag.User == null)
                return HttpNotFound();

            if (user == LoggedUserName)
            {
                ViewBag.ThemeVotes = Dao.GetUserThemeVotes(user);
                ViewBag.CommentVotes = Dao.GetUserCommentVotes(user);
            }

            return View();
        }

        [Route("Subforum/{name}")]
        public ActionResult Subforum(string name)
        {
            ViewBag.Subforum = Dao.GetSubforum(name);

            if (ViewBag.Subforum == null)
                return HttpNotFound();

            return View();
        }

        [Route("Subforum/{subforum}/{title}")]
        public ActionResult Theme(string subforum, string title)
        {
            ViewBag.Subforum = subforum;
            ViewBag.Theme = Dao.GetTheme(subforum, title);

            if (ViewBag.Theme == null)
                return HttpNotFound();

            return View();
        }


        private string LoggedUserName => Request.Cookies[CookieKeys.Login]?.Value;
        private IDao Dao => (IDao)HttpContext.Application[ApplicationKeys.Dao];
    }
}