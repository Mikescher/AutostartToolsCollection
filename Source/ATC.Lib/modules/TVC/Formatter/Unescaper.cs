using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ATC.Lib.modules.TVC.Formatter
{
	class Unescaper : TVCTransformators
	{
		public override string Name => "unescape";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			return Regex.Unescape(data);
		}
	}
}
