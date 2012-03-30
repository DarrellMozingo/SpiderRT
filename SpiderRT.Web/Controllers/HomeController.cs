using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
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
			var solrInstance = ServiceLocator.Current.GetInstance<ISolrOperations<CodeFile>>();

			var solrResults = solrInstance.Query(new SolrQueryByField("content", viewModel.SearchText));

			Mapper.CreateMap<CodeFile, SearchResultViewModel>();
			var resultViewModels = Mapper.Map<IEnumerable<CodeFile>, IEnumerable<SearchResultViewModel>>(solrResults)
				.GroupBy(x => x.VcsName);

			ViewBag.SearchText = viewModel.SearchText;
			return View("SearchResults", resultViewModels);
		}
	}
}