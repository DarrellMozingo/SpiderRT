using System;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using SolrNet;
using SolrNet.Attributes;

namespace SpiderRT
{
	public class Class1
	{
		private static readonly Guid _codeFileId = Guid.NewGuid();

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
		public void Add()
		{
			var file = new CodeFile {Id = _codeFileId, Content = "public class Foo { }", Filename = "Program.cs", FullPath = @"src\MyProgram\Program.cs"};

			var solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeFile>>();
			solr.Add(file);
			solr.Commit();
		}

		[Test]
		public void Query()
		{
			var solr = ServiceLocator.Current.GetInstance<ISolrOperations<CodeFile>>();
			var results = solr.Query(new SolrQueryByField("id", _codeFileId.ToString()));
			Assert.AreEqual(1, results.Count);
			Console.WriteLine(results[0].Filename);
		}
	}
}