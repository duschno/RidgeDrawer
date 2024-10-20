namespace RidgeDrawer
{
	public class LogicParams
	{
		[ConsoleArgument("input_image", "Input image destination path",
			null, typeof(string), true, false)]
		public string InputFilename { get; set; }

		[ConsoleArgument("output_image", "Output image destination path, input_image path by default",
			null, typeof(string), false, false)]
		public string OutputFilename { get; set; }

		public RenderParams RenderParams { get; set; }
	}
}
