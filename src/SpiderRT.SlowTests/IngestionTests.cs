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

			_tempWorkingFolder = Path.GetFullPath(Guid.NewGuid().ToString());
			Directory.CreateDirectory(_tempWorkingFolder);

			using(var session = _documentStore.OpenSession())
			{
				session.Store(new Settings { WorkingFolder = _tempWorkingFolder });
				session.SaveChanges();
			}

			_ingester = new Ingester(_documentStore);
		}

		[TearDown]
		public void After_each_test()
		{
			Directory.Delete(_tempWorkingFolder, true);
		}

		[Test]
		public void Should_ingest_a_single_file_in_a_single_folder()
		{
			const string repositoryName = "repo1";
			const string codeFileName = "test.txt";
			const string codeFileContents = "test-contents";
			var codeFilePath = Path.Combine(_tempWorkingFolder, repositoryName, codeFileName);

			createRepositoryFolder(repositoryName);
			File.WriteAllText(codeFilePath, codeFileContents);

			_ingester.Ingest();

			CodeFile ingestedCodeFile;

			using(var session = _documentStore.OpenSession())
			{
				ingestedCodeFile = session.Query<CodeFile>().FirstOrDefault();
			}

			Assert.That(ingestedCodeFile, Is.Not.Null, "No file was ingested.");
			Assert.That(ingestedCodeFile.Filename, Is.EqualTo(codeFileName));
			Assert.That(ingestedCodeFile.FullPath, Is.EqualTo(codeFilePath));
			Assert.That(ingestedCodeFile.Content, Is.EqualTo(codeFileContents));
		}

		private void createRepositoryFolder(string repositoryName)
		{
			Directory.CreateDirectory(Path.Combine(_tempWorkingFolder, repositoryName));
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