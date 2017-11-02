using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharpShellLib
{
	public static class Parser
	{
		private static readonly Regex explicitCommandRegex = new Regex(@"^([^ ]+) *((?: *--[^=]+ *= *(?:(""[^""]*"")|([^ ]*)))*)$");
		private static readonly Regex implicitCommandRegex = new Regex(@"^([^ ]+) *(.*)$");
		private static readonly Regex explicitParametersRegex = new Regex(@"--([^=]+) *= *(?:""([^""]*)""|([^ ]*))");
		private static readonly Regex implicitParametersRegex = new Regex(@"""([^""]*)""|([^ ]+)");

		public static Dictionary<string, string> ParseExplicitParameters(string ParametersPart)
		{
			MatchCollection matches;
			string name, value;
			Dictionary<string, string> result;

			result = new Dictionary<string, string>();
			matches = explicitParametersRegex.Matches(ParametersPart);
			foreach (Match match in matches)
			{
				name = match.Groups[1].Value;
				value = match.Groups[2].Value + match.Groups[3].Value;

				if (result.ContainsKey(name)) throw (new Exception($"Duplicated parameter found ({name})"));
				
				result.Add(name, value);
			}

			return result;
		}

		public static Dictionary<string, string> ParseImplicitParameters(string ParametersPart, Dictionary<string, Parameter> Parameters)
		{
			MatchCollection matches;
			string name, value;
			Dictionary<string, string> result;
			int count;
			string[] names;

			result = new Dictionary<string, string>();

			names = Parameters.Values.Select(item=>item.Name).ToArray();
			matches = implicitParametersRegex.Matches(ParametersPart);

			count = Math.Min(matches.Count, names.Length);
			for(int t=0;t<count;t++)
			{
				name = names[t];
				value = matches[t].Groups[1].Value+ matches[t].Groups[2].Value;

				if (result.ContainsKey(name)) throw (new Exception($"Duplicated parameter found ({name})"));

				result.Add(name, value);
			}

			return result;
		}

		public static bool ParseCommand(string Line,out string Command,out string Parameters)
		{
			Match match;

			match = explicitCommandRegex.Match(Line);
			if (match.Success)
			{
				Command = match.Groups[1].Value;
				Parameters = match.Groups[2].Value;
				return true;
			}
			match = implicitCommandRegex.Match(Line);
			if (match.Success)
			{
				Command = match.Groups[1].Value;
				Parameters = match.Groups[2].Value;
				return false;
			}

			throw (new Exception("Syntax error"));

			
			//parameterNames = match.Groups[2].Value;
		}

		
	}
}
