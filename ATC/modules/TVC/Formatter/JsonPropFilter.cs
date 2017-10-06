using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ATC.modules.TVC.Formatter
{
	class JsonPropFilter : TVCPostProcessor
	{
		public override string Name => "json_propfilter";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			var jin = JObject.Parse(data);
			var jout = new JObject();

			var regex = new Regex(settings["regex"]);

			foreach (var prop in jin.Properties().Where(p => regex.IsMatch(p.Name))) jout.Add(prop.Name, prop.Value);

			return jout.ToString();
		}
	}
}
