using SolrNet.Attributes;

namespace SpiderRT
{
	public class CodeFile : Entity
	{
		[SolrField("filename")]
		public string Filename { get; set; }

		[SolrField("fullPath")]
		public string FullPath { get; set; }

		[SolrField("content")]
		public string Content { get; set; }

		[SolrField("vcsName")]
		public string VcsName { get; set; }

		[SolrField("vcsUrl")]
		public string VcsUrl { get; set; }
	}
}