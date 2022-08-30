using RidgeDrawer;
using System;
using System.IO;

namespace SequenceGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("press Y to start: ");
			if (Console.ReadLine().ToUpper() != "Y")
				return;

			string folderName = @"D:\Downloads\seq";
			Directory.CreateDirectory(folderName);

			for (int i = 1; i < 10; i += 1)
			{
				string[] arguments = $"..\\soldier.png {folderName}\\res{i}.bmp -l:{i} -s:1 -f:50 -c:1 -b:0 -w:255 -a:0 -st:Antialias -lt:Curve -mt:Ridge -bt:GDIPlus -fi -ptx:960 -pty:540".Split(' ');
				Logic.ProcessByArgs(arguments);
				Console.WriteLine(i);
			}
		}
	}
}
