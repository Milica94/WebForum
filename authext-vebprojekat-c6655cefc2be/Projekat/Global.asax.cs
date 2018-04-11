using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Projekat.Dao;

namespace Projekat
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var dbPath = Server.MapPath("~/App_Data/data.db");
            _conn = new SQLiteConnection($"Data Source={dbPath}; Version=3; Foreign Keys=True;");
            _conn.Open();

            Application[ApplicationKeys.Dao] = new DbDao(_conn);
        }

        protected void Application_End()
        {
            _conn.Close();
        }


        private SQLiteConnection _conn;
    }


    public class ApplicationKeys
    {
        public const string Dao = "Dao";
    }


    public class CookieKeys
    {
        public const string Login = "webapp-login";
    }
}
