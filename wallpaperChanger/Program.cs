using System;
using System.Text.Json;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;

namespace setWallpaper
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(
        UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        private static readonly UInt32 SPI_SETDESKWALLPAPER = 0x14;
        private static readonly UInt32 SPIF_UPDATEINIFILE = 0x01;
        private static readonly UInt32 SPIF_SENDWININICHANGE = 0x02;

        static public void SetWallpaper(String path, int stretchType)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue(@"WallpaperStyle", stretchType.ToString()); // 2 is stretched
            key.SetValue(@"TileWallpaper", 0.ToString());

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        class Config
        {
            public string path;
            public string extention;
            public int stretchType;
            public int[] changeTimes;

            public Config(string _path, string _extention, int _stretchType, int[] _changeTimes)
            {
                path = _path;
                extention = _extention;
                stretchType = _stretchType;
                changeTimes = _changeTimes;
            }
        }

        static Config openConfig(string cfgpath)
        {
            if (!File.Exists(cfgpath))
            {
                string path = Path.GetFullPath(@"wallpapers\sl3_black_");
                string extention = ".jpg";
                int stretchType = 10;

                int[] changeTimes = new int[4] { 600, 900, 1800, 2000 };

                Config wallpaperConfig = new Config(path, extention, stretchType, changeTimes);

                using (StreamWriter SW = new StreamWriter(cfgpath))
                {
                    try
                    {
                        SW.WriteLine(JsonConvert.SerializeObject(wallpaperConfig));
                        Console.WriteLine($"{cfgpath} written.");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Error in writing {cfgpath}");
                    }
                }

                return wallpaperConfig;
            }
            else
            {
                try
                {
                    using StreamReader SR = new StreamReader(cfgpath);
                    string json = SR.ReadToEnd();
                    Config wallpaperConfig = JsonConvert.DeserializeObject<Config>(json);
                    return wallpaperConfig;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error reading {cfgpath}, please delete and run this again to create a new one.");
                }
                return null;
            }
        }

        static string checkTime(Config cfg)
        {
            int time = int.Parse(DateTime.Now.ToString("HHmm"));
            string imageExtention = "day";
            if (time >= cfg.changeTimes[3] || time < cfg.changeTimes[0])
            {
                imageExtention = "night";
            }
            else if (time >= cfg.changeTimes[0] && time < cfg.changeTimes[1])
            {
                imageExtention = "sunrise";
            }
            else if (time >= cfg.changeTimes[1] && time < cfg.changeTimes[2])
            {
                imageExtention = "day";
            }
            else if (time >= cfg.changeTimes[2] && time < cfg.changeTimes[3])
            {
                imageExtention = "sunset";
            }

            return imageExtention;
        }

        static void Main(string[] args)
        {

            Config wallpaperConfig = openConfig("wallpaper.cfg");

            string imageExtention = checkTime(wallpaperConfig);

            if (File.Exists(wallpaperConfig.path + imageExtention + wallpaperConfig.extention))
            {
                string wallpaperPath = wallpaperConfig.path + imageExtention + wallpaperConfig.extention;
                SetWallpaper(wallpaperPath, wallpaperConfig.stretchType);
            }
            else
            {
                Console.WriteLine("The file specified in your config doesn't exist, or there's an error in your config");
            }

        }

    }
}
