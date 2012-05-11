using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace SpiderRT.Web.Controllers
{
	public class CodeFile_ByContent : AbstractIndexCreationTask
	{
		public override IndexDefinition CreateIndexDefinition()
		{
			return new IndexDefinitionBuilder<CodeFile>
			{
				Map = codeFiles => from codeFile in codeFiles
				                   select new { codeFile.Content },
				Indexes = { { x => x.Content, FieldIndexing.Analyzed } }
			}.ToIndexDefinition(new DocumentStore().Conventions);
		}
	}
}