using System;
using System.Net;
using System.Runtime.InteropServices;

public class SetWallpaper
{
	//private string filename {  get; set; }
	const int SPI_SETDESKWALLPAPER = 20;
	const int SPIF_UPDATEINIFILE = 0x01;
	const int SPIF_SENDWININICHANGE = 0x02;

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

	//function change wallpapers
	public static void changeWallpaper(string filename)
	{
		SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, filename, SPIF_UPDATEINIFILE);
	}

}