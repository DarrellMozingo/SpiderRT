using System.Collections.Generic;

namespace SpiderRT
{
	public class Settings : Entity
	{
		public string WorkingFolder { get; set; }
		public string GitPath { get; set; }
		public IList<string> BlockedExtensions { get; set; }
		public IList<string> BlockedPaths { get; set; }

		public Settings()
		{
			this.BlockedExtensions = new List<string>();
			this.BlockedPaths = new List<string>();
		}
	}
}