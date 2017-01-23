using Microsoft.AspNetCore.Mvc;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Postcard.Controllers
{
    public class HomeController : Controller
    {
        private PostcardContext _context;
        private List<PlaceNode> placeNodes;
        public int PageSize = 50;

        public HomeController(PostcardContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            ParserAgent parser = new Parser.ParserAgent();
            placeNodes = _context.PlaceNodes.OrderByDescending(pn => pn.numberOfStatesThatHaveThisPlace).Skip((page - 1) * PageSize).Take(PageSize).ToList();
            parser.ProcessPlaceNodeStateDataForDisplay(placeNodes);
            return View(placeNodes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(bool runAsParallel)
        {
            if (ModelState.IsValid)
            {
                ParserAgent parser = new ParserAgent();
                _context.PlaceNodes.AddRange(await parser.Parse());
                _context.SaveChanges();
                
                return RedirectToAction("Index");
            }
            
            return View();
        }

        [HttpGet]
        public IActionResult Metrics()
        {
            ViewData["PlaceNodes"] = _context.PlaceNodes.Where(pn => pn.numberOfStatesThatHaveThisPlace > 10).ToList();
            return View();
        }

        [HttpGet]
        public IActionResult Destroy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DestroyAction()
        {
            try
            {
                _context.PlaceNodes.RemoveRange(_context.PlaceNodes);
                _context.SaveChanges();
            } catch (Exception ex)
            {
                Console.WriteLine("Derp!");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
    }
}
