using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RidgeDrawer
{
	internal static class ConsoleHelper
	{
		internal static void ShowUsage()
		{
			string argLine = "Usage: ridgedrawer";
			int usageIndent = argLine.Length;
			int longestArg = 0;

			var argAttrs = new List<ConsoleArgumentAttribute>();
			argAttrs.AddRange(GetPropertiesAttributes<LogicParams, ConsoleArgumentAttribute>());
			argAttrs.AddRange(GetPropertiesAttributes<RenderParams, ConsoleArgumentAttribute>());
			foreach (ConsoleArgumentAttribute attr in argAttrs)
			{
				int argLength = attr.GetArg(false).Length;
				if (argLength > longestArg)
					longestArg = argLength;
				string arg = attr.GetArg(true);
				string temp = string.Join(" ", argLine, arg);
				if (temp.Length < Console.WindowWidth)
					argLine = temp;
				else
				{
					Console.WriteLine(argLine);
					argLine = new string(' ', usageIndent + 1) + arg;
				}
			}
			Console.WriteLine(argLine);

			int tab = 3;
			int descrWidth = Console.WindowWidth - longestArg - tab - 1;
			Console.WriteLine("Params:");
			foreach (var attr in argAttrs)
			{
				var desc = attr.Description;
				if (attr.AllowedValue != null)
					desc += $"\nAllowed values: {attr.AllowedValue}";

				var descrLines = new List<string>();
				foreach (var descrLine in desc.Split('\n'))
					descrLines.AddRange(descrLine.Trim().Chunk(descrWidth));
				
				for (int i = 0; i < descrLines.Count; i++)
				{
					string line;
					if (i == 0)
						line = $"  {attr.GetArg(false).PadRight(longestArg)} {descrLines[i]}";
					else
						line = new string(' ', longestArg + tab) + descrLines[i];
					Console.WriteLine(line);
				}
			}
		}

		private static IEnumerable<TAttribute> GetPropertiesAttributes<TClass, TAttribute>()
			where TAttribute : Attribute
		{
			List<TAttribute> result = new List<TAttribute>();
			foreach (var prop in typeof(TClass).GetProperties())
			{
				var attr = prop.GetCustomAttributes<TAttribute>(false).FirstOrDefault();
				if (attr != null)
					result.Add(attr);
			}
			return result;
		}
	}
}
