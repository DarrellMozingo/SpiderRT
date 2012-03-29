using System;
using System.Linq;
using System.IO;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using SolrNet;
using SolrNet.Attributes;

namespace SpiderRT
{
	public class SolrTests
	{
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
		}

		[Test]
		public void Query()
		{
			var solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeFile>>();
			var results = solr.Query(new SolrQueryByField("content", "namespace"));

			foreach(var result in results)
			{
				Console.WriteLine("Match in: " + result.Filename);
			}
		}

		[Test, Explicit]
		public void Import()
		{
			var solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeFile>>();

			Directory.GetFiles(@"C:\work\SpiderRT", "*.cs", SearchOption.AllDirectories)
				.Select(filename => new FileInfo(filename))
				.Select(fileInfo => new CodeFile
				{
					Id = Guid.NewGuid(),
					FullPath = fileInfo.FullName,
					Content = File.ReadAllText(fileInfo.FullName),
					Filename = fileInfo.Name
				})
				.ForEach(codeFile =>
				         {
				         	Console.WriteLine("Adding file: {0}", codeFile.FullPath);
				         	solr.Add(codeFile);
				         });

			solr.Commit();
		}
	}
}