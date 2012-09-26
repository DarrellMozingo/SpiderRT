namespace SpiderRT
{
	public class VcsRoot : Entity
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public Vcs Type { get; set; }
	}
}