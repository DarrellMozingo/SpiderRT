using System;
using System.Collections.Generic;
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

		[Test]
		public void Should_ingest_two_files_in_a_single_folder()
		{
			const string repositoryName = "repo1";
			const string codeFileName1 = "test1.txt";
			const string codeFileName2 = "test2.txt";
			const string codeFileContents1 = "test-contents-1";
			const string codeFileContents2 = "test-contents-2";

			var codeFilePath1 = Path.Combine(_tempWorkingFolder, repositoryName, codeFileName1);
			var codeFilePath2 = Path.Combine(_tempWorkingFolder, repositoryName, codeFileName2);

			createRepositoryFolder(repositoryName);
			File.WriteAllText(codeFilePath1, codeFileContents1);
			File.WriteAllText(codeFilePath2, codeFileContents2);

			_ingester.Ingest();

			CodeFile[] ingestedCodeFiles;

			using (var session = _documentStore.OpenSession())
			{
				ingestedCodeFiles = session.Query<CodeFile>().ToArray();
			}

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(2));

			Assert.That(ingestedCodeFiles[0].Filename, Is.EqualTo(codeFileName1));
			Assert.That(ingestedCodeFiles[0].FullPath, Is.EqualTo(codeFilePath1));
			Assert.That(ingestedCodeFiles[0].Content, Is.EqualTo(codeFileContents1));

			Assert.That(ingestedCodeFiles[1].Filename, Is.EqualTo(codeFileName2));
			Assert.That(ingestedCodeFiles[1].FullPath, Is.EqualTo(codeFilePath2));
			Assert.That(ingestedCodeFiles[1].Content, Is.EqualTo(codeFileContents2));
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