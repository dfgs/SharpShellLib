using SharpShellLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	public class TestShell : Shell
	{
		public override string Prompt
		{
			get { return ">"; }
		}

		[Command(Name = "Hello")]
		public string SayHello([Parameter("Test")]string Name, int Count= 1)
		{
			return $"Hello {Name}."+Count;
		}

		[Command]
		public IEnumerable<string> List()
		{
			for(int t=0;t<10;t++)
			{
				yield return $"Item {t}";
			}
		}

	}




}
