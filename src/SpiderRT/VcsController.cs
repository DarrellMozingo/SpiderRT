using System.Collections.Generic;

namespace SpiderRT
{
	public class VcsController
	{
		private readonly IEnumerable<IVcsManager> _vcsManagers;

		public VcsController(IEnumerable<IVcsManager> vcsManagers)
		{
			_vcsManagers = vcsManagers;
		}

		public void UpdateAll()
		{
			_vcsManagers.ForEach(x => x.CreateOrUpdate());
		}
	}
}