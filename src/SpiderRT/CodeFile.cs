namespace SpiderRT
{
	public class CodeFile : Entity
	{
		public string Filename { get; set; }
		public string FullPath { get; set; }
		public string Content { get; set; }
		public VcsRoot VcsRoot { get; set; }
	}
}