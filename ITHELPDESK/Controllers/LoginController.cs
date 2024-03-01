using ITHELPDESK.Entity;
using ITHELPDESK.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ITHELPDESK.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Login2()
        {
            return View();
        }


        public ActionResult LoginPage()
        {
            Session.Clear();
            return View();
        }

        [HttpPost]
        public ActionResult LoginPage(User user)
        {
            using (var ctx = new ctxITHELPDESK())
            {
                //string[] users = ConfigurationManager.AppSettings["ITUSERS"].Split(';');
                var UserLogin = ctx.tbUsers.Where(x => x.Delflag == "n").Select(x => x.Username).ToList();
                if (!string.IsNullOrWhiteSpace(user.Username) && !string.IsNullOrWhiteSpace(user.Password))
                {
                    if (user.IsValid(user.Username.Trim(), user.Password.Trim()))
                    {
                        if (UserLogin.Any(x => x == user.Username.ToLower()))
                        {
                            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                                1,
                                user.Username,
                                DateTime.Now,
                                DateTime.Now.AddDays(30),
                                false,
                                string.Empty,
                                FormsAuthentication.FormsCookiePath);
                            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                            {
                                HttpOnly = true,
                                Secure = FormsAuthentication.RequireSSL,
                                Path = FormsAuthentication.FormsCookiePath,
                                Domain = FormsAuthentication.CookieDomain
                            };
                            Response.Cookies.Add(cookie);

                            var thisUser = ctx.tbUsers.Where(x => x.Username == user.Username && x.Delflag == "n").FirstOrDefault();
                            thisUser.LastLogin = DateTime.Now;
                            ctx.Entry(thisUser).State = System.Data.Entity.EntityState.Modified;
                            ctx.SaveChanges();

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewBag.msgError = "Only IT Staff .. !";
                            Session["username"] = user.Username;
                            Session["password"] = user.Password;
                        }
                    }
                    else
                    {
                        ViewBag.msgError = "Username or Password is incorrect.. !";
                        Session["username"] = user.Username;
                        Session["password"] = user.Password;
                    }
                }
                return View();
            }
        }


        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("LoginPage", "Login");
        }

        public List<string> ITList()
        {
            try
            {
                List<string> samACC = new List<string>();
                string srvr = ConfigurationManager.AppSettings["LDAPName"];
                var username = "itadminksp"; var password = "password@202747";
                var ctx = new DirectoryEntry(srvr, username, password);
                var searcher = new DirectorySearcher(ctx)
                {
                    Filter = "(&(objectCategory=person)(objectClass=user))",
                    SearchScope = SearchScope.Subtree
                };

                foreach (SearchResult searchEntry in searcher.FindAll())
                {
                    var entry = new DirectoryEntry(searchEntry.GetDirectoryEntry().Path);
                    string sAMAccountName = entry.Properties["sAMAccountName"].Value.ToString();

                    if (sAMAccountName.Contains(".") && searchEntry.Path.Contains("OU=IT") && searchEntry.Path.Contains("OU=KSPUTH"))
                    {
                        samACC.Add(entry.Properties["sAMAccountName"].Value.ToString().ToLower());
                    }
                }
                return samACC;
            }
            catch (Exception)
            {
                return null;
            }
        }


        

    }
}