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
		private static readonly Regex commandRegex = new Regex(@"^([^ ]+) *((?: *--[^=]+ *= *[^ ]*)*)$");
		private static readonly Regex parametersRegex = new Regex(@"--([^=]+) *= *([^ ]*)");

		public static Dictionary<string, string> ParseParameters(string Line)
		{
			MatchCollection matches;
			string name, value;
			Dictionary<string, string> result;

			result = new Dictionary<string, string>();
			matches = parametersRegex.Matches(Line);
			foreach (Match match in matches)
			{
				name = match.Groups[1].Value;
				value = match.Groups[2].Value;

				if (result.ContainsKey(name)) throw (new Exception($"Duplicated parameter found ({name})"));
				
				result.Add(name, value);
			}

			return result;
		}

		public static string ParseCommand(string Line)
		{
			Match match;
			
			match = commandRegex.Match(Line);
			if (!match.Success) throw (new Exception("Syntax error"));

			return match.Groups[1].Value;
			//parameterNames = match.Groups[2].Value;
		}


	}
}
