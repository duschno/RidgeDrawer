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
			for (int i = 180; i > 1; i--)
			{
				p.StartInfo.Arguments = string.Join(" ", 
					new string[]
					{
						$@"..\soldier.png",
						$@"{folderName}\res{i}.bmp",
						$"-l:80",
						$"-f:30",
						$"-c:{i}",
						$"-lt:curve",
						$"-mt:ridge",
						$"-dos:1",
						$"-a:0",
					});
				p.Start();
			}
		}
	}
}
