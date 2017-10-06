using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ATC.modules.TVC.Formatter
{
	class Unescaper : TVCPostProcessor
	{
		public override string Name => "unescape";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			return Regex.Unescape(data);
		}
	}
}
