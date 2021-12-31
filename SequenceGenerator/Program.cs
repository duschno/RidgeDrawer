using ImageDrawer;
using System;
using System.Diagnostics;

namespace SequenceGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("press Y to start: ");
			if (Console.ReadLine() != "Y")
				return;

			string folderName = "sequence";
			System.IO.Directory.CreateDirectory(folderName);
			for (int i = 0; i < 360; i += 10)
			{
				//p.StartInfo.Arguments = string.Join(" ",
				//	new string[]
				//	{
				//		$@"..\soldier.png",
				//		$@"{folderName}\res{i}.bmp",
				//		$"-l:80",
				//		$"-f:30",
				//		$"-c:{i}",
				//		$"-lt:curve",
				//		$"-mt:ridge",
				//		$"-dos:1",
				//		$"-a:0",
				//	});
				string[] arguments = $"..\\soldier.png {folderName}\\res{i}.bmp -l:50 -s:1 -f:50 -c:5 -b:0 -w:255 -a:{i} -st:None -lt:Line -mt:Ridge -bt:GDIPlus -inv".Split(' ');
				Logic.ProcessByArgs(arguments);
			}
		}
	}
}
