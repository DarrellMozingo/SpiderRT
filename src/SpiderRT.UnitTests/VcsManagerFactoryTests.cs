using NUnit.Framework;

namespace SpiderRT.UnitTests
{
	public class VcsManagerFactoryTests
	{
		[Test]
 		public void Should_create_a_vcs_manager_from_the_given_information()
		{
			var settings = new Settings();
			var vcsRoot = new VcsRoot { Type = Vcs.Git };

			var vcsManager = new VcsManagerFactory().CreateVcsManagerFrom(settings, vcsRoot);

			Assert.That(vcsManager, Is.InstanceOf<GitManager>());
		}
	}
}