using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Streaming;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.Text;

namespace ITHELPDESK.Controllers
{
    [Authorize]
    public class SignatureController : Controller
    {
        // GET: Signature
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sign_Pdf_Document(/*string filename*/)
        {
            var pdfQueryString = Request.QueryString["pdf"];
            if (Request.QueryString["pdf"] != null)
            {
                ViewBag.fileName = "~/Content/web/signature/" + pdfQueryString;
                return View();
            }

            //if (filename != null)
            //{
            //    ViewBag.fileName = "~/Content/web/signature/" + filename;
            //    return View();
            //}

            string pdfName = "travelling001.pdf";
            ViewBag.fileName = "~/Content/web/signature/" + pdfName;
            return View();
        }
        public string PreparePdf(string pdfData, string pdfURL)
        {
            byte[] resultingBytes = null;
            byte[] finalBytes = null;

            Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider provider = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider();
            byte[] renderedBytes = Convert.FromBase64String(pdfData);

            RadFixedDocument document1 = null;
            RadFixedDocument document2 = provider.Import(renderedBytes);

            string filePath = Server.MapPath(pdfURL);
            //string filePath = Server.MapPath("~/Content/web/signature/travelling001.pdf");

            using (FileStream input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                document1 = provider.Import(input);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                provider.Export(document1, ms);
                resultingBytes = ms.ToArray();
            }

            finalBytes = AppendContent(resultingBytes, document2);
            string result = Convert.ToBase64String(finalBytes);

            return result;
        }
        private byte[] AppendContent(byte[] resultingBytes, RadFixedDocument document2)
        {
            RadFixedPage foregroundContentOwner = document2.Pages[0];

            MemoryStream ms = new MemoryStream();
            byte[] renderedBytes = null;
            using (MemoryStream stream = new MemoryStream(resultingBytes))
            {
                using (PdfFileSource fileSource = new PdfFileSource(stream))
                {
                    using (PdfStreamWriter fileWriter = new PdfStreamWriter(ms, true))
                    {
                        foreach (PdfPageSource pageSource in fileSource.Pages)
                        {
                            using (PdfPageStreamWriter pageWriter = fileWriter.BeginPage(pageSource.Size, pageSource.Rotation))
                            {
                                pageWriter.WriteContent(pageSource);

                                using (pageWriter.SaveContentPosition())
                                {
                                    double xCenteringTranslation = (pageSource.Size.Width - foregroundContentOwner.Size.Width) - (-260);
                                    double yCenteringTranslation = (pageSource.Size.Height - foregroundContentOwner.Size.Height) - (-15);
                                    pageWriter.ContentPosition.Translate(xCenteringTranslation, yCenteringTranslation);
                                    pageWriter.ContentPosition.Scale(0.2, 0.2);
                                    pageWriter.WriteContent(foregroundContentOwner);
                                }
                            }
                        }
                    }
                }
            }
            renderedBytes = ms.ToArray();
            return renderedBytes;
        }


        public ActionResult Async_Save(IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var physicalPath = Path.Combine(Server.MapPath("~/Content/web/signature"), fileName);

                    file.SaveAs(physicalPath);
                }
                //var pdfName = files.Select(x => x.FileName).FirstOrDefault();
                //return RedirectToAction("Sign_Pdf_Document", new { filename = pdfName });
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
                    var physicalPath = Path.Combine(Server.MapPath("~/Content/UserFiles/Folders"), fileName);

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
    }
}
