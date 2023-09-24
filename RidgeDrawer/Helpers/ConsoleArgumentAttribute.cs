using RidgeDrawer;
using System;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Property)]
public class ConsoleArgumentAttribute : Attribute
{
	public ConsoleArgumentAttribute(string name, string desc, Type type = null,
		bool isMandatory = false, bool isSignRequired = true)
	{
		Name = name;
		Description = desc;
		Type = type ?? typeof(int);
		IsMandatory = isMandatory;
		IsSignRequired = isSignRequired;
	}

	public string Name { get; private set; }
	public string Description { get; private set; }
	private Type Type { get; set; }
	private bool IsMandatory { get; set; }
	private bool IsSignRequired { get; set; }

	public object FromString(string value)
	{
		if (Type == typeof(bool))
		{
			if (string.IsNullOrEmpty(value))
				value = "true";
			return bool.Parse(value.Replace("1", "true").Replace("0", "false"));
		}
		if (Type.IsEnum)
			return Enum.Parse(Type, value, true);
		if (Type == typeof(BackendDrawerBase))
			return Type.GetType($"{Assembly.GetExecutingAssembly().GetName().Name}.{value}", true, true);
		return Convert.ChangeType(value, Type);
	}

	public bool Validate(string value)
	{
		return !string.IsNullOrEmpty(value) || Type == typeof(bool);
	}

	public string TypeName
	{
		get
		{
			if (Type.IsEnum)
				return Type.Name;
			if (Type == typeof(BackendDrawerBase))
				return Type.Name;
			if (Type == typeof(int))
				return "int";
			return null;
		}
	}

	public string AllowedValue
	{
		get
		{
			if (Type.IsEnum)
				return string.Join(", ", Enum.GetNames(Type));
			if (Type == typeof(BackendDrawerBase))
			{
				return string.Join(", ",
					typeof(BackendDrawerBase).Assembly.GetTypes()
					.Where(t => t.IsSubclassOf(typeof(BackendDrawerBase)))
					.Select(t => t.Name));
			}
			return null;
		}
	}

	public string GetArg(bool showMandatoriness)
	{
		string arg = IsSignRequired ? "-" : "";
		arg += Name;
		if (TypeName != null)
			arg += $":<{TypeName}>";
		if (showMandatoriness && !IsMandatory)
			arg = $"[{arg}]";
		return arg;
	}
}