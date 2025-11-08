namespace RidgeDrawer
{
	public class CASParams : ParamsBase
	{
		[ConsoleArgument("sharpening", "Sharpening", type: typeof(float))]
		public float Sharpening { get; set; } = 1.0f;

		[ConsoleArgument("conrtast", "Contrast", type: typeof(float))]
		public float Contrast { get; set; } = 0.0f;

		public override ParamsBase Clone()
		{
			return CloneImpl<CASParams>();
		}
	}
}
