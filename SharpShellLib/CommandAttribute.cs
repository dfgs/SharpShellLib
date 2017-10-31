using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpShellLib
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CommandAttribute:Attribute
	{
		public string Name
		{
			get;
			set;
		}

		public CommandAttribute([CallerMemberName]string Name=null)
		{
			this.Name = Name;
			
		}
	}
}
