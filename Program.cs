using System;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using System.IO;
using System.Linq;

namespace VideoToASCII
{
	class Saving
	{
		public static string PictureToASCII (string path)
		{
			// Initialization
			string brightness = " `.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@";
			var image = Image.Load<Rgba32>(path);
			string ASCIIImage = "";

			int xCrop = Convert.ToInt16(Math.Ceiling(image.Width / Console.WindowWidth * 1d + 0.5));
			int yCrop = Convert.ToInt16(Math.Ceiling(image.Height / Console.WindowHeight * 1d + 0.5));
			int crop = new int[] {xCrop, yCrop}.Max();
			if (crop == 0) crop = 1;
			
			for (int y = 0; y < image.Height; y += crop)
			{
				for (int x = 0; x < image.Width; x += crop)
				{
					double middleColor = 0;
					for (int j = 0; j < crop; j++)
					{
						for (int i = 0; i < crop; i++)
						{
							if (x + j + 1 <= image.Width && y + i + 1 <= image.Height)
							{
								middleColor = middleColor + ((image[x + j, y + i].R + image[x + j, y + i].G + image[x + j, y + i].B) / 3);
						
							}
						}
					}
					middleColor = middleColor / (crop * crop);
					
					ASCIIImage += new string(brightness[Convert.ToInt16(middleColor / 255 * (brightness.Length - 1))], 2);
				}
				ASCIIImage += "\n";
			}
			return ASCIIImage;
		}
		public static void Save()
		{
			// Deleting files
			File.Delete("text.txt");
			if (Directory.Exists("images"))
			{
				Directory.Delete("images", true);
			}
			Directory.CreateDirectory("images");

			// Path to video
			Console.WriteLine("Enter video filename");
			string? videoID = Console.ReadLine();
			string path = videoID + ".mp4";

			// Deleting video.txt file
			File.Delete("text" + videoID + ".txt");

			// FPS
			Console.WriteLine("Enter FPS (20-30 is recommended):");
			int FPS = Convert.ToInt16(Console.ReadLine());
			File.AppendAllText("text" + videoID + ".txt", FPS + "ё");
			
			// Converting video to images
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = false;
			startInfo.UseShellExecute = true;
			startInfo.FileName = "ffmpeg";
			startInfo.WindowStyle = ProcessWindowStyle.Minimized;
			startInfo.Arguments = "-i " + path + " images/image%01d.png -r " + FPS;
			Process.Start(startInfo).WaitForExit();
			
			// Converting images to text
			Console.WriteLine("Getting the file count...");
			int imagesCount = Directory.GetFiles("images", "*", SearchOption.TopDirectoryOnly).Length;
			Console.WriteLine("Appending ASCII text...");
			for (int i = 1; i <= imagesCount; i++)
			{	
				File.AppendAllText("text" + videoID + ".txt", PictureToASCII("images/image" + i + ".png") + "ё");

				// Progressbar
				string beforeProgressBar = "Converting image " + i + " to text ";
				int progressBarLength = Console.WindowWidth - (beforeProgressBar.Length + 3 + 4);
				Console.WriteLine(beforeProgressBar + "[" + new string('#', Convert.ToInt16((double)i / imagesCount * progressBarLength)) + new string ('.', Convert.ToInt16(progressBarLength - (double)i / imagesCount * progressBarLength)) + "] " + Convert.ToInt16((double)i / imagesCount * 100) + "%");
			}
		}
	}


	class Loading
	{
		public static void Load()
		{
			// Path to video
			Console.WriteLine("Enter video filename");
			string? videoID = Console.ReadLine();

			// Initialization
			Console.WriteLine("Loading...");
			string[] ASCIIVideo = File.ReadAllText("text" + videoID + ".txt").Split("ё");
			int newLinesToFit = Console.WindowHeight - ASCIIVideo[1].Split("\n").Length;
			if (newLinesToFit < 1)
			{
				newLinesToFit = 1;
			}
			Console.WriteLine("Loaded. FPS is " + ASCIIVideo[0] + ". Confirm playing a video.");
			Console.ReadKey();

			// Playing a video
			for (int i = 1; i < ASCIIVideo.Length; i++)
			{
				Console.Write(ASCIIVideo[i] + new string('\n', newLinesToFit));
				Thread.Sleep(Convert.ToInt16(1d / Convert.ToDouble(ASCIIVideo[0]) * 1000d));
			}
		}	
	}


	class Program
	{
		static void Main()
		{
			Console.WriteLine("Would you like to save(s) or load(l) a video?");
			string? answer = Console.ReadLine();
			if (answer.ToUpper() == "S")
			{
				Saving.Save();
				Loading.Load();
			}
			if (answer.ToUpper() == "L")
			{
				Loading.Load();	
			}
		}	
	}
}
