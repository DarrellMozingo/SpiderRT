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
		public void Should_ingest_a_single_new_file_in_a_single_folder()
		{
			var codeFilePath = createFileInRepository("repo", "test.txt", "test-contents");

			_ingester.Ingest();

			var ingestedCodeFile = savedCodeFiles().FirstOrDefault();

			Assert.That(ingestedCodeFile, Is.Not.Null, "No file was ingested.");
			asssertCodeFileIsCorrect(ingestedCodeFile, codeFilePath, "test-contents");
		}

		[Test]
		public void Should_ingest_two_new_files_in_a_single_folder()
		{
			var codeFilePath1 = createFileInRepository("repo", "test1.txt", "test-contents-1");
			var codeFilePath2 = createFileInRepository("repo", "test2.txt", "test-contents-2");

			_ingester.Ingest();

			var ingestedCodeFiles = savedCodeFiles();

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(2));
			asssertCodeFileIsCorrect(ingestedCodeFiles[0], codeFilePath1, "test-contents-1");
			asssertCodeFileIsCorrect(ingestedCodeFiles[1], codeFilePath2, "test-contents-2");
		}

		[Test]
		public void Should_ingest_two_new_files_in_two_folders()
		{
			var codeFilePath1 = createFileInRepository("repo1", "test1.txt", "test-contents-1");
			var codeFilePath2 = createFileInRepository("repo2", "test2.txt", "test-contents-2");

			_ingester.Ingest();

			var ingestedCodeFiles = savedCodeFiles();

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(2));
			asssertCodeFileIsCorrect(ingestedCodeFiles[0], codeFilePath1, "test-contents-1");
			asssertCodeFileIsCorrect(ingestedCodeFiles[1], codeFilePath2, "test-contents-2");
		}

		[Test]
		public void Should_ingest_four_new_files_in_two_folders()
		{
			var codeFilePath1 = createFileInRepository("repo1", "test1.txt", "test-contents-1");
			var codeFilePath2 = createFileInRepository("repo1", "test2.txt", "test-contents-2");
			var codeFilePath3 = createFileInRepository("repo2", "test3.txt", "test-contents-3");
			var codeFilePath4 = createFileInRepository("repo2", "test4.txt", "test-contents-4");

			_ingester.Ingest();

			var ingestedCodeFiles = savedCodeFiles();

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(4));
			asssertCodeFileIsCorrect(ingestedCodeFiles[0], codeFilePath1, "test-contents-1");
			asssertCodeFileIsCorrect(ingestedCodeFiles[1], codeFilePath2, "test-contents-2");
			asssertCodeFileIsCorrect(ingestedCodeFiles[2], codeFilePath3, "test-contents-3");
			asssertCodeFileIsCorrect(ingestedCodeFiles[3], codeFilePath4, "test-contents-4");
		}

		private string createFileInRepository(string repositoryName, string filename, string fileContents)
		{
			var repositoryPath = Path.Combine(_tempWorkingFolder, repositoryName);
			var codeFilePath = Path.Combine(repositoryPath, filename);

			Directory.CreateDirectory(repositoryPath);
			File.WriteAllText(codeFilePath, fileContents);

			return codeFilePath;
		}

		private CodeFile[] savedCodeFiles()
		{
			using (var session = _documentStore.OpenSession())
			{
				return session.Query<CodeFile>().ToArray();
			}
		}

		private static void asssertCodeFileIsCorrect(CodeFile codeFile, string expectedPath, string expectedContents)
		{
			Assert.That(codeFile.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(codeFile.Filename, Is.EqualTo(Path.GetFileName(expectedPath)));
			Assert.That(codeFile.FullPath, Is.EqualTo(expectedPath));
			Assert.That(codeFile.Content, Is.EqualTo(expectedContents));
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