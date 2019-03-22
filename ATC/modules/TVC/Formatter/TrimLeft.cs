using System.Collections.Generic;

namespace ATC.modules.TVC.Formatter
{
	class TrimLeft : TVCTransformators
	{
		public override string Name => "trim_left";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			if (settings.ContainsKey("chars")) return data.TrimStart(settings["chars"].ToCharArray());
			return data.TrimStart();
		}
	}
}
