using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace SpiderRT.SlowTests
{
	public class IndexTaskTests
	{
		private IDocumentStore _documentStore;

		[SetUp]
		public void Before_each_test()
		{
			_documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize();
		}

		[Test]
		public void Should_pass_settings_and_vcs_roots_on_to_the_ingester()
		{
			var fakeIngester = A.Fake<IIngester>();

			var settings = new Settings { Id = Guid.NewGuid() };
			var vcsRoots = new[] { new VcsRoot { Id = Guid.NewGuid() } };

			using(var session = _documentStore.OpenSession())
			{
				session.Store(vcsRoots[0]);
				session.Store(settings);

				session.SaveChanges();
			}

			var indexController = new IndexTask(_documentStore, fakeIngester, A.Fake<IVcsManagerFactory>());

			indexController.Run();

			A.CallTo(() => fakeIngester.Ingest(A<Settings>.That.Matches(x => x.Id == settings.Id),
			                                   A<IEnumerable<VcsRoot>>.That.Matches(x => x.Single().Id == vcsRoots[0].Id))).MustHaveHappened();
		}

		[Test]
		public void Should_get_vcs_managers_for_each_stored_vcs_root_and_update_the_managers()
		{
			var fakeVcsManagerFactory = A.Fake<IVcsManagerFactory>();

			var settings = new Settings { Id = Guid.NewGuid() };
			var vcsRoots = new[]
			{
				new VcsRoot { Id = Guid.NewGuid() },
				new VcsRoot { Id = Guid.NewGuid() }
			};

			using(var session = _documentStore.OpenSession())
			{
				session.Store(vcsRoots[0]);
				session.Store(vcsRoots[1]);
				session.Store(settings);

				session.SaveChanges();
			}

			var indexController = new IndexTask(_documentStore, A.Fake<IIngester>(), fakeVcsManagerFactory);

			indexController.Run();

			A.CallTo(() => fakeVcsManagerFactory.CreateVcsManagerFrom(A<Settings>.That.Matches(x => x.Id == settings.Id),
			                                                          A<VcsRoot>.That.Matches(x => x.Id == vcsRoots[0].Id))).MustHaveHappened();

			A.CallTo(() => fakeVcsManagerFactory.CreateVcsManagerFrom(A<Settings>.That.Matches(x => x.Id == settings.Id),
			                                                          A<VcsRoot>.That.Matches(x => x.Id == vcsRoots[1].Id))).MustHaveHappened();
		}

		[Test]
		public void Should_update_the_managers_for_each_vcs_root()
		{
			var fakeVcsManagerFactory = A.Fake<IVcsManagerFactory>();
			var fakeVcsManager = A.Fake<IVcsManager>();

			var settings = new Settings();
			var vcsRoots = new[]
			{
				new VcsRoot(),
				new VcsRoot()
			};

			using (var session = _documentStore.OpenSession())
			{
				session.Store(vcsRoots[0]);
				session.Store(vcsRoots[1]);
				session.Store(settings);

				session.SaveChanges();
			}

			A.CallTo(() => fakeVcsManagerFactory.CreateVcsManagerFrom(null, null)).WithAnyArguments().Returns(fakeVcsManager);

			var indexController = new IndexTask(_documentStore, A.Fake<IIngester>(), fakeVcsManagerFactory);

			indexController.Run();

			A.CallTo(() => fakeVcsManager.CreateOrUpdate()).MustHaveHappened(Repeated.Exactly.Twice);
		}
	}
}