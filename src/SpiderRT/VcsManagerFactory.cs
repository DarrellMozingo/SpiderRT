namespace SpiderRT
{
	public interface IVcsManagerFactory
	{
		IVcsManager CreateVcsManagerFrom(Settings settings, VcsRoot vcsRoot);
	}

	public class VcsManagerFactory : IVcsManagerFactory
	{
		public IVcsManager CreateVcsManagerFrom(Settings settings, VcsRoot vcsRoot)
		{
			if (vcsRoot.Type == Vcs.Git)
			{
				return new GitManager(settings.WorkingFolder, settings.GitPath, vcsRoot.Url, vcsRoot.Name);
			}

			return null;
		}
	}
}