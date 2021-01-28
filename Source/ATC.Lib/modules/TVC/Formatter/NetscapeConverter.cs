using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Xml;
using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ATC.Lib.modules.TVC.Formatter
{
	class NetscapeConverter : TVCTransformators
	{
		public override string Name => "netscape_bookmarks_convert";

		public override string Process(string data, IDictionary<string, string> settings)
		{
			return Parse(data).Serialize();
		}

		private BMGroup Parse(string code)
		{
			BMGroup root = new BMGroup(null);
			BMGroup curr = root;
			BMElement last_el = null;

			var linenum = 0;

			var next_is_dl = true;
			var in_comment = false;

			foreach (var rline in Regex.Split(code, @"\r?\n"))
			{
				var line = rline.Trim();
				linenum++;

				if (string.IsNullOrWhiteSpace(line)) continue;

				if (line.StartsWith("<!--") && line.EndsWith("-->")) continue;

				if (line.StartsWith("<!--")) { in_comment = true; continue; }

				if (in_comment && line.EndsWith("-->")) { in_comment = false; continue; }

				if (in_comment) continue;

				if (linenum == 1 && line.StartsWith("<!DOCTYPE")) continue;
				
				if (line.StartsWith("<HR>")) { continue; } //ignore

				if (line.StartsWith("<META"))
				{
					if (curr != root) throw new Exception($"[{linenum}] <META> only allowed in top-level. Line := {line}");

					continue;
				}

				if (line.StartsWith("<TITLE"))
				{
					if (curr != root) throw new Exception($"[{linenum}] <TITLE> only allowed in top-level. Line := {line}");
					root.Title = HttpUtility.HtmlDecode(Regex.Match(line, @">([^>]+)</TITLE>$").Groups[1].Value);

					continue;
				}

				if (line.StartsWith("<H1"))
				{
					if (curr != root) throw new Exception($"[{linenum}] <H1> only allowed in top-level. Line := {line}");
					root.Comment = HttpUtility.HtmlDecode(Regex.Match(line, @">([^>]+)</H1>$").Groups[1].Value);

					continue;
				}

				if (line.StartsWith("<DL>"))
				{
					if (!next_is_dl)
					{
						var nc = new BMGroup(curr);
						curr.Children.Add(nc);
						curr = nc;
					}
					next_is_dl = false;

					continue;
				}

				if (line.StartsWith("<DT><A"))
				{
					if (next_is_dl) throw new Exception($"[{linenum}] Expected <DL>, found {line}");

					var el = new BMLink(curr);
					el.Title = HttpUtility.HtmlDecode(Regex.Match(line, @">([^>]+)</A>$").Groups[1].Value);
					el.Target = HttpUtility.HtmlDecode(Regex.Match(line, "HREF=\"([^\"]+)\"").Groups[1].Value);
					el.CreationDate = ExtractDate(line, "ADD_DATE");
					el.ModificationDate = ExtractDate(line, "LAST_MODIFIED");

					curr.Children.Add(el);
					last_el = el;

					continue;
				}

				if (line.StartsWith("<DT><H3"))
				{
					if (next_is_dl) throw new Exception($"[{linenum}] Expected <DL>, found {line}");

					next_is_dl = true;

					var nc = new BMGroup(curr);
					curr.Children.Add(nc);
					curr = nc;

					curr.Title = HttpUtility.HtmlDecode(Regex.Match(line, @">([^>]+)</H3>$").Groups[1].Value);
					curr.CreationDate = ExtractDate(line, "ADD_DATE");
					curr.ModificationDate = ExtractDate(line, "LAST_MODIFIED");

					continue;
				}

				if (line.StartsWith("</DL>"))
				{
					if (next_is_dl) throw new Exception($"[{linenum}] Expected <DL>, found {line}");
					next_is_dl = false;
					curr = curr.Parent;

					continue;
				}

				if (line.StartsWith("<DD>"))
				{
					if (next_is_dl)
						curr.Comment = HttpUtility.HtmlDecode(Regex.Match(line, @"<DD>(.*)$").Groups[1].Value);
					else
						last_el.Comment = HttpUtility.HtmlDecode(Regex.Match(line, @"<DD>(.*)$").Groups[1].Value);

					continue;
				}

				throw new Exception($"[{linenum}] Unknown netscape in line: {line}");
			}

			return root;
		}

		private DateTime? ExtractDate(string line, string ident)
		{
			var rex = Regex.Match(line, ident + "=\"([0-9]+)\"");
			if (!rex.Success) return null;

			return (new DateTime(1970, 1, 1)).AddSeconds(int.Parse(rex.Groups[1].Value));
		}

		private static string XmlEscape(string unescaped)
		{
			XmlDocument doc = new XmlDocument();
			var node = doc.CreateAttribute("foo");
			node.InnerText = unescaped;
			return node.InnerXml.Replace("\"", "&quot;").Replace("\r", "&#xD;").Replace("\n", "&#xA;");
		}

		private abstract class BMElement
		{
			public readonly BMGroup Parent;

			public string Title;
			public string Comment;
			public DateTime? CreationDate;
			public DateTime? ModificationDate;

			public BMElement(BMGroup p) { Parent = p; }

			public abstract string Serialize(int nindent);
		}

		private class BMGroup : BMElement
		{
			public List<BMElement> Children = new List<BMElement>();

			public BMGroup(BMGroup p) : base(p) { }

			public string Serialize()
			{
				var ret = new StringBuilder();
				ret.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
				ret.AppendLine(Serialize(0));
				return ret.ToString();
			}

			public override string Serialize(int nindent)
			{
				var indent = new string('\t', nindent);

				var ret = new StringBuilder();

				ret.Append(indent);
				ret.Append("<group title=\"");
				ret.Append(XmlEscape(Title));
				ret.Append("\"");
				if (CreationDate != null) ret.Append(" cdate=\"" + CreationDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + "\"");
				if (ModificationDate != null) ret.Append(" mdate=\"" + ModificationDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + "\"");
				if (!string.IsNullOrEmpty(Comment)) ret.Append(" comment=\"" + XmlEscape(Comment) + "\"");

				if (Children.Any())
				{
					ret.AppendLine(">");
					foreach (var child in Children) ret.AppendLine(child.Serialize(nindent + 1));
					ret.AppendLine(indent + "</group>");
				}
				else
				{
					ret.AppendLine("/>");
				}

				return ret.ToString().TrimEnd('\r', '\n');
			}
		}

		private class BMLink : BMElement
		{
			public string Target;

			public BMLink(BMGroup p) : base(p) { }

			public override string Serialize(int nindent)
			{
				var indent = new string('\t', nindent);

				var ret = new StringBuilder();

				ret.Append(indent);

				ret.Append("<link title=\"");
				ret.Append(XmlEscape(Title));
				ret.Append("\"");

				ret.Append(" href=\"");
				ret.Append(XmlEscape(Target));
				ret.Append("\"");

				if (CreationDate != null) ret.Append(" cdate=\"" + CreationDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + "\"");

				if (ModificationDate != null) ret.Append(" mdate=\"" + ModificationDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + "\"");

				if (!string.IsNullOrEmpty(Comment)) ret.Append(" comment=\"" + XmlEscape(Comment) + "\"");

				ret.AppendLine("/>");

				return ret.ToString().TrimEnd('\r', '\n');
			}
		}

	}
}