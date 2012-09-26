using System.Collections.Generic;
using Taskie;

namespace SpiderRT
{
	public class TaskieServiceLocator : ITaskieServiceLocator
	{
		public INSTANCE GetInstance<INSTANCE>()
		{
			return IoC.Resolve<INSTANCE>();
		}

		public IEnumerable<INSTANCE> GetAllInstances<INSTANCE>()
		{
			return IoC.ResolveAll<INSTANCE>();
		}
	}
}