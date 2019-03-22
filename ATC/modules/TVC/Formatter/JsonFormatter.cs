using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ATC.modules.TVC.Formatter
{
	class JsonFormatter : TVCTransformators
	{
		public override string Name => "json_format";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			return JObject.Parse(data).ToString(Newtonsoft.Json.Formatting.Indented);
		}
	}
}
