using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpShellLib
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class ParameterAttribute:Attribute
	{
		public string Name
		{
			get;
			private set;
		}

		public ParameterAttribute(string Name )
		{
			this.Name = Name;
		}
	}
}
