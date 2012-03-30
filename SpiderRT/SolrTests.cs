using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using SolrNet;

namespace SpiderRT
{
	public class SolrTests
	{
		private ISolrOperations<CodeFile> _solrInstance;
		private IDocumentStore _documentStore;

		private readonly IEnumerable<VcsInfo> _vcsRoots = new[]
		{
			new VcsInfo { Name = "test", Url = @"C:\work\test" },
			new VcsInfo { Name = "SpiderRT", Url = @"C:\work\SpiderRT" }
		};

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			Startup.Init<CodeFile>("http://darrellmlnx:8983/solr");
			_solrInstance = ServiceLocator.Current.GetInstance<ISolrOperations<CodeFile>>();

			_documentStore = new DocumentStore { Url = "http://localhost:8080" }.Initialize();
		}

		[Test]
		public void QuerySolr()
		{
			_solrInstance.Query(new SolrQueryByField("content", "namespace"))
				.ForEach(codeFile => displayCodeFile(codeFile, "Solr"));
		}

		[Test]
		public void QueryDb()
		{
			using(var session = _documentStore.OpenSession())
			{
				session.Query<CodeFile>()
					.ForEach(codeFile => displayCodeFile(codeFile, "Database"));
			}
		}

		private static void displayCodeFile(CodeFile codeFile, string source)
		{
			Console.WriteLine("{0}: ({1}) - {2}", source, codeFile.Id, codeFile.Filename);
		}

		[Test]
		public void Import()
		{
			updateVcsRoots();
			saveToDb();
			saveToSolr();
		}

		private void updateVcsRoots()
		{
			_vcsRoots.ForEach(vcsRoot => vcsRoot.CreateOrUpdateIn());
		}

		private void saveToDb()
		{
			using(var session = _documentStore.OpenSession())
			{
				var savedFiles = session.Query<CodeFile>().ToList();

				getFiles()
					.ForEach(codeFile =>
					         {
					         	var exists = savedFiles.Any(x => x.FullPath == codeFile.FullPath);

					         	if(exists == false)
					         	{
					         		Console.WriteLine("Adding to DB: {0}", codeFile.FullPath);
					         		session.Store(codeFile);
					         	}
					         });

				session.SaveChanges();
			}
		}

		private IEnumerable<CodeFile> getFiles()
		{
			return _vcsRoots
				.SelectMany(vcsRoot => Directory.EnumerateFiles(vcsRoot.LocalPath, "*.*", SearchOption.AllDirectories)
				                       	.Select(filePath => new FileInfo(filePath))
				                       	.Where(fileIsNotBlackListed)
				                       	.Select(indexedFile => new CodeFile
				                       	{
				                       		Id = Guid.NewGuid(),
				                       		FullPath = indexedFile.FullName,
				                       		Content = File.ReadAllText(indexedFile.FullName),
				                       		Filename = indexedFile.Name,
				                       		VcsName = vcsRoot.Name,
				                       		VcsUrl = vcsRoot.Url
				                       	}));
		}

		private static bool fileIsNotBlackListed(FileInfo fileInfo)
		{
			var filePath = fileInfo.FullName.ToLower();

			var extensionBlackList = new[] { ".sln", ".gitignore", ".user", ".suo", ".csproj", ".chm" };
			var pathBlackList = new[] { ".git", "packages", "bin", "obj", "_resharper.*" };

			var blackListedByExtention = extensionBlackList.Any(filePath.EndsWith);
			var blackListedByPath = pathBlackList.Any(blackListPath => Regex.IsMatch(filePath, string.Format(@"\\{0}\\", blackListPath)));

			var fileIsBlackListed = (blackListedByExtention || blackListedByPath);

			if(fileIsBlackListed)
			{
				Console.WriteLine("BLACKLISTED: {0}", filePath);
			}

			return fileIsBlackListed == false;
		}

		private void saveToSolr()
		{
			using(var session = _documentStore.OpenSession())
			{
				var documentsToAdd = session.Query<CodeFile>().Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(30))).ToList();

				documentsToAdd.ForEach(codeFile => Console.WriteLine("Adding/updating in Solr: {0}", codeFile.FullPath));
				_solrInstance.Add(documentsToAdd);
			}

			_solrInstance.Commit();
			_solrInstance.Optimize();
		}
	}
}