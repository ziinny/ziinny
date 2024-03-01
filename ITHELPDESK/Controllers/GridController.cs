using System;
﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ITHELPDESK.Models;
using System.Configuration;
using System.Threading.Tasks;

namespace ITHELPDESK.Controllers
{
    [Authorize]
	public partial class GridController : Controller
    {
		public ActionResult Orders_Read([DataSourceRequest]DataSourceRequest request)
		{
			var result = Enumerable.Range(0, 50).Select(i => new OrderViewModel
			{
				OrderID = i,
				Freight = i * 10,
				OrderDate = DateTime.Now.AddDays(i),
				ShipName = "ShipName " + i,
				ShipCity = "ShipCity " + i
			});

			return Json(result.ToDataSourceResult(request));
		}

		public ActionResult Link_Installer()
		{
			var model = Read();

			return View(model);
		}

		public IEnumerable<MyLink> Read()
		{
			return GetAll();
		}


		public IList<MyLink> GetAll()
        {
			string[] selectFromPCFS_Folder = ConfigurationManager.AppSettings.AllKeys.Where(key => key.StartsWith("PCFS")).Select(key => ConfigurationManager.AppSettings[key]).ToArray();
			string[] selectFromSETUP_Folder = ConfigurationManager.AppSettings.AllKeys.Where(key => key.StartsWith("SETUP")).Select(key => ConfigurationManager.AppSettings[key]).ToArray();

			List<MyLink> items = new List<MyLink>();

			Parallel.ForEach(selectFromPCFS_Folder, (string item) =>
			{
				items.Add(new MyLink { ProgramName = item.Split('/')[5].ToString(), URLName = item });
			});

			Parallel.ForEach(selectFromSETUP_Folder, (string item) =>
			{
				items.Add(new MyLink { ProgramName = item.Split('/')[4].ToString(), URLName = item });
			});

			return items.OrderBy(sort => sort.ProgramName).ToList();
		}

		


		[HttpPost]
		public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
		{
			var fileContents = Convert.FromBase64String(base64);

			return File(fileContents, contentType, fileName);
		}


		public class MyLink
		{
			public string ProgramName { get; set; }
			public string URLName { get; set; }
		}
	}
}
