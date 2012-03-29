using System.Web.Mvc;
using SpiderRT.Web.Models;

namespace SpiderRT.Web.Controllers
{
	public class HomeController : Controller
	{
		 public ActionResult Index()
		 {
		 	return View();
		 }

		[HttpPost]
		public ActionResult Search(SearchViewModel viewModel)
		{
			var results = new[] { new SearchResultViewModel{Filename = "foo"} };

			return View("SearchResults", results);
		}
	}
}