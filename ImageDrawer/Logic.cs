using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace ImageDrawer
{
	[Guid("1A91BC9C-09A1-4817-9C71-4D91A674AAF1")]
	public class Logic
	{
		/// <summary>
		/// Gets backend drawer implementation
		/// </summary>
		/// <param name="type">Backend to use</param>
		/// <returns>Drawer instance</returns>
		private static BackendDrawerBase GetBackendDrawer(Type type)
		{
			try
			{
				return (BackendDrawerBase)Activator.CreateInstance(type);
			}
			catch (Exception e)
			{
				throw new NotImplementedException($"{type.Name} backend is not supported", e);
			}
		}

		public static IEnumerable<Type> GetImplementations(Type interfaceType)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract);
		}

		/// <summary>
		/// Renders whole image
		/// </summary>
		/// <param name="origBitmap">Original bitmap</param>
		/// <param name="param">Render params</param>
		/// <returns>Rendered bitmap</returns>
		private static Bitmap RenderImage(Bitmap origBitmap, RenderParams param)
		{
			Bitmap newBitmap = new Bitmap(origBitmap.Width, origBitmap.Height);

			using (var graphics = Graphics.FromImage(newBitmap))
			{
				using (SolidBrush brush = new SolidBrush(Color.White))
					graphics.FillRectangle(brush, 0, 0, newBitmap.Width, newBitmap.Height);

				BackendDrawerBase drawer = GetBackendDrawer(param.Backend);
				drawer.Construct(newBitmap, origBitmap, param);
				drawer.Draw();
			}

			return newBitmap;
		}

		public static Bitmap ProcessByFilename(string inputFilename, RenderParams param)
		{
			Bitmap bmp = new Bitmap(inputFilename);
			return RenderImage(bmp, param);
		}

		/// <summary>
		/// Saves BitmapSource by given filename
		/// </summary>
		/// <param name="bmp">Bitmap to save</param>
		/// <param name="outputFilename">Name of the file to use</param>
		public static void SaveAsPng(BitmapSource bmp, string outputFilename)
		{
			using (var fileStream = new FileStream(outputFilename, FileMode.Create))
			{
				BitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(bmp));
				encoder.Save(fileStream);
			}
		}

		#region Helpers

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteObject([In] IntPtr hObject);

		/// <summary>
		/// Converts Bitmap to BitmapSource
		/// </summary>
		/// <param name="bmp">Bitmap image</param>
		/// <returns>BitmapSource image</returns>
		public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
		{
			var handle = bmp.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(handle, 
					IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally { DeleteObject(handle); }
		}

		#endregion

		#region Console app related stuff

		public static void ProcessByArgs(string[] args)
		{
			Main(args);
		}

		/// <summary>
		/// Entry point of the console app
		/// </summary>
		/// <param name="args">Command line arguments</param>
		private static void Main(string[] args)
		{
			// default values
			LogicParams logicParam = new LogicParams
			{
				InputFilename = null,
				OutputFilename = null,
				RenderParams = new RenderParams
				{
					LinesCount = 1,
					Stroke = 1,
					Factor = 0,
					ChunkSize = 1,
					GreyPoint = 127,
					BlackPoint = 0,
					WhitePoint = 255,
					Smoothing = SmoothingType.None,
					LineType = LineType.Line,
					Method = MethodType.Ridge,
					DrawOnSides = false,
					PointsAroundPeak = -1,
					FillInside = false,
					Invert = false,
					Debug = false,
					Backend = typeof(GDIPlus),
					PullPointX = 960,
					PullPointY = 540
				}
			};

#if DEBUG
			//// debug values
			//logicParam = new LogicParams
			//{
			//	InputFilename = @"..\Rachel-Carson.jpg",
			//	OutputFilename = @"..\Rachel-Carson_proc.jpg",
			//	RenderParams = new RenderParams
			//	{
			//		LinesCount = 80,
			//		Stroke = 1,
			//		Factor = 5,
			//		ChunkSize = 5,
			//		BlackPoint = 0,
			//		WhitePoint = 255,
			//		Smoothing = SmoothingType.Antialias,
			//		LineType = LineType.Curve,
			//		Method = MethodType.Squiggle,
			//		DrawOnSides = true,
			//		FillInside = true,
			//		Invert = false,
			//		Debug = false,
			//		Backend = typeof(GDIPlus)
			//	}
			//};
#endif

			logicParam = ParseArgs(logicParam, args);
			Bitmap bmp = ProcessByFilename(logicParam.InputFilename, logicParam.RenderParams);
			SaveAsPng(BitmapToBitmapSource(bmp), logicParam.OutputFilename);
		}

		private static LogicParams ParseArgs(LogicParams defaultLogicParam, string[] args)
		{
			LogicParams logicParam = defaultLogicParam;
			if (logicParam.InputFilename == null && args.Length == 0)
				throw new ArgumentException("Image filename not specified");
			if (args.Length == 0)
				return logicParam;

			logicParam.InputFilename = args[0];

			if (args.Length > 1 && !args[1].StartsWith("-"))
				logicParam.OutputFilename = args[1];
			else
				logicParam.OutputFilename = AddPostfix(logicParam.InputFilename);

			FieldInfo[] fields = logicParam.RenderParams.GetType().GetFields();
			Regex r = new Regex(@"-(?'name'\w+)(?'value':.+)?");
			object renderParamObj = logicParam.RenderParams;
			foreach (string arg in args)
			{
				Match match = r.Match(arg);
				if (match.Success)
				{
					string name = match.Groups["name"].Value;
					string value = match.Groups["value"].Value;
					value = string.IsNullOrEmpty(value) ? null : value.Substring(1);
					FieldInfo field = fields.FirstOrDefault(
						f => (string)f.CustomAttributes.FirstOrDefault().ConstructorArguments[0].Value == name);

					if (field != null)
					{
						object obj = null;
						if (field.FieldType == typeof(bool))
							obj = bool.Parse((value ?? "true").Replace("1", "true").Replace("0", "false"));
						else if (field.FieldType.IsEnum)
							obj = Enum.Parse(field.FieldType, value, true);
						else if (field.FieldType == typeof(Type))
							obj = Type.GetType($"{Assembly.GetExecutingAssembly().GetName().Name}.{value}", true, true);
						else
							obj = Convert.ChangeType(value, field.FieldType);

						field.SetValue(renderParamObj, obj);
					}
				}

			}

			logicParam.RenderParams = (RenderParams)renderParamObj;
			return logicParam;
		}

		public static string CopyArgs(string filename, RenderParams param)
		{
			if (string.IsNullOrEmpty(filename))
				return string.Empty;

			string result = filename.Contains(" ") ? $"\"{filename}\"" : filename;
			FieldInfo[] fields = param.GetType().GetFields();
			foreach (FieldInfo field in fields)
			{
				string name = (string)field.CustomAttributes.FirstOrDefault().ConstructorArguments[0].Value;
				object fieldValue = field.GetValue(param);
				if (fieldValue == null)
					continue;
				string value = fieldValue.ToString();
				if (field.FieldType == typeof(Type))
					value = value.Substring(value.LastIndexOf('.') + 1);

				if (field.FieldType == typeof(bool))
				{
					if ((bool)field.GetValue(param))
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
		private static string AddPostfix(string imagePath)
		{
			string directory = Path.GetDirectoryName(imagePath);
			string filename = Path.GetFileNameWithoutExtension(imagePath);
			string extension = ImageFormat.Png.ToString().ToLower();
			return Path.Combine(directory, $"{filename}_processed.{extension}");
		}

#endregion
	}
}
