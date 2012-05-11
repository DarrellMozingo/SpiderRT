using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using AutoMapper;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
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
			IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), MvcApplication.DocumentStore);

			if (string.IsNullOrEmpty(viewModel.SearchText))
			{
				return RedirectToAction("Index");
			}

			IEnumerable<CodeFile> searchResults;
			using (var session = MvcApplication.DocumentStore.OpenSession())
			{
				searchResults = session.Query<CodeFile, CodeFile_ByContent>().Search(x => x.Content, viewModel.SearchText).ToList();
			}

			Mapper.CreateMap<CodeFile, SearchResultViewModel>();
			var viewModels = Mapper.Map<IEnumerable<CodeFile>, IEnumerable<SearchResultViewModel>>(searchResults)
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

				settings.WorkingFolder = viewModel.WorkingFolder;
				settings.GitPath = viewModel.GitPath;

				session.Store(settings);

				session.SaveChanges();
			}

			return RedirectToAction("Index");
		}
	}

	public class CodeFile_ByContent : AbstractIndexCreationTask
	{
		public override IndexDefinition CreateIndexDefinition()
		{
			return new IndexDefinitionBuilder<CodeFile>
			{
				Map = codeFiles => from codeFile in codeFiles
								   select new { codeFile.Content },
				Indexes = { { x => x.Content, FieldIndexing.Analyzed } }
			}.ToIndexDefinition(new DocumentStore().Conventions);
		}
	}
}