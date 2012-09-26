using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SpiderRT.SlowTests
{
	public class GitManagerTests
	{
		[Test]
		public void Should_clone_the_repo_if_it_does_not_exist()
		{
			createGitRepo();
			addFileToGitRepo("test.txt");

			var gitManager = createGitManager();

			gitManager.CreateOrUpdate();

			Assert.That(Directory.Exists(_clonedRepoPath), Is.True);
			Assert.That(File.Exists(Path.Combine(_clonedRepoPath, "test.txt")), Is.True);
		}

		[Test]
		public void Should_update_the_repo_if_it_already_exists()
		{
			createGitRepo();
			addFileToGitRepo("random-first-file.txt");
			
			var gitManager = createGitManager();

			gitManager.CreateOrUpdate();

			Assert.That(Directory.GetFiles(_clonedRepoPath).Single(), Is.StringEnding("random-first-file.txt"));

			addFileToGitRepo("random-second-file.txt");

			gitManager.CreateOrUpdate();

			Assert.That(Directory.Exists(_clonedRepoPath), Is.True);
			Assert.That(File.Exists(Path.Combine(_clonedRepoPath, "random-first-file.txt")), Is.True);
			Assert.That(File.Exists(Path.Combine(_clonedRepoPath, "random-second-file.txt")), Is.True);
		}

		private static readonly string _workingFolder = Path.GetTempPath();
		private static readonly string _gitExePath = Path.GetFullPath("../../../../tools/git/bin/git.exe");

		private string _randomRepoFolder;

		private string _randomRepoName
		{
			get { return _randomRepoFolder + "-clone"; }
		}

		private string _fullGitRepoPath
		{
			get { return Path.Combine(_workingFolder, _randomRepoFolder); }
		}

		private string _clonedRepoPath
		{
			get { return Path.Combine(_workingFolder, _randomRepoName); }
		}

		[SetUp]
		public void Once_before_each_test()
		{
			_randomRepoFolder = Guid.NewGuid().ToString();
		}

		[TearDown]
		public void After_each_test()
		{
			deleteGitFolder(_fullGitRepoPath);
			deleteGitFolder(_clonedRepoPath);
		}

		private static void deleteGitFolder(string path)
		{
			makeHiddenFolderDeletable(new DirectoryInfo(Path.Combine(path, ".git")));
			Directory.Delete(path, true);
		}

		private GitManager createGitManager()
		{
			return new GitManager(_workingFolder, _gitExePath, _fullGitRepoPath, _randomRepoName);
		}

		private void createGitRepo()
		{
			Directory.CreateDirectory(_fullGitRepoPath);
			runGitCommand("init");
		}

		private void addFileToGitRepo(string filename)
		{
			File.WriteAllText(Path.Combine(_fullGitRepoPath, filename), "");

			runGitCommand("add -A");
			runGitCommand("commit -m ''");
		}

		private static void makeHiddenFolderDeletable(DirectoryInfo directoryInfo)
		{
			directoryInfo.GetDirectories().ForEach(makeHiddenFolderDeletable);
			directoryInfo.GetFiles().ForEach(file => file.Attributes = FileAttributes.Normal);
			directoryInfo.Attributes = FileAttributes.Normal;
		}

		private void runGitCommand(string command)
		{
			var gitProcessInfo = new ProcessStartInfo
			{
				RedirectStandardError = true,
				CreateNoWindow = true,
				UseShellExecute = false,
				FileName = _gitExePath,
				Arguments = command,
				WorkingDirectory = _fullGitRepoPath
			};

			using (var process = new Process { StartInfo = gitProcessInfo })
			{
				process.Start();
				process.WaitForExit();

				var stdError = process.StandardError.ReadToEnd();

				if(!string.IsNullOrEmpty(stdError))
				{
					Console.WriteLine("Error from git command \"{0}\": {1}", command, stdError);
				}
			}
		}
	}
}