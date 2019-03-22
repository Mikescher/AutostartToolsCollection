using System.Collections.Generic;

namespace ATC.modules.TVC.Formatter
{
	class Substring : TVCTransformators
	{
		public override string Name => "substring";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			int start = int.Parse(settings["start"]);
			if (settings.ContainsKey("length"))
			{
				int length = int.Parse(settings["length"]);
				return data.Substring(start, length);
			}
			else
			{
				return data.Substring(start);
			}
		}
	}
}
