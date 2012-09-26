using System.Linq;
using Raven.Client;
using Taskie;

namespace SpiderRT
{
	public class IndexTask : ITask
	{
		private readonly IDocumentStore _documentStore;
		private readonly IIngester _ingester;
		private readonly IVcsManagerFactory _vcsManagerFactory;

		public IndexTask(IDocumentStore documentStore, IIngester ingester, IVcsManagerFactory vcsManagerFactory)
		{
			_documentStore = documentStore;
			_ingester = ingester;
			_vcsManagerFactory = vcsManagerFactory;
		}

		public void Run()
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