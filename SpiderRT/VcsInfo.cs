using System;
using System.Diagnostics;
using System.IO;

namespace SpiderRT
{
	public class VcsInfo
	{
		private Settings _settings;
		private const string WORKING_FOLDER = @"C:\spider-repos";

		public string Url { get; set; }
		public string Name { get; set; }

		public string LocalPath
		{
			get { return Path.Combine(WORKING_FOLDER, Name); }
		}

		public void CreateOrUpdate(Settings settings)
		{
			_settings = settings;

			if(exists())
			{
				update();
			}
			else
			{
				create();
			}
		}

		private bool exists()
		{
			return Directory.Exists(LocalPath);
		}

		private void update()
		{
			var gitPullCommand = string.Format("pull origin master");

			execute(gitPullCommand, LocalPath);
		}

		private void create()
		{
			var gitCloneCommand = string.Format("clone {0} {1}", Url, Name);

			execute(gitCloneCommand, WORKING_FOLDER);
		}

		private void execute(string gitCommand, string path)
		{
			var gitInfo = new ProcessStartInfo
			{
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				FileName = Path.Combine(_settings.GitPath, "git.exe"),
				Arguments = gitCommand,
				WorkingDirectory = path
			};

			Console.WriteLine("Executing: git {0}", gitCommand);

			var process = new Process { StartInfo = gitInfo };
			process.Start();

			var stdError = process.StandardError.ReadToEnd();
			var stdOutput = process.StandardOutput.ReadToEnd();

			if (!string.IsNullOrEmpty(stdError))
			{
				Console.WriteLine("  ======================= error:\n{0}\n  ======================= /error", stdError);
			}

			if (!string.IsNullOrEmpty(stdOutput))
			{
				Console.WriteLine("  ======================= output:\n{0}\n  ======================= /output", stdOutput);
			}

			process.WaitForExit();
			process.Close();
		}
	}
}