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
			var viewModels = Mapper.Map<IEnumerable<CodeFile>, IEnumerable<SearchResultViewModel>>(solrResults)
				.GroupBy(x => x.VcsName);

			ViewBag.SearchText = viewModel.SearchText;
			return View("SearchResults", viewModels);
		}

		[HttpGet]
		public ActionResult Settings()
		{
			Settings settings;

			using(var session = MvcApplication.DocumentStore.OpenSession())
			{
				settings = session.Query<Settings>().FirstOrDefault() ?? new Settings();
			}

			Mapper.CreateMap<Settings, SettingsViewModel>();
			var viewModel = Mapper.Map<Settings, SettingsViewModel>(settings);

			return View(viewModel);
		}

		[HttpPost]
		public ActionResult Settings(SettingsViewModel viewModel)
		{
			using (var session = MvcApplication.DocumentStore.OpenSession())
			{
				var settings = session.Query<Settings>().FirstOrDefault() ?? new Settings();

				settings.GitPath = viewModel.GitPath;
				settings.IndexServer = viewModel.IndexServer;

				session.Store(settings);

				session.SaveChanges();
			}

			return RedirectToAction("Index");
		}
	}
}