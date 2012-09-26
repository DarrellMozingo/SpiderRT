using System.Diagnostics;
using System.IO;

namespace SpiderRT
{
	public class GitManager : IVcsManager
	{
		private readonly string _workingFolder;
		private readonly string _gitExePath;
		private readonly string _url;
		private readonly string _repoName;

		private string _localPath
		{
			get { return Path.Combine(_workingFolder, _repoName); }
		}

		public GitManager(string workingFolder, string gitExePath, string url, string repoName)
		{
			_workingFolder = workingFolder;
			_gitExePath = gitExePath;
			_url = url;
			_repoName = repoName;
		}

		public void CreateOrUpdate()
		{
			if (repoIsAlreadyCloned())
			{
				update();
			}
			else
			{
				create();
			}
		}

		private bool repoIsAlreadyCloned()
		{
			return Directory.Exists(_localPath);
		}

		private void update()
		{
			execute("pull origin master", _localPath);
		}

		private void create()
		{
			var gitCloneCommand = string.Format("clone {0} {1}", _url, _repoName);

			execute(gitCloneCommand, _workingFolder);
		}

		private void execute(string gitCommand, string path)
		{
			var gitInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				FileName = _gitExePath,
				Arguments = gitCommand,
				WorkingDirectory = path
			};

			using(var process = new Process { StartInfo = gitInfo })
			{
				process.Start();
				process.WaitForExit();
			}
		}
	}
}