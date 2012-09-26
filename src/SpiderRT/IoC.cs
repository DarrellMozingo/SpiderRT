using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Document;
using StructureMap;
using Taskie;

namespace SpiderRT
{
	public static class IoC
	{
		public static void Bootstrap()
		{
			var store = new DocumentStore
			{
				Url = "http://localhost:8080"
			}.Initialize();

			ObjectFactory.Initialize(x => x.Scan(y =>
			{
				y.TheCallingAssembly();
				y.WithDefaultConventions();
				y.AddAllTypesOf<ITask>();

				x.For<IDocumentStore>().Singleton().Use(store);
			}));
		}

		public static T Resolve<T>()
		{
			return ObjectFactory.GetInstance<T>();
		}

		public static IEnumerable<T> ResolveAll<T>()
		{
			return ObjectFactory.GetAllInstances<T>();
		}
	}
}