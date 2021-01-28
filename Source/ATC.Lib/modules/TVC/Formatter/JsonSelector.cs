using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ATC.Lib.modules.TVC.Formatter
{
	class JsonSelector : TVCTransformators
	{
		public override string Name => "json_select";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			return JObject.Parse(data).GetValue(settings["jpath"]).ToString();
		}
	}
}
