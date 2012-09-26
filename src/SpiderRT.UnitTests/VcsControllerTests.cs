using FakeItEasy;
using NUnit.Framework;

namespace SpiderRT.UnitTests
{
	public class VcsControllerTests
	{
		[Test]
		public void Should_update_all_vcs_managers()
		{
			var vcsManager1 = A.Fake<IVcsManager>();
			var vcsManager2 = A.Fake<IVcsManager>();

			var vcsManager = new VcsController(new[] { vcsManager1, vcsManager2 });

			vcsManager.UpdateAll();

			A.CallTo(() => vcsManager1.CreateOrUpdate()).MustHaveHappened();
			A.CallTo(() => vcsManager2.CreateOrUpdate()).MustHaveHappened();
		}
	}
}