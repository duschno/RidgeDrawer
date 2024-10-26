using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RidgeDrawer
{
	public static class ArgsHelper
	{
		internal static void PrintUsage()
		{
			string argLine = $"Usage: {Assembly.GetCallingAssembly().GetName().Name}";
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
				var desc = attr.FullDescription;
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

		public static LogicParams ParamsFromArgs(LogicParams defaultLogicParam, bool inputFileRequired, string[] args)
		{
			LogicParams logicParam = defaultLogicParam;
			if (inputFileRequired)
			{
				if (args.Length == 0)
					throw new ArgumentException("Image filename not specified");

				logicParam.InputFilename = args[0];
				args = args.Skip(1).ToArray();
			}
			else
			{
				if (args.Length == 0)
					return logicParam;

				if (!args[0].StartsWith("-"))
				{
					logicParam.InputFilename = args[0];
					args = args.Skip(1).ToArray();
				}
			}

			if (args.Length > 1 && !args[1].StartsWith("-"))
			{
				logicParam.OutputFilename = args[1];
				args = args.Skip(1).ToArray();
			}

			if (logicParam.RenderParams == null)
				logicParam.RenderParams = new RenderParams();

			var propAndAttrs = new List<Tuple<PropertyInfo, ConsoleArgumentAttribute>>(); // todo: move to class
			foreach (PropertyInfo prop in typeof(RenderParams).GetProperties()) // todo: move to method
			{
				var attr = prop.GetCustomAttributes().OfType<ConsoleArgumentAttribute>()
					.FirstOrDefault();
				if (attr != null)
					propAndAttrs.Add(new Tuple<PropertyInfo, ConsoleArgumentAttribute>(prop, attr));
			}
			Regex r = new Regex(@"^-(?<name>\w+)(:(?<value>.+))?$", RegexOptions.ExplicitCapture);
			RenderParams renderParam = logicParam.RenderParams;
			foreach (string arg in args)
			{
				Match match = r.Match(arg);
				if (match.Success)
				{
					string name = match.Groups["name"].Value;
					string value = match.Groups["value"].Value;
					var propAndAttr = propAndAttrs.FirstOrDefault(p => p.Item2.Name == name);
					if (propAndAttr != null)
					{
						if (propAndAttr.Item2.Validate(value))
							propAndAttr.Item1.SetValue(renderParam, propAndAttr.Item2.FromString(value));
						else
							throw new ArgumentException($"Value required for {arg}");
					}
					else
						throw new ArgumentException($"Invalid arg {arg}");
				}
				else
					throw new ArgumentException($"Invalid format {arg}");
			}

			logicParam.RenderParams = renderParam;
			return logicParam;
		}

		public static string ArgsFromParams(string filename, RenderParams param)
		{
			if (string.IsNullOrEmpty(filename))
				return string.Empty;

			string result = filename.Contains(" ") ? $"\"{filename}\"" : filename;
			PropertyInfo[] props = param.GetType().GetProperties();
			foreach (PropertyInfo prop in props)
			{
				string name = (string)prop.CustomAttributes.FirstOrDefault().ConstructorArguments[0].Value;
				object fieldValue = prop.GetValue(param);
				if (fieldValue == null)
					continue;
				string value = fieldValue.ToString();
				if (prop.PropertyType == typeof(Type))
					value = value.Substring(value.LastIndexOf('.') + 1);

				if (prop.PropertyType == typeof(bool))
				{
					if ((bool)prop.GetValue(param))
						result += $" -{name}";
				}
				else
					result += $" -{name}:{value}";
			}

			return result;
		}

		/// <summary>
		/// Adds postfix to the name of processed image
		/// </summary>
		/// <param name="imagePath">Original image name</param>
		/// <returns>Name with postfix</returns>
		public static string AddPostfix(string imagePath, string extension, bool includeDirectory)
		{
			string directory = includeDirectory ? Path.GetDirectoryName(imagePath) : string.Empty;
			string filename = Path.GetFileNameWithoutExtension(imagePath);
			return Path.Combine(directory, $"{filename}_processed.{extension}");
		}
	}
}
