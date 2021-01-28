using System.Collections.Generic;

namespace ATC.Lib.modules.TVC.Formatter
{
	class TrimRight : TVCTransformators
	{
		public override string Name => "trim_right";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			if (settings.ContainsKey("chars")) return data.TrimEnd(settings["chars"].ToCharArray());
			return data.TrimEnd();
		}
	}
}
