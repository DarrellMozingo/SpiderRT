using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;

namespace SpiderRT
{
	public class Tests
	{
		private IDocumentStore _documentStore;
		private Settings _settings;
		private IEnumerable<VcsInfo> _vcsRoots;

		private static readonly string[] _extensionBlackList = new[] { ".sln", ".gitignore", ".user", ".suo", ".csproj", ".chm", ".dll", ".exe" };
		private static readonly string[] _pathBlackList = new[] { ".git", "packages", "bin", "obj", "_resharper.*" };

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_documentStore = new DocumentStore { ConnectionStringName = "Raven" }.Initialize();

			loadSettings();

			_vcsRoots = new[]
			{
				new VcsInfo(_settings) { Name = "test", Url = @"C:\work\test" },
			};
		}

		private void loadSettings()
		{
			using(var session = _documentStore.OpenSession())
			{
				_settings = session.Query<Settings>().FirstOrDefault();
			}

			if(_settings == null)
			{
				throw new Exception("No settings document found. Fire up the web site and fill in the settings details first.");
			}
		}

		[Test]
		public void Import()
		{
			updateVcsRoots();
			saveToDb();
		}

		private void updateVcsRoots()
		{
			_vcsRoots.ForEach(vcsRoot => vcsRoot.CreateOrUpdate());
		}

		private void saveToDb()
		{
			using(var session = _documentStore.OpenSession())
			{
				var savedFiles = session.Query<CodeFile>().ToList();

				getFiles()
					.ForEach(codeFile =>
					         {
					         	var existingFile = savedFiles.SingleOrDefault(x => x.FullPath == codeFile.FullPath);

					         	if(existingFile == null)
					         	{
					         		Console.WriteLine("Adding to DB: {0}", codeFile.FullPath);
					         		session.Store(codeFile);
					         	}
					         	else
					         	{
					         		Console.WriteLine("Updating in DB: {0}", codeFile.FullPath);
					         		existingFile.Content = codeFile.Content;
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

			var blackListedByExtention = _extensionBlackList.Any(filePath.EndsWith);
			var blackListedByPath = _pathBlackList.Any(blackListPath => Regex.IsMatch(filePath, string.Format(@"\\{0}\\", blackListPath)));

			return (blackListedByExtention || blackListedByPath) == false;
		}
	}
}