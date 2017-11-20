using System.Collections.Generic;

namespace ATC.modules.TVC.Formatter
{
	public abstract class TVCPostProcessor
	{
		public abstract string Name { get; }

		public abstract string Process(string data, IDictionary<string, string> settings);

		public static readonly TVCPostProcessor[] Processors =
		{
			new JsonFormatter(),
			new JsonSelector(),
			new Unescaper(),
			new JsonPropFilter(),
			new NetscapeConverter(),
		};
	}
}
