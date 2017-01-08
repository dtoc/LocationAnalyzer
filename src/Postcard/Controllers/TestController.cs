using Microsoft.AspNetCore.Mvc;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Postcard.Controllers
{
    public class TestController : Controller
    {
        private PostcardContext _context;

        public TestController(PostcardContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ParserAgent parser = new Parser.ParserAgent();
            var placeNodes = _context.PlaceNodes.ToList();
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
                List<PlaceNode> nodesToAdd = new List<PlaceNode>();

                ParserAgent parser = new ParserAgent();
                var duplicates = await parser.Parse();

                /*foreach (var duplicate in duplicates)
                {
                    var placeNode = new PlaceNode
                    {
                        PlaceName = duplicate.PlaceName,
                        StateName = duplicate.StateName,
                        link = duplicate.link,
                        StatesThatHaveThisPlace = duplicate.StatesThatHaveThisPlace
                    };

                    nodesToAdd.Add(placeNode);
                }*/

                _context.PlaceNodes.AddRange(duplicates);
                _context.SaveChanges();
                
                return RedirectToAction("Index");
            }
            
            return View();
        }

        [HttpGet]
        public IActionResult Destroy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Destroyyy()
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
    }
}
