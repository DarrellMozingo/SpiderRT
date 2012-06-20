using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Raven.Client;

namespace SpiderRT
{
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

		private bool fileIsNotBlackListed(FileSystemInfo fileInfo)
		{
			var filePath = fileInfo.FullName.ToLower();

			var extensionIsBlackListed = _settings.BlockedExtensions.Any(extension => extension.ToLower() == Path.GetExtension(filePath));
			var pathIsBlackListed = _settings.BlockedPaths.Any(blockedPath => Regex.IsMatch(filePath, string.Format(@"\\{0}\\", blockedPath.ToLower())));

			return (extensionIsBlackListed || pathIsBlackListed) == false;
		}
	}
}