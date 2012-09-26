using System.Collections.Generic;
using FakeItEasy;
using NUnit.Framework;

namespace SpiderRT.UnitTests
{
	public class VcsManagerTests
	{
		[Test]
		public void Should_update_all_vcs_roots()
		{
			var vcs1 = A.Fake<IVcs>();
			var vcs2 = A.Fake<IVcs>();

			var vcsManager = new VcsManager(new[] { vcs1, vcs2 });

			vcsManager.UpdateAll();

			A.CallTo(() => vcs1.CreateOrUpdate()).MustHaveHappened();
			A.CallTo(() => vcs2.CreateOrUpdate()).MustHaveHappened();
		}
	}

	public class VcsManager
	{
		private readonly IEnumerable<IVcs> _vcsRoots;

		public VcsManager(IEnumerable<IVcs> vcsRoots)
		{
			_vcsRoots = vcsRoots;
		}

		public void UpdateAll()
		{
			_vcsRoots.ForEach(x => x.CreateOrUpdate());
		}
	}
}