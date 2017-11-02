using LogUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkerLib;

namespace SharpShellLib
{
    public abstract class Shell:ThreadWorker
    {

		private Dictionary<string, Command> commands;

		public virtual string Title
		{
			get { return "Shell"; }
		}
		public abstract string Prompt
		{
			get;
		}

		public Shell() : base("Shell",System.Threading.ThreadPriority.Normal)
		{
		}

		protected override sealed void OnInitializeRessources(params object[] Parameters)
		{
			Type type;
			CommandAttribute commandAttribute;
			ParameterAttribute parameterAttribute;
			Command command;
			Parameter parameter;

			base.OnInitializeRessources(Parameters);

			commands = new Dictionary<string, Command>();

			type = GetType();
			foreach(MethodInfo mi in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				commandAttribute=mi.GetCustomAttribute<CommandAttribute>();
				if (commandAttribute == null) continue;
				if (commands.ContainsKey(commandAttribute.Name))
				{
					WriteLog(LogLevels.Warning, $"Duplicate command found ({commandAttribute.Name})");
					continue;
				}

				command = new Command() { Name = commandAttribute.Name, MethodInfo=mi };

				foreach(ParameterInfo pi in mi.GetParameters())
				{
					parameter = new Parameter() { ParameterInfo = pi };
					parameterAttribute = pi.GetCustomAttribute<ParameterAttribute>();
					if (parameterAttribute != null) parameter.Name = parameterAttribute.Name;
					else parameter.Name = pi.Name;

					if (command.Parameters.ContainsKey(parameter.Name))
					{
						WriteLog(LogLevels.Warning, $"Duplicate parameter found in command {command.Name} ({parameter.Name})");
						goto nextMethod;
					}

					command.Parameters.Add(parameter.Name, parameter);
				}

				commands.Add(commandAttribute.Name, command);

				nextMethod:;

			}
		}

		protected override sealed void OnDisposeRessources()
		{
			base.OnDisposeRessources();

		}
		private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true;
			this.Stop();
		}

		public virtual IEnumerable<string> GetBannerLines()
		{
			yield break;
		}

		private Command GetCommand(string Name)
		{
			Command result;
			if (!commands.TryGetValue(Name, out result)) throw (new Exception($"Command not found ({Name})"));
			return result;
		}

		private Dictionary<string,object> GetParameters(Command Command,Dictionary<string,string> ParsedParameters)
		{
			string value;
			object converted;
			Dictionary<string, object> results;
			results = new Dictionary<string, object>();

			foreach (Parameter parameter in Command.Parameters.Values)
			{
				if (!ParsedParameters.TryGetValue(parameter.Name, out value))
				{
					if (!parameter.ParameterInfo.HasDefaultValue) throw(new Exception($"Missing parameter ({parameter.Name})"));
					results.Add(parameter.Name, parameter.ParameterInfo.DefaultValue);
				}
				else
				{
					try
					{
						converted = Convert.ChangeType(value, parameter.ParameterInfo.ParameterType);
					}
					catch (Exception ex)
					{
						WriteLog(ex);
						throw (new Exception($"Invalid value provided for parameter {parameter.Name} ({value})"));
					}
					results.Add(parameter.Name, converted);
				}
			}

			return results;
		}

		protected virtual void OnStart()
		{
		}
		protected virtual void OnStop()
		{
		}

		protected override void ThreadLoop()
		{
			string line;
			string commandPart, parameterPart;
			Command command;
			Dictionary<string,object> parameters;
			object result;
			bool explicitParamerters;

			Console.Title = Title;

			Console.CancelKeyPress += Console_CancelKeyPress;

			OnStart();

			foreach(string l in GetBannerLines())
			{
				Console.WriteLine(l);
			}

			while (State==WorkerStates.Started)
			{
				Console.Write(Prompt);
				line = Console.ReadLine();
				
				if (line == null) WaitHandles(StopTimeout, QuitEvent);
				else
				{
					if (string.IsNullOrWhiteSpace(line)) continue;

					try
					{
						explicitParamerters = Parser.ParseCommand(line, out commandPart, out parameterPart);
						command = GetCommand(commandPart);
						if (explicitParamerters) parameters = GetParameters(command, Parser.ParseExplicitParameters(parameterPart));
						else parameters = GetParameters(command, Parser.ParseImplicitParameters(parameterPart,command.Parameters));

						result =command.MethodInfo.Invoke(this, parameters.Values.ToArray() );
						if (result == null) continue;
						if (result is string)
						{
							Console.WriteLine(result);
						}
						else if (result is System.Collections.IEnumerable)
						{
							foreach(object item in (System.Collections.IEnumerable)result)
							{
								Console.WriteLine(item);
							}
						}
						else
						{
							Console.WriteLine(result);
						}

					}
					catch (Exception ex)
					{
						WriteLog(ex);
						Console.WriteLine(ex.Message);
						continue;
					}
										
				}
			}

			OnStop();
		}

		

		

		
		private string GetHelp(Command Command)
		{
			string result = Command.Name;
			foreach (Parameter parameter in Command.Parameters.Values)
			{
				if (parameter.ParameterInfo.HasDefaultValue) result += $" [--{parameter.Name}=value]";
				else result += $" --{parameter.Name}=value";
			}
			return result;
		}

		[Command]
		public IEnumerable<string> Help([Parameter("Command")]string CommandName=null)
		{
			Command command;

			if (CommandName != null)
			{
				command = GetCommand(CommandName);
				yield return GetHelp(command);
			}
			else
			{
				foreach (Command item in commands.Values)
				{
					yield return GetHelp(item);
				}
			}
						
		}

	}
}
