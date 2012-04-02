using System;

namespace SpiderRT
{
	public class Settings
	{
		public Guid Id { get; set; }
		public string IndexServer { get; set; }
		public string GitPath { get; set; }

		public Settings()
		{
			Id = Guid.NewGuid();
		}
	}
}