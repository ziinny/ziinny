using ITHELPDESK.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITHELPDESK.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        // GET: Upload
        [HttpGet]
        public ActionResult FilesRepository()
        {
            var dir = new DirectoryInfo(Server.MapPath("~/Content/UserFiles/Folders"));
            FileInfo[] fileNames = dir.GetFiles("*.*"); List<FilesList> items = new List<FilesList>();
            foreach (var file in fileNames)
            {
                //items.Add(file.Name);
                items.Add(new FilesList { FileName = file.Name });
            }

            return View(items.ToList());
        }

        public ActionResult Async_Save(IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var physicalPath = Path.Combine(Server.MapPath("~/Content/UserFiles/Folders"), fileName);

                    file.SaveAs(physicalPath);
                }
            }
            return Content("");
        }

        public ActionResult Async_Remove(string[] fileNames)
        {
            // The parameter of the Remove action must be called "fileNames"

            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    var fileName = Path.GetFileName(fullName);
                    var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                    {
                        // The files are not actually removed in this demo
                        // System.IO.File.Delete(physicalPath);
                    }
                }
            }

            // Return an empty string to signify success
            return Content("");
        }

        public FileResult DownloadFile(string fileName)
        {
            var FileVirtualPath = "~/Content/UserFiles/Folders/" + fileName;
            return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));
        }

        public ActionResult DeleteFile(string fileName)
        {
            var rootFolder = Server.MapPath("~/Content/UserFiles/Folders");
            try
            {
                if (System.IO.File.Exists(Path.Combine(rootFolder, fileName)))
                {
                    System.IO.File.Delete(Path.Combine(rootFolder, fileName));
                }
                return Content("");
            }
            catch (IOException ioE)
            {
                throw ioE;
            }
        }

    }
}