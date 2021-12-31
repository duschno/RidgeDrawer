using System.Diagnostics;

namespace SequenceGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			string folderName = "sequence";
			System.IO.Directory.CreateDirectory(folderName);
			Process p = new Process();
			p.StartInfo.FileName = "ImageDrawer.exe";
			for (int i = 20; i > 0; i--)
			{
				p.StartInfo.Arguments = $"\"..\\soldier.bmp\" \"{folderName}\\res{i}.bmp\" -l:50 -f:10 -c:{i}";
				p.Start();
			}
		}
	}
}
