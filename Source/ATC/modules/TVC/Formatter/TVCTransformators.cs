using System;
using System.Collections.Generic;
using System.Linq;

namespace ATC.modules.TVC.Formatter
{
	public abstract class TVCTransformators
	{
		public abstract string Name { get; }

		public abstract string Process(string data, IDictionary<string, string> settings);

		public static readonly Dictionary<string, TVCTransformators> Processors = new TVCTransformators[]
		{
			new JsonFormatter(),
			new JsonSelector(),
			new Unescaper(),
			new JsonPropFilter(),
			new NetscapeConverter(),
			new TrimRight(),
			new TrimLeft(),
			new CSVColumn(), 
			new Uppercase(),
			new Lowercase(),
			new Substring(),
		}.ToDictionary(p => p.Name, p => p, StringComparer.InvariantCultureIgnoreCase);
	}
}
