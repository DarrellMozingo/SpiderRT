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
		public void Should_ingest_a_single_new_file_in_a_single_folder()
		{
			var codeFilePath = createFileInRepository("repo", "test.txt", "test-contents");

			_ingester.Ingest();

			var ingestedCodeFile = savedCodeFiles().SingleOrDefault();

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

		[Test]
		public void Should_reingest_a_single_existing_file_in_a_single_folder_by_matching_on_its_repository_and_file_path()
		{
			var codeFilePath = createFileInRepository("repo", "test.txt", "test-contents-new");
			var existingCodeFile = createCodeFileInDatabase(codeFilePath, "test-contents-old");

			_ingester.Ingest();

			var ingestedCodeFile = savedCodeFiles().SingleOrDefault();

			Assert.That(ingestedCodeFile, Is.Not.Null, "No file was ingested.");
			Assert.That(ingestedCodeFile.Id, Is.EqualTo(existingCodeFile.Id));
			Assert.That(ingestedCodeFile.Filename, Is.EqualTo(existingCodeFile.Filename));
			Assert.That(ingestedCodeFile.FullPath, Is.EqualTo(existingCodeFile.FullPath));
			Assert.That(ingestedCodeFile.Content, Is.EqualTo("test-contents-new"));
		}

		[Test]
		public void Should_not_ingest_files_with_a_single_blocked_file_extension()
		{
			createFileInRepository("repo", "test.blockedExtension", "random-content-1");
			var allowedCodeFilePath = createFileInRepository("repo", "test.allowedExtension", "random-content-2");

			setBlockedExtensions(".blockedExtension");

			_ingester.Ingest();

			var ingestedCodeFiles = savedCodeFiles();

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(1), "Only the allowed extension should have been ingested");
			asssertCodeFileIsCorrect(ingestedCodeFiles[0], allowedCodeFilePath, "random-content-2");
		}

		[Test]
		public void Should_not_ingest_files_with_multiple_blocked_file_extensions()
		{
			createFileInRepository("repo", "test1.blockedExtension1", "random-content-1");
			createFileInRepository("repo", "test2.blockedExtension2", "random-content-2");
			var allowedCodeFilePath = createFileInRepository("repo", "test.allowedExtension", "random-content-3");

			setBlockedExtensions(".blockedExtension1", ".blockedExtension2");

			_ingester.Ingest();

			var ingestedCodeFiles = savedCodeFiles();

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(1), "Only the allowed extensions should have been ingested");
			asssertCodeFileIsCorrect(ingestedCodeFiles[0], allowedCodeFilePath, "random-content-3");
		}

		[Test]
		public void Should_not_ingest_files_with_a_single_blocked_path()
		{
			createFileInRepository("blocked-path", "test.txt", "random-content-1");
			var allowedCodeFilePath = createFileInRepository("allowed-path", "test2.txt", "random-content-2");

			setBlockedPaths("blocked-path");

			_ingester.Ingest();

			var ingestedCodeFiles = savedCodeFiles();

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(1), "Only the allowed path should have been ingested");
			asssertCodeFileIsCorrect(ingestedCodeFiles[0], allowedCodeFilePath, "random-content-2");
		}

		[Test]
		public void Should_not_ingest_files_with_multiple_blocked_paths()
		{
			createFileInRepository("blocked-path-1", "test1.txt", "random-content-1");
			createFileInRepository("blocked-path-2", "test2.txt", "random-content-2");
			var allowedCodeFilePath = createFileInRepository("allowed-path", "test3.txt", "random-content-3");

			setBlockedPaths("blocked-path-1", "blocked-path-2");

			_ingester.Ingest();

			var ingestedCodeFiles = savedCodeFiles();

			Assert.That(ingestedCodeFiles.Length, Is.EqualTo(1), "Only the allowed path should have been ingested");
			asssertCodeFileIsCorrect(ingestedCodeFiles[0], allowedCodeFilePath, "random-content-3");
		}

		private void setBlockedPaths(params string[] blockedPath)
		{
			using (var session = _documentStore.OpenSession())
			{
				var settings = session.Query<Settings>().Single();

				settings.BlockedPaths = new List<string>(blockedPath);
				session.SaveChanges();
			}
		}

		private void setBlockedExtensions(params string[] blockedExtensions)
		{
			using (var session = _documentStore.OpenSession())
			{
				var settings = session.Query<Settings>().Single();

				settings.BlockedExtensions = new List<string>(blockedExtensions);
				session.SaveChanges();
			}
		}

		private CodeFile createCodeFileInDatabase(string codeFilePath, string fileContents)
		{
			var newCodeFile = new CodeFile
			{
				Filename = Path.GetFileName(codeFilePath),
				FullPath = codeFilePath,
				Content = fileContents
			};

			using(var session = _documentStore.OpenSession())
			{
				session.Store(newCodeFile);
				session.SaveChanges();
			}

			return newCodeFile;
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
			using(var session = _documentStore.OpenSession())
			{
				return session.Query<CodeFile>()
					.Customize(x => x.WaitForNonStaleResults())
					.ToArray();
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
		private Settings _settings;

		public Ingester(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public void Ingest()
		{
			using(var session = _documentStore.OpenSession())
			{
				_settings = session.Query<Settings>().Single();
				var existingCodeFiles = session.Query<CodeFile>().ToList();

				getFilesToIngest()
					.ForEach(codeFile =>
					         {
					         	var existingCodeFile = existingCodeFiles.SingleOrDefault(x => x.FullPath == codeFile.FullPath);

					         	if(existingCodeFile != null)
					         	{
					         		existingCodeFile.Content = codeFile.Content;
					         	}
					         	else
					         	{
					         		session.Store(codeFile);
					         	}
					         });

				session.SaveChanges();
			}
		}

		private IEnumerable<CodeFile> getFilesToIngest()
		{
			return Directory.EnumerateFiles(_settings.WorkingFolder, "*", SearchOption.AllDirectories)
				.Select(fullPath => new FileInfo(fullPath))
				.Where(fileIsNotBlackListed)
				.Select(file => new CodeFile
				{
					Id = Guid.NewGuid(),
					Content = File.ReadAllText(file.FullName),
					Filename = Path.GetFileName(file.Name),
					FullPath = file.FullName
				});
		}

		private bool fileIsNotBlackListed(FileSystemInfo fullPath)
		{
			var extensionIsBlackListed = _settings.BlockedExtensions.Any(ext => ext == Path.GetExtension(fullPath.Name));
			var pathIsBlackListed = _settings.BlockedPaths.Any(fullPath.FullName.Contains);

			return (extensionIsBlackListed || pathIsBlackListed) == false;
		}
	}
}