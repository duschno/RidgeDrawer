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

		public static Bitmap ProcessByFilename(string inputFileName, RenderParams param)
		{
			Bitmap bmp = new Bitmap(inputFileName);
			return RenderImage(bmp, param);
		}

		/// <summary>
		/// Saves BitmapSource by given filename
		/// </summary>
		/// <param name="bmp">Bitmap to save</param>
		/// <param name="outputFileName">Name of the file to use</param>
		public static void Save(BitmapSource bmp, string outputFileName)
		{
			using (var fileStream = new FileStream(outputFileName, FileMode.Create))
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
			string inputFileName = null;
			string outputFileName = null;
			RenderParams param = new RenderParams
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
			};

#if DEBUG
			//inputFileName = @"..\Rachel-Carson.jpg";
			//outputFileName = @"..\Rachel-Carson_proc.jpg";
			//param = new RenderParams
			//{
			//	LinesCount = 80,
			//	Stroke = 1,
			//	Factor = 5,
			//	ChunkSize = 5,
			//	BlackPoint = 0,
			//	WhitePoint = 255,
			//	Smoothing = SmoothingType.Antialias,
			//	LineType = LineType.Curve,
			//	Method = MethodType.Squiggle,
			//	DrawOnSides = true,
			//	FillInside = true,
			//	Invert = false,
			//	Debug = false,
			//	Backend = typeof(GDIPlus)
			//};
#endif

			ParseArgs(ref inputFileName, ref outputFileName, ref param, args);
			Bitmap bmp = ProcessByFilename(inputFileName, param);
			bmp.Save(outputFileName, ImageFormat.Bmp);
		}

		private static void ParseArgs(ref string inputFileName, ref string outputFileName, ref RenderParams param, string[] args)
		{
			if (inputFileName == null && args.Length == 0)
				throw new ArgumentException("Image filename not specified");
			if (args.Length == 0)
				return;

			inputFileName = args[0];

			if (args.Length > 1 && !args[1].StartsWith("-"))
				outputFileName = args[1];
			else
				outputFileName = AddPostfix(inputFileName);

			FieldInfo[] fields = param.GetType().GetFields();
			Regex r = new Regex(@"-(?'name'\w+)(?'value':.+)?");
			object paramObj = param;
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

						field.SetValue(paramObj, obj);
					}
				}

			}

			param = (RenderParams)paramObj;
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
		/// <param name="imageName">Original image name</param>
		/// <returns>Name with postfix</returns>
		private static string AddPostfix(string imageName)
		{
			string filename = Path.GetFileNameWithoutExtension(imageName);
			string outputExt = ImageFormat.Bmp.ToString().ToLower();
			return filename + "_processed." + outputExt; 
		}

#endregion
	}
}
