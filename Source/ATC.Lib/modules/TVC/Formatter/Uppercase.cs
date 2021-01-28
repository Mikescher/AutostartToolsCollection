using System.Collections.Generic;

namespace ATC.modules.TVC.Formatter
{
	class Uppercase : TVCTransformators
	{
		public override string Name => "uppercase";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			return data.ToUpper();
		}
	}
}
