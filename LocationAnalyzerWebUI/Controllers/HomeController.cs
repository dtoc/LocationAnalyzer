using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parser;

namespace LocationAnalyzerWebUI.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Home()
        {
            ParserAgent parser = new ParserAgent();
            parser.Parse();
            ViewBag.Duplicates = parser.duplicates;
            return View();
        }
    }
}