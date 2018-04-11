using Projekat.Dao;
using Projekat.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class InteractionController : Controller
    {
        [Route("ChangeType")]
        public ActionResult ChangeType()
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (Dao.GetUser(LoggedUserName).Role != Models.User.ForumRole.Administrator)
                return View("NotAuthorized");

            if (Request.HttpMethod == "GET")
                return View();

            var name = Request.Params["name"];
            var type = (User.ForumRole)int.Parse(Request.Params["type"]);

            if (Dao.ChangeRole(name, type))
            {
                ViewBag.Title = "Changing Type Successful";
            }
            else
            {
                ViewBag.Title = "Changing Type Failed";
            }

            return View("ChangeTypeResult");
        }

        [Route("CreateSubforum")]
        public ActionResult CreateSubforum()
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanCreateSubforum(LoggedUserName))
                return View("NotAuthorized");

            if (Request.HttpMethod == "GET")
                return View();

            var subforum = new CreationSubforum()
            {
                Name = Request.Params["name"],
                Description = Request.Params["description"],
                IconPath = Request.Params["iconpath"],
                Rules = Request.Params["rules"]
            };

            if (Dao.CreateSubforum(LoggedUserName, subforum))
            {
                ViewBag.Title = "Creation Successful";
                ViewBag.Subforum = subforum.Name;
            }
            else
            {
                ViewBag.Title = "Creation Failed";
            }

            return View("CreateSubforumResult");
        }

        [Route("DeleteSubforum/{subforum}")]
        public ActionResult DeleteSubforum(string subforum)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanDeleteSubforum(LoggedUserName, subforum))
                return View("NotAuthorized");

            if (Dao.DeleteSubforum(subforum))
                ViewBag.Title = "Deletion Successful";
            else
                ViewBag.Title = "Deletion Failed";

            return View("DeleteSubforumResult");
        }

        [Route("CreateTheme/{subforum}")]
        public ActionResult CreateTheme(string subforum)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            var subforumNames = Dao.GetSubforumNames();
            if (!subforumNames.Contains(subforum))
                return HttpNotFound();

            ViewBag.Subforum = subforum;

            if (Request.HttpMethod == "GET")
                return View();

            var theme = new CreationTheme()
            {
                Title = Request.Params["title"],
                AuthorName = LoggedUserName,
                Content = Request.Params["content"],
                CreatedOn = DateTime.Now,
                Kind = (Kind)int.Parse(Request.Params["kind"]),
            };

            if (Dao.CreateTheme(subforum, theme))
            {
                ViewBag.Title = "Creation Successful";
                ViewBag.Theme = theme.Title;
            }
            else
            {
                ViewBag.Title = "Creation Failed";
            }

            return View("CreateThemeResult");
        }

        [Route("EditTheme/{subforum}/{title}")]
        public ActionResult EditTheme(string subforum, string title)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanEditOrDeleteTheme(LoggedUserName, subforum, title))
                return View("NotAuthorized");

            ViewBag.Subforum = subforum;
            ViewBag.Theme = title;

            if (Request.HttpMethod == "GET")
                return View();

            var kind = (Kind)int.Parse(Request.Params["kind"]);
            var content = Request.Params["content"];

            if (Dao.EditTheme(subforum, title, kind, content))
                ViewBag.Title = "Editing Successful";
            else
                ViewBag.Title = "Editing Failed";

            return View("EditThemeResult");
        }

        [Route("DeleteTheme/{subforum}/{title}")]
        public ActionResult DeleteTheme(string subforum, string title)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanEditOrDeleteTheme(LoggedUserName, subforum, title))
                return View("NotAuthorized");

            if (Dao.DeleteTheme(subforum, title))
                ViewBag.Title = "Deletion Successful";
            else
                ViewBag.Title = "Deletion Failed";

            ViewBag.Subforum = subforum;

            return View("DeleteThemeResult");
        }

        [Route("EditComment/{id:int}")]
        public ActionResult EditComment(int id)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanEditComment(LoggedUserName, (uint) id))
                return View("NotAuthorized");

            ViewBag.Comment = Dao.GetComment((uint) id);

            if (Request.HttpMethod == "GET")
                return View();

            var comment = new EditComment()
            {
                Id = (uint)id,
                Content = Request.Params["content"]
            };

            if (Dao.EditComment(LoggedUserName, (uint)id, comment))
                ViewBag.Title = "Editing Successful";
            else
                ViewBag.Title = "Editing Failed";

            return View("EditCommentResult");
        }

        [Route("DeleteComment/{id:int}")]
        public ActionResult DeleteComment(int id)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanDeleteComment(LoggedUserName, (uint)id))
                return View("NotAuthorized");

            if (Dao.DeleteComment((uint)id))
                ViewBag.Title = "Deletion Successful";
            else
                ViewBag.Title = "Deletion Failed";

            return View("DeleteCommentResult");
        }

        [Route("AddModerator/{subforum}")]
        public ActionResult AddModerator(string subforum)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanAddOrRemoveModerator(LoggedUserName, subforum))
                return View("NotAuthorized");

            ViewBag.Subforum = subforum;

            if (Request.HttpMethod == "GET")
                return View();

            var name = Request.Params["name"];

            if (Dao.AddModerator(subforum, name))
                ViewBag.Title = "Adding Successful";
            else
                ViewBag.Title = "Adding Failed";

            return View("AddModeratorResult");
        }

        [Route("RemoveModerator/{subforum}")]
        public ActionResult RemoveModerator(string subforum)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (!Dao.CanAddOrRemoveModerator(LoggedUserName, subforum))
                return View("NotAuthorized");

            ViewBag.Subforum = subforum;

            if (Request.HttpMethod == "GET")
                return View();

            var name = Request.Params["name"];

            if (Dao.RemoveModerator(subforum, name))
                ViewBag.Title = "Removal Successful";
            else
                ViewBag.Title = "Removal Failed";

            return View("RemoveModeratorResult");
        }

        [Route("ReplyComment/{id:int}")]
        public ActionResult ReplyComment(int id)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            ViewBag.Comment = Dao.GetComment((uint)id);

            if (Request.HttpMethod == "GET")
                return View();

            var comment = new ReplyComment()
            {
                Parent = (uint)id,
                AuthorName = LoggedUserName,
                Content = Request.Params["content"],
                CreatedOn = DateTime.Now,
                SubforumName = ViewBag.Comment.SubforumName,
                ThemeTitle = ViewBag.Comment.ThemeTitle
            };

            if (Dao.AddComment(comment))
                ViewBag.Title = "Replying Successful";
            else
                ViewBag.Title = "Replying Failed";

            return View("ReplyResult");
        }

        [Route("ReplyTopLevel/{subforum}/{title}")]
        public ActionResult ReplyTopLevel(string subforum, string title)
        {
            if (LoggedUserName == null)
                return View("NotLoggedIn");

            ViewBag.Comment = new ReplyComment();
            ViewBag.Comment.SubforumName = subforum;
            ViewBag.Comment.ThemeTitle = title;

            if (Request.HttpMethod == "GET")
                return View();

            var comment = new ReplyTopLevel()
            {
                AuthorName = LoggedUserName,
                Content = Request.Params["content"],
                CreatedOn = DateTime.Now,
                SubforumName = subforum,
                ThemeTitle = title
            };

            if (Dao.AddTopLevelComment(comment))
                ViewBag.Title = "Replying Successful";
            else
                ViewBag.Title = "Replying Failed";

            return View("ReplyResult");
        }

        [Route("VoteComment/{pos:int}/{id:int}")]
        public ActionResult VoteComment(int pos, int id)
        {
            if (pos != 1 && pos != 0)
                return HttpNotFound();

            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (Dao.VoteComment(LoggedUserName, (uint) id, pos == 1))
                ViewBag.Title = "Voting Successful";
            else
                ViewBag.Title = "Voting Failed";

            return View("VoteResult");
        }

        [Route("VoteTheme/{pos:int}/{subforum}/{theme}")]
        public ActionResult VoteTheme(int pos, string subforum, string theme)
        {
            if (pos != 1 && pos != 0)
                return HttpNotFound();

            if (LoggedUserName == null)
                return View("NotLoggedIn");

            if (Dao.VoteTheme(LoggedUserName, subforum, theme, pos == 1))
                ViewBag.Title = "Voting Successful";
            else
                ViewBag.Title = "Voting Failed";

            return View("VoteResult");
        }

        [Route("Search")]
        [Route("Search/{id:int}")]
        public ActionResult Search(int? id)
        {
            if (Request.HttpMethod == "GET")
                return View();

            switch (id)
            {
            case 0:
                ViewBag.Results = Dao.SearchSubforums(new SearchSubforums()
                {
                    Name = Request.Params["subforum-name"],
                    Description = Request.Params["subforum-description"],
                    MainModerator = Request.Params["subforum-mainmoderator"]
                });
                break;

            case 1:
                ViewBag.Results = Dao.SearchThemes(new SearchThemes()
                {
                    Title = Request.Params["theme-title"],
                    Content = Request.Params["theme-content"],
                    AuthorName = Request.Params["theme-author"],
                    SubforumName = Request.Params["theme-subforum"]
                });
                break;

            case 2:
                ViewBag.Results = Dao.SearchUsers(new SearchUsers()
                {
                    Name = Request.Params["user-name"]
                });
                break;

            default:
                return HttpNotFound();
            }

            ViewBag.ResultType = id;
            return View("SearchResult");
        }


        private string LoggedUserName => Request.Cookies[CookieKeys.Login]?.Value;
        private IDao Dao => (IDao)HttpContext.Application[ApplicationKeys.Dao];
    }
}