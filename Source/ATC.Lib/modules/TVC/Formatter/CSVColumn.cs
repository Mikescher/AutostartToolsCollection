using System.Collections.Generic;

namespace ATC.Lib.modules.TVC.Formatter
{
	class CSVColumn : TVCTransformators
	{
		public override string Name => "csv_column";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			char sep = '\t';
			if (settings.ContainsKey("separator")) sep = settings["separator"][0];
			var cols = data.Split(sep);

			var idx = int.Parse(settings["column"]);

			if (cols.Length <= idx) return string.Empty;
			return cols[idx];
		}
	}
}
