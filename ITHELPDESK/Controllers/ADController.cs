using ITHELPDESK.Entity;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ITHELPDESK.Controllers
{
    [Authorize]
    public class ADController : Controller
    {
        private readonly string domainAuth;

        private readonly string user_Admin;
        private readonly string pass_Admin;
        public ADController()
        {
            domainAuth = ConfigurationManager.AppSettings["domainAuthen"];
            #region sa
            user_Admin = "itadminksp";
            pass_Admin = "password@202747";
            #endregion
        }

        // GET: AD
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ADIndex()
        {
            //string cookieName = User.Identity.Name;
            //ViewBag.UserName = cookieName.ToLower();
            return View();
        }

        public ActionResult LoggingPage()
        {
            return View();
        }

        public ActionResult GetLogging([DataSourceRequest] DataSourceRequest request)
        {
            using (var ctx = new ctxITHELPDESK())
            {
                var getLog = ctx.tbLoggingADUsers.ToList();
                return Json(getLog.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }


        public string GetTaskName(int taskID)
        {
            using (var ctx = new ctxITHELPDESK())
            {
                var logName = ctx.tbLogTasks.Where(x => x.TaskID == taskID && x.delflag == "n").FirstOrDefault();
                if (logName != null)
                    return logName.TaskName;
                else
                    return "Undefined Task Name";
            }
        }


        public JsonResult GetUsersAD()
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    List<UsersAD> users = new List<UsersAD>();
                    string status = string.Empty;
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry ?? new DirectoryEntry();

                        UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, (string)de.Properties["samAccountName"].Value);
                        DateTime? passwordLastSet = user.LastPasswordSet;

                        int pwdAge = 0;
                        if (passwordLastSet != null)
                        {
                            pwdAge = GetPasswordAge(passwordLastSet);
                        }

                        users.Add(new UsersAD
                        {
                            FirstName = de.Properties["givenName"].Value as string ?? "",
                            LastName = de.Properties["sn"].Value as string ?? "",
                            samAccount = de.Properties["samAccountName"].Value as string ?? "",
                            principalName = de.Properties["userPrincipalName"].Value as string ?? "",
                            Department = de.Properties["department"].Value as string ?? "",
                            DisplayDropdown = $"{de.Properties["samAccountName"].Value as string}",
                            distinguishedName = de.Properties["distinguishedName"].Value as string ?? "",
                            PwdLastSet = pwdAge,
                            status = IsActive(de) == true ? "Enable" : "Disabled"
                        });
                    }

                    var items = users.OrderBy(x => x.samAccount).Select(s => new UsersAD { FirstName = s.FirstName, LastName = s.LastName, samAccount = s.samAccount, principalName = s.principalName, BadPwdCount = s.BadPwdCount, DisplayDropdown = s.DisplayDropdown, Department = s.Department, PwdLastSet = s.PwdLastSet, status = s.status, distinguishedName = s.distinguishedName });

                    return Json(items, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult GetUsersAD_BySamAccount(string samAccount)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    UsersAD userAD = new UsersAD();
                    string status = string.Empty;
                    var result = searcher.FindAll().Where(x => x.SamAccountName.ToUpper().Contains(samAccount.Trim().ToUpper())).FirstOrDefault();
                    DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry ?? new DirectoryEntry();

                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, (string)de.Properties["samAccountName"].Value);
                    DateTime? passwordLastSet = user.LastPasswordSet;

                    int pwdAge = 0;
                    if (passwordLastSet != null)
                    {
                        pwdAge = GetPasswordAge(passwordLastSet);
                    }

                    userAD.FirstName = de.Properties["givenName"].Value as string ?? "";
                    userAD.LastName = de.Properties["sn"].Value as string ?? "";
                    userAD.samAccount = de.Properties["samAccountName"].Value as string ?? "";
                    userAD.principalName = de.Properties["userPrincipalName"].Value as string ?? "";
                    userAD.Department = de.Properties["department"].Value as string ?? "";
                    userAD.DisplayDropdown = $"{de.Properties["samAccountName"].Value as string}";
                    userAD.PwdLastSet = pwdAge;
                    userAD.status = IsActive(de) == true ? "Enable" : "Disabled";


                    return Json(userAD, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }

        public int GetPasswordAge(DateTime? pwdLastSet)
        {
            DateTime currentDate = DateTime.Now;
            TimeSpan passwordAge = currentDate - pwdLastSet.Value;
            int passwordAge_Date = passwordAge.Days;

            return passwordAge_Date;
        }




        public ActionResult GetUserFromAllDomain([DataSourceRequest] DataSourceRequest request, string username)
        {
            var DC = DomainControllerList();
            List<AD_Details> aD_Details = new List<AD_Details>();

            bool IsLock = false;
            foreach (var item in DC)
            {
                var ping = new Ping();
                var reply = ping.Send(item.IPAddress, 1000);

                if (reply.Status == IPStatus.Success)
                {
                    using (var context = new PrincipalContext(ContextType.Domain, item.IPAddress, user_Admin, pass_Admin))
                    {
                        using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                        {
                            var result = searcher.FindAll().Where(x => x.SamAccountName.ToUpper().Contains(username.Trim().ToUpper())).FirstOrDefault();
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;

                            if (de.Properties["badPwdCount"].Value as int? == 3)
                            {
                                IsLock = true;
                            }

                            aD_Details.Add(new AD_Details
                            {
                                DC_Name = item.DomainName,
                                Site = item.Site,
                                User_State = de.Properties["badPwdCount"].Value as int? < 3 ? "Active" : "Locked",
                                Bad_Pwd_Count = de.Properties["badPwdCount"].Value as int?
                            });
                        }
                    }
                }
                else
                {
                    aD_Details.Add(new AD_Details
                    {
                        DC_Name = item.DomainName,
                        Site = item.Site,
                        User_State = "Server is currently down",
                        Bad_Pwd_Count = default(int)
                    });
                }
            }

            foreach (var item in aD_Details)
            {
                if (item.Bad_Pwd_Count is null)
                {
                    item.User_State = "Not Found";
                }
            }

            return Json(aD_Details.Select(s => new AD_Details { DC_Name = s.DC_Name, Bad_Pwd_Count = s.Bad_Pwd_Count, Site = s.Site, User_State = s.User_State, IsLock = IsLock }).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public JsonResult UnlockUserFromAD(string samAccount)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccount);
                if (user != null)
                {
                    try
                    {
                        user.UnlockAccount();
                        using (var logging = new ctxITHELPDESK())
                        {
                            var taskName = GetTaskName(1001);
                            logging.tbLoggingADUsers.Add(new tbLoggingADUser { taskName = taskName, submittedBy = User.Identity.Name, userName = samAccount, submittedDate = DateTime.Now });
                            logging.SaveChanges();
                        }

                        return Json(new { message = $"User : {samAccount} Unlocked !" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { message = "Not found user .." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult UnlockUserFromAllAD(string samAccount, string task = "Unlock")
        {
            var DC = DomainControllerList();

            try
            {
                foreach (var item in DC)
                {
                    var ping = new Ping();
                    var reply = ping.Send(item.IPAddress, 1000);

                    if (reply.Status == IPStatus.Success)
                    {
                        using (var context = new PrincipalContext(ContextType.Domain, item.IPAddress, user_Admin, pass_Admin))
                        {
                            UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccount);
                            if (user != null)
                            {
                                user.UnlockAccount();
                            }
                        }
                    }
                }

                if (task == "Unlock")
                {
                    using (var logging = new ctxITHELPDESK())
                    {
                        var taskName = GetTaskName(1001);
                        logging.tbLoggingADUsers.Add(new tbLoggingADUser { taskName = taskName, submittedBy = User.Identity.Name, userName = samAccount, submittedDate = DateTime.Now });
                        logging.SaveChanges();
                    }
                }

                return Json(new { message = $"User : {samAccount} Unlocked !" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        public JsonResult ResetPassword(string samAccount, string newPassword)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccount);
                try
                {
                    if (user != null)
                    {
                        user.SetPassword(newPassword);
                        user.Save();
                        UnlockUserFromAllAD(samAccount, "Reset");

                        using (var logging = new ctxITHELPDESK())
                        {
                            var taskName = GetTaskName(1002);
                            logging.tbLoggingADUsers.Add(new tbLoggingADUser { taskName = taskName, submittedBy = User.Identity.Name, userName = samAccount, submittedDate = DateTime.Now });
                            logging.SaveChanges();
                        }

                        return Json(new { result = true, message = "Reset Password Successful !" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { result = false, message = "Reset Password Failed !", error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }

        public JsonResult DisableUser(string samAccount)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccount);
                try
                {
                    if (user != null)
                    {
                        if (user.Enabled == true)
                        {
                            user.Enabled = false;
                            user.Save();

                            using (var logging = new ctxITHELPDESK())
                            {
                                var taskName = GetTaskName(1004);
                                logging.tbLoggingADUsers.Add(new tbLoggingADUser { taskName = taskName, submittedBy = User.Identity.Name, userName = samAccount, submittedDate = DateTime.Now });
                                logging.SaveChanges();
                            }

                            return Json(new { result = true, message = "User is Disabled !" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { result = false, message = "user disable failed !", error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }

        public JsonResult EnableUser(string samAccount)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccount);
                try
                {
                    if (user.Enabled == false)
                    {
                        user.Enabled = true;
                        user.Save();

                        using (var logging = new ctxITHELPDESK())
                        {
                            var taskName = GetTaskName(1003);
                            logging.tbLoggingADUsers.Add(new tbLoggingADUser { taskName = taskName, submittedBy = User.Identity.Name, userName = samAccount, submittedDate = DateTime.Now });
                            logging.SaveChanges();
                        }

                        return Json(new { result = true, message = "User is Activated  !" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { result = false, message = "User enable failed !", error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }



        public JsonResult GetAllGroup()
        {
            List<string> result = new List<string>();
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                GroupPrincipal qbeGroup = new GroupPrincipal(context);

                PrincipalSearcher srch = new PrincipalSearcher(qbeGroup);

                foreach (Principal group in srch.FindAll())
                {
                    result.Add(group.Name);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public JsonResult GetMembersFromGroup()
        {
            List<string> members = new List<string>();
            using (var context = new PrincipalContext(ContextType.Domain, domainAuth, user_Admin, pass_Admin))
            {
                GroupPrincipal grp = GroupPrincipal.FindByIdentity(context, "MyGroup");
                List<string> lst = grp.Members.Select(g => g.SamAccountName).ToList();

                return Json(members, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetDomainLists()
        {
            List<DomainList> DC_List = DomainControllerList();
            return Json(DC_List, JsonRequestBehavior.AllowGet);
        }


        //public List<DomainList> DomainControllerList()
        //{
        //    List<DomainList> DC_List = new List<DomainList>();
        //    Forest currentForest = Forest.GetCurrentForest();
        //    DomainCollection domains = currentForest.Domains;
        //    foreach (Domain objDomain in domains)
        //    {
        //        DomainControllerCollection DCs = objDomain.DomainControllers;
        //        if (DCs.Count > 0)
        //        {
        //            foreach (DomainController controller in DCs)
        //            {
        //                DC_List.Add(new DomainList { DomainName = controller.Name.Split('.')[0], IPAddress = controller.IPAddress, Site = controller.SiteName });
        //            }
        //        }
        //    }
        //    return DC_List;
        //}

        public List<DomainList> DomainControllerList()
        {
            List<DomainList> DC_List = new List<DomainList>();
            DC_List.Add(new DomainList { DomainName = "KSPUTH-DC", IPAddress = "192.168.104.5", Site = "KSPUTH" });
            DC_List.Add(new DomainList { DomainName = "KSPUTH12P-DC", IPAddress = "192.168.104.30", Site = "KSPUTH" });
            DC_List.Add(new DomainList { DomainName = "KSPUTH12S-DC", IPAddress = "192.168.104.31", Site = "KSPUTH" });
            DC_List.Add(new DomainList { DomainName = "SPKSP", IPAddress = "192.168.104.8", Site = "KSPUTH" });
            DC_List.Add(new DomainList { DomainName = "KPHQ-AD", IPAddress = "196.196.196.6", Site = "KPHQ" });
            DC_List.Add(new DomainList { DomainName = "KPHQ-DC", IPAddress = "196.196.196.2", Site = "KPHQ" });

            return DC_List;
        }



        public class DomainList
        {
            public string DomainName { get; set; }
            public string IPAddress { get; set; }
            public string Site { get; set; }
        }

        public class AD_Details
        {
            public string DC_Name { get; set; }
            public string Site { get; set; }
            public string User_State { get; set; }
            public int? Bad_Pwd_Count { get; set; }
            public bool IsLock { get; set; }
        }


        public class UsersAD
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string samAccount { get; set; }
            public string principalName { get; set; }
            public int? BadPwdCount { get; set; }
            public string Department { get; set; }
            public string DisplayDropdown { get; set; }
            public int PwdLastSet { get; set; }
            public string status { get; set; }
            public string distinguishedName { get; set; }
        }

    }
}