using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharpShellLib
{
	public class Command
	{
		public string Name
		{
			get;
			set;
		}

		public MethodInfo MethodInfo
		{
			get;
			set;
		}

		public Dictionary<string,Parameter> Parameters
		{
			get;
			set;
		}

		public Command()
		{
			Parameters = new Dictionary<string, Parameter>();
		}


	}
}
