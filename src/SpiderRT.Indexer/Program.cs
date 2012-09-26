using Taskie;

namespace SpiderRT.Indexer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IoC.Bootstrap();
			TaskieRunner.RunWith(args, new TaskieServiceLocator());
		}
	}
}