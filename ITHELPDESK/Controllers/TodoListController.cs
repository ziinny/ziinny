using ITHELPDESK.Entity;
using ITHELPDESK.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ITHELPDESK.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private ctxITHELPDESK db;


        public TodoListController()
        {
            db = new ctxITHELPDESK();
        }

        public ActionResult Index()
        {
            var Username = User.Identity.Name;
            var User_Info = db.tbUsers.Where(x => x.Username == Username && x.Delflag == "n").FirstOrDefault();
            if (User_Info.Location == "KSP")
                ViewBag.Select = 0;
            else if (User_Info.Location == "HQ")
                ViewBag.Select = 1;

            return View();
        }

        public ActionResult TaskManageKSP()
        {
            return View();
        }

        public ActionResult TaskManageHQ()
        {
            return View();
        }


        public ActionResult SubCardEdit(string ComputerName, int? TaskID)
        {
            var item = db.tbCardViewModels.Where(x => x.ComputerName == ComputerName && x.OrderBy == TaskID && x.delflag == "n").FirstOrDefault();
            return View();
        }



        [HttpPost]
        public ActionResult CustomDropZone_Save(HttpPostedFileBase file, int taskID)
        {
            if (file != null)
            {
                var fileName = Path.GetFileName(file.FileName.Trim('"'));
                var physicalPath = Path.Combine(Server.MapPath("~/Content/web/taskboard"), fileName);

                var item = db.tbCardTaskKSP.Where(x => x.TaskID == taskID && x.delflag == "n").FirstOrDefault();
                if (item.Image == null)
                {
                    item.Image = fileName;
                    db.SaveChanges();
                    file.SaveAs(physicalPath);
                }
                else
                {
                    var ExistingFile = Path.Combine(Server.MapPath("~/Content/web/taskboard"), item.Image);
                    if (System.IO.File.Exists(ExistingFile))
                    {
                        System.IO.File.Delete(ExistingFile);
                    }
                    item.Image = fileName;
                    db.SaveChanges();
                    file.SaveAs(physicalPath);
                }
            }

            return Content("");
        }

        [HttpPost]
        public ActionResult CustomDropZone_Remove(HttpPostedFileBase file)
        {
            if (file != null)
            {
                var fileName = Path.GetFileName(file.FileName.Trim('"'));
                var physicalPath = Path.Combine(Server.MapPath("~/Content/web/taskboard"), fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            return Content("");
        }


        public ActionResult EditTaskKSP(int taskID)
        {
            var item = db.tbCardTaskKSP.Where(x => x.TaskID == taskID && x.delflag == "n").Select(s => new TaskModel
            {
                TaskID = s.TaskID,
                TaskName = s.TaskName,
                Description = s.Description,
                Image = s.Image,
                FileName = string.Empty
            }).FirstOrDefault();

            if (!string.IsNullOrEmpty(item.Image))
            {
                ViewBag.hasImg = item.Image;
            }

            return View(item);
        }


        [HttpPost]
        public ActionResult EditTaskKSP(TaskModel task)
        {
            var item = db.tbCardTaskKSP.Where(x => x.TaskID == task.TaskID && x.delflag == "n").FirstOrDefault();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    item.TaskName = task.TaskName;
                    item.Description = task.Description;

                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return View();
                }
            }

            return RedirectToAction("TaskManageKSP");
        }

        public ActionResult EditTaskHQ(int taskID)
        {
            var task = db.tbCardTaskHQ.Where(x => x.TaskID == taskID && x.delflag == "n").FirstOrDefault();
            return View();
        }

        public ActionResult TaskKPS_Read([DataSourceRequest] DataSourceRequest request)
        {
            var tasks = db.tbCardTaskKSP.Where(x => x.delflag == "n").ToList();

            return Json(tasks.ToDataSourceResult(request));
        }

        public ActionResult TaskHQ_Read([DataSourceRequest] DataSourceRequest request)
        {
            var tasks = db.tbCardTaskHQ.Where(x => x.delflag == "n").ToList();

            return Json(tasks.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult UpdateTask(string computerName, string Username, string Location)
        {
            if (Location == "KSP")
            {
                var allTaskKSP = db.tbCardTaskKSP.Where(x => x.delflag == "n").ToList();
                var selectCardID = db.tbCardViewModels.Where(x => x.delflag == "n" && x.ComputerName == computerName).Select(x => x.OrderBy).ToList();

                var exceptTask = allTaskKSP.Select(x => x.TaskID).Except(selectCardID).ToList();
                var exceptTaskList = allTaskKSP.Where(x => exceptTask.Contains(x.TaskID)).ToList();

                var containTask = allTaskKSP.Where(x => selectCardID.Contains(x.TaskID)).ToList();

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var allCard = db.tbCardViewModels.Where(x => x.delflag == "n" && x.ComputerName == computerName).ToList();
                        foreach (var item in allCard)
                        {
                            var task = allTaskKSP.Where(x => x.TaskID == item.OrderBy).FirstOrDefault();
                            item.Title = task.TaskName;
                            item.Description = task.Description;
                            db.Entry(item).State = EntityState.Modified;
                        }

                        if (exceptTaskList.Count() != 0)
                        {
                            foreach (var exceptItem in exceptTaskList)
                            {
                                db.tbCardViewModels.Add(new tbCardViewModel()
                                {
                                    ComputerName = computerName,
                                    Username = Username,
                                    Title = exceptItem.TaskName,
                                    OrderBy = exceptItem.TaskID,
                                    Description = exceptItem.Description,
                                    Status = 1,
                                    Color = 1,
                                    Location = "KSP",
                                    Image = exceptItem.TaskID.ToString(),
                                    delflag = "n",
                                    UserCreate = User.Identity.Name,
                                    dateCreate = DateTime.Now
                                });
                            }
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        return Json(new { IsError = false, message = $"Update {computerName} successfull !" }, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return Json(new { IsError = true, message = $"Update {computerName} failed !" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            if (Location == "HQ")
            {
                var allTaskHQ = db.tbCardTaskHQ.Where(x => x.delflag == "n").ToList();
                var selectCardID = db.tbCardViewModels.Where(x => x.delflag == "n" && x.ComputerName == computerName).Select(x => x.OrderBy).ToList();

                var exceptTask = allTaskHQ.Select(x => x.TaskID).Except(selectCardID).ToList();
                var exceptTaskList = allTaskHQ.Where(x => exceptTask.Contains(x.TaskID)).ToList();

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in exceptTaskList)
                        {
                            db.tbCardViewModels.Add(new tbCardViewModel()
                            {
                                ComputerName = computerName,
                                Username = Username,
                                Title = item.TaskName,
                                OrderBy = item.TaskID,
                                Description = item.Description,
                                Status = 1,
                                Color = 1,
                                Location = "HQ",
                                Image = item.TaskID.ToString(),
                                delflag = "n",
                                UserCreate = User.Identity.Name,
                                dateCreate = DateTime.Now
                            });
                        }

                        db.SaveChanges();
                        transaction.Commit();
                        return Json(new { message = $"Update {computerName} successfull !" }, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {
                        transaction.Rollback();
                        return Json(new { message = $"Update {computerName} failed !" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(new { message = $"Not found !" });

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateTaskKSP([DataSourceRequest] DataSourceRequest request, tbCardTaskKSP task)
        {
            var tasks = db.tbCardTaskKSP.Where(x => x.delflag == "n").Select(x => x.TaskID).ToList();

            if (!tasks.Contains(task.TaskID))
            {
                using (var transaction = db.Database.BeginTransaction())
                {

                }
                db.tbCardTaskKSP.Add(new tbCardTaskKSP
                {
                    TaskID = task.TaskID,
                    TaskName = task.TaskName,
                    Description = task.Description,
                    delflag = "n"
                });
                db.SaveChanges();
            }
            else
            {
                return Json(new DataSourceResult
                {
                    Errors = "Task ID Duplicate !"
                });
            }


            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RemoveTaskKSP([DataSourceRequest] DataSourceRequest request, tbCardTaskKSP task)
        {
            using (var ctx = new ctxITHELPDESK())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    var selectTask = ctx.tbCardTaskKSP.Where(x => x.TaskID == task.TaskID && x.delflag == "n").FirstOrDefault();

                    if (selectTask != null)
                    {
                        selectTask.delflag = "y";
                        ctx.Entry(selectTask).State = EntityState.Modified;

                        try
                        {
                            ctx.SaveChanges();
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return Json(new DataSourceResult
                            {
                                Errors = "Task not found or cannot delete !"
                            });
                        }
                    }
                }
            }

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateTaskHQ([DataSourceRequest] DataSourceRequest request, tbCardTaskHQ task)
        {
            var tasks = db.tbCardTaskHQ.Where(x => x.delflag == "n").Select(x => x.TaskID).ToList();

            if (!tasks.Contains(task.TaskID))
            {
                using (var transaction = db.Database.BeginTransaction())
                {

                }
                db.tbCardTaskKSP.Add(new tbCardTaskKSP
                {
                    TaskID = task.TaskID,
                    TaskName = task.TaskName,
                    Description = task.Description,
                    delflag = "n"
                });
                db.SaveChanges();
            }
            else
            {
                return Json(new DataSourceResult
                {
                    Errors = "Task ID Duplicate !"
                });
            }


            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RemoveTaskHQ([DataSourceRequest] DataSourceRequest request, tbCardTaskHQ task)
        {
            using (var ctx = new ctxITHELPDESK())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    var selectTask = ctx.tbCardTaskHQ.Where(x => x.TaskID == task.TaskID && x.delflag == "n").FirstOrDefault();

                    if (selectTask != null)
                    {
                        selectTask.delflag = "y";
                        ctx.Entry(selectTask).State = EntityState.Modified;

                        try
                        {
                            ctx.SaveChanges();
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return Json(new DataSourceResult
                            {
                                Errors = "Task not found or cannot delete !"
                            });
                        }
                    }
                }
            }

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }






        public IList<MachineInformation> GetAllIndex()
        {
            var groupByComputerName = db.tbCardViewModels.Where(x => x.delflag == "n").GroupBy(x => x.ComputerName, (key, group) => new MachineInformation { ComputerName = key, Tasks = group.Count(), Location = group.Select(x => x.Location).FirstOrDefault(), Username = group.Select(x => x.Username).FirstOrDefault() }).ToList();

            return groupByComputerName;
        }

        public ActionResult CardViewKSP_Read([DataSourceRequest] DataSourceRequest request)
        {
            var groupByComputerName = db.tbCardViewModels.Where(x => x.delflag == "n" && x.Location == "KSP").GroupBy(x => x.ComputerName, (key, group) => new MachineInformation { ComputerName = key, Tasks = group.Count(), Location = group.Select(x => x.Location).FirstOrDefault(), Username = group.Select(x => x.Username).FirstOrDefault() }).ToList();

            return Json(groupByComputerName.ToDataSourceResult(request));
        }


        //public ActionResult LogCardViewKSP_Read([DataSourceRequest] DataSourceRequest request)
        //{
        //    var Logging = db.tbCardViewModels.Where(x => x.delflag == "y" && x.Location == "KSP") 1128400
        //        .GroupBy(g => new { g.ComputerName, g.Username })
        //        .Select(grp => new MachineInformation
        //        {
        //            ComputerName = grp.Key.ComputerName,
        //            Username = grp.Key.Username,
        //            Location = grp.Select(x => x.Location).FirstOrDefault(),
        //            Tasks = grp.Count()
        //        }).ToList();

        //    return Json(Logging.ToDataSourceResult(request));
        //}

        public ActionResult CardView_Read_Details([DataSourceRequest] DataSourceRequest request, string ComputerName)
        {
            var items = db.tbCardViewModels.Where(x => x.delflag == "n" && x.ComputerName == ComputerName).ToList();
            var result = (from a in items
                          join b in db.tbCardStatus on a.Status equals b.StatusID
                          select new CardReadDetails
                          {
                              ComputerName = a.ComputerName,
                              TaskID = a.OrderBy,
                              Title = a.Title,
                              Description = a.Description,
                              StatusName = b.StatusName
                          }).ToList();
            return Json(result.ToDataSourceResult(request));
        }

        public ActionResult CardViewHQ_Read([DataSourceRequest] DataSourceRequest request)
        {
            var groupByComputerName = db.tbCardViewModels.Where(x => x.delflag == "n" && x.Location == "HQ").GroupBy(x => x.ComputerName, (key, group) => new MachineInformation { ComputerName = key, Tasks = group.Count(), Location = group.Select(x => x.Location).FirstOrDefault(), Username = group.Select(x => x.Username).FirstOrDefault() }).ToList();

            return Json(groupByComputerName.ToDataSourceResult(request));
        }


        public ActionResult Task(string ComputerName, string Location)
        {
            ViewBag.ComputerName = ComputerName;
            ViewBag.Cards = GetCards(ComputerName, Location);

            return View();
        }


        private IList<CardViewModel> GetCards(string ComputerName, string Location)
        {
            var GetCardViewModels = db.tbCardViewModels.Where(x => x.ComputerName == ComputerName && x.delflag == "n").ToList();

            var result = new List<CardViewModel>();
            if (Location == "KSP")
            {
                result = (from a in GetCardViewModels
                          join b in db.tbCardStatus on a.Status equals b.StatusID
                          join c in db.tbCardColors on a.Color equals c.ColorID
                          join d in db.tbCardTaskKSP on a.Image equals d.TaskID.ToString()
                          select new CardViewModel
                          {
                              ID = a.ID,
                              Title = a.Title,
                              Order = a.OrderBy,
                              Description = a.Description,
                              Status = b.StatusName,
                              Color = c.ColorName,
                              Image = d.Image,
                              userCreate = a.UserCreate,
                              userEdit = a.UserEdit,
                              dateCreate = a.dateCreate,
                              dateEdit = a.dateEdit
                          }).ToList();
            }
            else if (Location == "HQ")
            {
                result = (from a in GetCardViewModels
                          join b in db.tbCardStatus on a.Status equals b.StatusID
                          join c in db.tbCardColors on a.Color equals c.ColorID
                          join d in db.tbCardTaskHQ on a.Image equals d.TaskID.ToString()
                          select new CardViewModel
                          {
                              ID = a.ID,
                              Title = a.Title,
                              Order = a.OrderBy,
                              Description = a.Description,
                              Status = b.StatusName,
                              Color = c.ColorName,
                              Image = d.Image,
                              userCreate = a.UserCreate,
                              userEdit = a.UserEdit,
                              dateCreate = a.dateCreate,
                              dateEdit = a.dateEdit
                          }).ToList();
            }

            return result;
        }


        public JsonResult EditTask(int id, string status, string color)
        {
            var username = User.Identity.Name;
            using (var ctx = new ctxITHELPDESK())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var colorID = ctx.tbCardColors.Where(x => x.ColorName == color).FirstOrDefault().ColorID;
                        var statusID = ctx.tbCardStatus.Where(x => x.StatusName == status).FirstOrDefault().StatusID;

                        var task = ctx.tbCardViewModels.Find(id);
                        task.Status = statusID;
                        task.Color = colorID;
                        task.UserEdit = username;
                        task.dateEdit = DateTime.Now;

                        ctx.Entry(task).State = EntityState.Modified;
                        ctx.SaveChanges();
                        transaction.Commit();

                        return Json(new { message = "success !" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { message = $"Error ! {ex}" });
                    }

                }
            }

        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateCardKSP([DataSourceRequest] DataSourceRequest request, MachineInformation product)
        {
            var username = User.Identity.Name;
            var existRecord = db.tbCardViewModels.Where(x => x.ComputerName == product.ComputerName && x.delflag == "n").FirstOrDefault() != null;

            if (ModelState.IsValid)
            {
                if (!existRecord)
                {
                    using (var ctx = new ctxITHELPDESK())
                    {
                        using (var transaction = ctx.Database.BeginTransaction())
                        {
                            var Tasks = ctx.tbCardTaskKSP.Where(x => x.delflag == "n").ToList();

                            foreach (var item in Tasks)
                            {
                                ctx.tbCardViewModels.Add(new tbCardViewModel()
                                {
                                    ComputerName = product.ComputerName.ToUpper(),
                                    Username = product.Username,
                                    Title = item.TaskName,
                                    OrderBy = item.TaskID,
                                    Description = item.Description,
                                    Status = 1,
                                    Color = 1,
                                    Location = "KSP",
                                    Image = item.TaskID.ToString(),
                                    delflag = "n",
                                    UserCreate = username,
                                    dateCreate = DateTime.Now
                                });
                            }

                            try
                            {
                                ctx.SaveChanges();
                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                            }
                        }
                    }
                }
                else
                {
                    return Json(new DataSourceResult
                    {
                        Errors = "Computer Name Duplicate !"
                    });
                }
            }

            return Json(new[] { product }.ToDataSourceResult(request, ModelState));
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateCardHQ([DataSourceRequest] DataSourceRequest request, MachineInformation product)
        {
            var username = User.Identity.Name;
            var existRecord = db.tbCardViewModels.Where(x => x.ComputerName == product.ComputerName && x.delflag == "n").FirstOrDefault() != null;

            if (ModelState.IsValid)
            {
                if (!existRecord)
                {
                    using (var ctx = new ctxITHELPDESK())
                    {
                        using (var transaction = ctx.Database.BeginTransaction())
                        {
                            var Tasks = ctx.tbCardTaskHQ.Where(x => x.delflag == "n").ToList();

                            foreach (var item in Tasks)
                            {
                                ctx.tbCardViewModels.Add(new tbCardViewModel()
                                {
                                    ComputerName = product.ComputerName.ToUpper(),
                                    Username = product.Username,
                                    Title = item.TaskName,
                                    OrderBy = item.TaskID,
                                    Description = item.Description,
                                    Status = 1,
                                    Color = 1,
                                    Location = "HQ",
                                    Image = item.TaskID.ToString(),
                                    delflag = "n",
                                    UserCreate = username,
                                    dateCreate = DateTime.Now
                                });
                            }

                            try
                            {
                                ctx.SaveChanges();
                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                            }
                        }
                    }
                }
                else
                {
                    return Json(new DataSourceResult
                    {
                        Errors = "Computer Name Duplicate !"
                    });
                }
            }

            return Json(new[] { product }.ToDataSourceResult(request, ModelState));
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CardDestroy([DataSourceRequest] DataSourceRequest request, MachineInformation product)
        {

            using (var ctx = new ctxITHELPDESK())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    var GetCards = ctx.tbCardViewModels.Where(x => x.ComputerName == product.ComputerName && x.delflag == "n").ToList();

                    foreach (var item in GetCards)
                    {
                        item.delflag = "y";
                        item.UserDelete = User.Identity.Name;
                        item.dateDelete = DateTime.Now;
                        ctx.Entry(item).State = EntityState.Modified;
                    }

                    try
                    {
                        ctx.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        transaction.Rollback();
                    }
                }
            }

            return Json(new[] { product }.ToDataSourceResult(request, ModelState));

        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CardUpdate([DataSourceRequest] DataSourceRequest request, MachineInformation product)
        {
            using (var ctx = new ctxITHELPDESK())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    var GetCards = ctx.tbCardViewModels.Where(x => x.ComputerName == product.ComputerName && x.delflag == "n").ToList();

                    foreach (var item in GetCards)
                    {
                        item.ComputerName = product.ComputerName;
                        item.Username = product.Username;
                        ctx.Entry(item).State = EntityState.Modified;
                    }

                    try
                    {
                        ctx.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        transaction.Rollback();
                    }
                }
            }

            return Json(new[] { product }.ToDataSourceResult(request, ModelState));
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(MachineInformation product)
        {
            RouteValueDictionary routeValues;

            using (var ctx = new ctxITHELPDESK())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    var GetCards = ctx.tbCardViewModels.Where(x => x.ComputerName == product.ComputerName && x.delflag == "n").ToList();

                    foreach (var item in GetCards)
                    {
                        item.ComputerName = product.ComputerName;
                        item.Username = product.Username;
                        item.Location = product.Location;
                        ctx.Entry(item).State = EntityState.Modified;
                    }

                    try
                    {
                        ctx.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        transaction.Rollback();
                    }
                }
            }

            routeValues = this.GridRouteValues();

            return RedirectToAction("Index", routeValues);
        }




        public class CardViewModel
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public int? Order { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public string Color { get; set; }
            public string Image { get; set; }
            public string userCreate { get; set; }
            public string userEdit { get; set; }
            public DateTime? dateCreate { get; set; }
            public DateTime? dateEdit { get; set; }

        }

        public class MachineInformation
        {
            [Required]
            public string ComputerName { get; set; }
            [Required]
            public string Username { get; set; }
            [HiddenInput(DisplayValue = false)]
            public int Tasks { get; set; }
            [HiddenInput(DisplayValue = false)]
            public string Location { get; set; }
        }


        public class Location
        {
            public int LocationID { get; set; }
            public string LocationName { get; set; }
        }


        public class CardTaskKSP
        {
            [HiddenInput(DisplayValue = false)]
            public int ID { get; set; }

            public int? TaskID { get; set; }

            [StringLength(200)]
            public string TaskName { get; set; }

            public string Description { get; set; }

            [StringLength(200)]
            public string Image { get; set; }

            [StringLength(1)]
            public string delflag { get; set; }

            public string FileName { get; set; }
        }
    }
}