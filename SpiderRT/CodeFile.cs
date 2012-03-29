using System;
using SolrNet.Attributes;

namespace SpiderRT
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
}