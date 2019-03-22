using System.Collections.Generic;

namespace ATC.modules.TVC.Formatter
{
	class Lowercase : TVCTransformators
	{
		public override string Name => "lowercase";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			return data.ToLower();
		}
	}
}
