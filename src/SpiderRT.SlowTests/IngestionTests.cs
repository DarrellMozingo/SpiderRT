using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;

namespace SpiderRT.SlowTests
{
	public class IngestionTests
	{
		private IDocumentStore _documentStore;
		private Ingester _ingester;
		private string _tempWorkingFolder;

		[SetUp]
		public void Before_each_test()
		{
			_documentStore = new EmbeddableDocumentStore { RunInMemory = true }.Initialize();

			_ingester = new Ingester(_documentStore);

			_tempWorkingFolder = Path.GetFullPath(Guid.NewGuid().ToString());
			Directory.CreateDirectory(_tempWorkingFolder);

			using(var session = _documentStore.OpenSession())
			{
				session.Store(new Settings { WorkingFolder = _tempWorkingFolder });

				session.SaveChanges();
			}
		}

		[TearDown]
		public void After_each_test()
		{
			Directory.Delete(_tempWorkingFolder, true);
		}

		[Test]
		public void Should_ingest_a_single_file_in_a_single_folder()
		{
			Directory.CreateDirectory(_tempWorkingFolder + "\\repo1");
			File.WriteAllText(_tempWorkingFolder + "\\repo1\\test.txt", "test-contents");

			_ingester.Ingest();

			CodeFile ingestedCodeFile;

			using(var session = _documentStore.OpenSession())
			{
				ingestedCodeFile = session.Query<CodeFile>().FirstOrDefault();
			}

			Assert.That(ingestedCodeFile, Is.Not.Null, "No file was ingested.");
			Assert.That(ingestedCodeFile.Filename, Is.EqualTo("test.txt"));
			Assert.That(ingestedCodeFile.FullPath, Is.EqualTo(_tempWorkingFolder + "\\repo1\\test.txt"));
			Assert.That(ingestedCodeFile.Content, Is.EqualTo("test-contents"));
		}
	}

	public class Ingester
	{
		private readonly IDocumentStore _documentStore;

		public Ingester(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public void Ingest()
		{
			using(var session = _documentStore.OpenSession())
			{
				var workingFolder = session.Query<Settings>().First().WorkingFolder;

				Directory.EnumerateFiles(workingFolder, "*", SearchOption.AllDirectories)
					.ForEach(filePath =>
					         session.Store(new CodeFile
					         {
					         	Content = File.ReadAllText(filePath),
					         	Filename = Path.GetFileName(filePath),
					         	FullPath = filePath
					         }));

				session.SaveChanges();
			}
		}
	}
}