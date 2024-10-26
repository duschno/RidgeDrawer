using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace RidgeDrawer
{
	[Guid("1A91BC9C-09A1-4817-9C71-4D91A674AAF1")]
	public class Logic
	{
		private static BackendBase backend;

		/// <summary>
		/// Gets backend implementation
		/// </summary>
		/// <param name="type">Backend to use</param>
		/// <returns>Backend instance</returns>
		private static BackendBase GetBackend(Type type)
		{
			try
			{
				return (BackendBase)Activator.CreateInstance(type);
			}
			catch (Exception e)
			{
				throw new NotImplementedException($"{type.Name} backend is not supported", e);
			}
		}

		public static string OutputTypeDescription => backend.OutputTypeDescription;
		public static string OutputType => backend.OutputTypeExtension;

		/// <summary>
		/// Gets all possible implementations
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public static IEnumerable<Type> GetImplementations(Type interfaceType)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract);
		}

		/// <summary>
		/// Renders whole image
		/// </summary>
		/// <param name="inputFilename">Input file path</param>
		/// <param name="param">Render params</param>
		/// <returns>Rendered bitmap</returns>
		public static Bitmap Render(string inputFilename, RenderParams param)
		{
			Bitmap origBitmap = new Bitmap(inputFilename);
			Bitmap newBitmap = new Bitmap(origBitmap.Width, origBitmap.Height);

			using (var graphics = Graphics.FromImage(newBitmap))
			using (SolidBrush brush = new SolidBrush(Color.White))
				graphics.FillRectangle(brush, 0, 0, newBitmap.Width, newBitmap.Height);

			backend = GetBackend(param.Backend);
			backend.Construct(newBitmap, origBitmap, param);
			backend.Draw();

			return newBitmap;
		}

		/// <summary>
		/// Saves BitmapSource by given filename
		/// </summary>
		/// <param name="bmp">Bitmap to save</param>
		/// <param name="outputFilename">Name of the file to use</param>
		public static void Save(string outputFilename)
		{
			backend.Save(outputFilename);
		}

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
					Effect = typeof(RidgeLines),
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
			//      Method = MethodType.Squiggle,
			//		Effect = EffectType.Squiggle,
			//		DrawOnSides = true,
			//		FillInside = true,
			//		Invert = false,
			//		Debug = false,
			//		Backend = typeof(GDIPlus)
			//	}
			//};
#endif
			if (args.Length == 0)
			{
				ArgsHelper.PrintUsage();
				return;
			}

			try
			{
				logicParam = ArgsHelper.ParamsFromArgs(logicParam, true, args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return;
			}
			Render(logicParam.InputFilename, logicParam.RenderParams); // TODO: not needed when SVG
			if (logicParam.OutputFilename == null)
				logicParam.OutputFilename = ArgsHelper.AddPostfix(logicParam.InputFilename, OutputType, true);
			Save(logicParam.OutputFilename);
			Console.WriteLine($"Saved to {logicParam.OutputFilename}");
		}

		#endregion
	}
}
