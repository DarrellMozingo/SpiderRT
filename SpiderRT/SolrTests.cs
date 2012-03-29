﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using SolrNet;
using SolrNet.Attributes;

namespace SpiderRT
{
	public class SolrTests
	{
		private ISolrOperations<CodeFile> _solrInstance;
		private IDocumentStore _documentStore;

		public class CodeFile
		{
			[SolrUniqueKey("id")]
			public Guid Id { get; set; }

			[SolrField("filename")]
			public string Filename { get; set; }

			[SolrField("fullPath")]
			public string FullPath { get; set; }

			[SolrField("content")]
			public string Content { get; set; }
		}

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

		[Test, Explicit]
		public void Import()
		{
			saveToDb();
			saveToSolr();
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

								 if (exists == false)
								 {
									 Console.WriteLine("Adding to DB: {0}", codeFile.FullPath);
									 session.Store(codeFile);
								 }
							 });

				session.SaveChanges();
			}
		}

		private static IEnumerable<CodeFile> getFiles()
		{
			return Directory.EnumerateFiles(@"C:\work\SpiderRT", "*.*", SearchOption.AllDirectories)
				.Where(fileIsNotBlackListed)
				.Select(filename => new FileInfo(filename))
				.Select(fileInfo => new CodeFile
				{
					Id = Guid.NewGuid(),
					FullPath = fileInfo.FullName,
					Content = File.ReadAllText(fileInfo.FullName),
					Filename = fileInfo.Name
				});
		}

		private static bool fileIsNotBlackListed(string filename)
		{
			filename = filename.ToLower();

			var extensionBlackList = new[] { ".sln", ".gitignore", ".user", ".suo", ".csproj", ".chm" };
			var pathBlackList = new[] { ".git", "packages", "bin", "obj", "_resharper" };

			var blackListedByExtention = extensionBlackList.Any(filename.EndsWith);
			var blackListedByPath = pathBlackList.Any(path => filename.Contains("\\" + path));

			return (blackListedByExtention || blackListedByPath) == false;
		}

		private void saveToSolr()
		{
			using(var session = _documentStore.OpenSession())
			{
				session.Query<CodeFile>()
					.Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(30)))
					.ForEach(codeFile =>
					         {
					         	Console.WriteLine("Adding/updating in Solr: {0}", codeFile.FullPath);
					         	_solrInstance.Add(codeFile);
					         });

				_solrInstance.Commit();
			}
		}
	}
}