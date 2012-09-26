using System.Linq;
using Raven.Client;

namespace SpiderRT
{
	public class IndexerController
	{
		private readonly IDocumentStore _documentStore;
		private readonly IIngester _ingester;
		private readonly IVcsManagerFactory _vcsManagerFactory;

		public IndexerController(IDocumentStore documentStore, IIngester ingester, IVcsManagerFactory vcsManagerFactory)
		{
			_documentStore = documentStore;
			_ingester = ingester;
			_vcsManagerFactory = vcsManagerFactory;
		}

		public void RunIndexer()
		{
			Settings settings;
			VcsRoot[] vcsRoots;

			using(var session = _documentStore.OpenSession())
			{
				settings = session.Query<Settings>().Single();
				vcsRoots = session.Query<VcsRoot>().ToArray();
			}

			vcsRoots
				.Select(vcsRoot => _vcsManagerFactory.CreateVcsManagerFrom(settings, vcsRoot))
				.ForEach(x => x.CreateOrUpdate());

			_ingester.Ingest(settings, vcsRoots);
		}
	}
}