using Sharprompt;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NFCI
{
    internal class Methods
    {
        internal static void BitrateHelp()
        {
            Console.Clear();//Clears console and prints information about variable and constant bitrate.
            Console.WriteLine("\t\t\t\tVariable bitrate:");
            Console.WriteLine("The bitrate can dynamically change and can increase or decrease per frame,");
            Console.WriteLine("depending on how much dedicated data ffmpeg believes each frame needs.");
            Console.WriteLine("Variable bitrate is generally better and should always be used if possible,");
            Console.WriteLine("especially if there's a mix of fast and slow moving action in the video.");
            Console.WriteLine("However if a certain filesize is required, the option 'ABR (Average Bitrate)'");
            Console.WriteLine("or constant bitrate may be prefered.\n\n");
            Console.WriteLine("\t\t\t\tConstant bitrate:");
            Console.WriteLine("The bitrate is more or less constant and each frame gets the same bitrate");
            Console.WriteLine("regardless of how much it requires.");
            Console.WriteLine("This can end up wasting valuable data if there is barely any movement in the video,");
            Console.WriteLine("while making faster or darker parts of the video look poorly, if the bitrate isn't high enough.");
            Console.WriteLine("This option is only prefered if a certain filesize is required\n");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
        internal static void DetectFFmpeg() //creates a function named DetectFFmpeg used for, well... detecting if ffmpeg is on the system
        {
            try
            {
                using Process ffmpeg = new();
                ffmpeg.StartInfo.FileName = "ffmpeg";
                ffmpeg.StartInfo.CreateNoWindow = true;
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.RedirectStandardError = true;
                _ = ffmpeg.Start();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("ffmpeg detected!");
                Console.ResetColor();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Warning! no ffmpeg program was detected.");
                Console.ResetColor();
                Console.WriteLine("\nPlease either download and put ffmpeg in {0} or add it's location to PATH", Directory.GetCurrentDirectory());
                Console.WriteLine("ffmpeg is a requirement, and this cannot be skipped.");
                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine("oops... an unknown error occured. Please create an issue on Github with the message below attached\nas well as what you might've done to cause it\n");
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }
        internal static (string, string) GetOS() //creates custom function named GetOS for detecting the OS and setting the Regex and OS variable accordingly. Also tells the function it'll return two string type variables
        {
            string RegexString; //create empty string variable to allow the OS detection and Regex to use it
            string FfmpegPass1Output;
            Console.ForegroundColor = ConsoleColor.Green;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //runs if the OS is Windows
            {
                Console.WriteLine("Windows based OS detected!");
                RegexString = @"[<>*?\/\|""]";
                FfmpegPass1Output = "NUL";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) //runs if the OS is Linux
            {
                Console.WriteLine("Linux based OS detected!");
                RegexString = "\"";
                FfmpegPass1Output = "/dev/null";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) //runs if the OS is MacOS
            {
                Console.WriteLine("MacOS detected!");
                RegexString = "\"";
                FfmpegPass1Output = "/dev/null";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) //runs if the OS is FreeBSD
            {
                Console.WriteLine("FreeBSD detected!");
                RegexString = "\"";
                FfmpegPass1Output = "/dev/null";
            }
            else //runs if the OS type could not be detected, or is unknown
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error detecting OS! This shouldn't be an issue unless you're running Windows");
                RegexString = "\"";
                FfmpegPass1Output = "/dev/null";
            }
            Console.ResetColor();
            return (RegexString, FfmpegPass1Output); //returns with two variables, one containing the appropriate Regex, and the other the detected OS type
        }
        internal static void ShowFfmpegPass2(string FfmpegPass2) //creates custom function named ShowCommand. Used for displaying the current command used for ffmpeg. Thanks to a certain individual for teaching me this!
        {
            Console.Clear();
            Console.Write("\nYour current ffmpeg commandline looks like the following: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("ffmpeg{0}", FfmpegPass2);
            Console.ResetColor();
            Console.WriteLine("\n");
        }
        internal static void VariableBitrateHelp()
        {
            do
            {
                Console.Clear();
                var BitrateHelpOption = Prompt.Select("Please select which bitrate option you'd like to read about", new[] { "CRF (Constant Quality)", "ABR (Average Bitrate)", "CQ (Constrained Quality)", "Return" }); //Uses Sharprompt to select which bitrate option the user would like to read about
                if (BitrateHelpOption == "CRF (Constant Quality)")
                {
                    Console.Clear();
                    Console.WriteLine("CRF is the most recommended option. It is the best option when it comes to getting the best quality,");
                    Console.WriteLine("as long as the CRF value provided is sufficient. Based on a quality value between 0 and 63, ffmpeg");
                    Console.WriteLine("increases or decreases the bitrate per frame as it attempts to keep it to the level of quality selected.");
                    Console.WriteLine("This in turn should mean that every frame is neither overfed, nor underfed with data, if the CRF value is low enough.");
                    Console.WriteLine("Generally a lower value around 25 is good for animations and other digital art.");
                    Console.WriteLine("While a higher value around 35 is more suited for real-life scenes, due to the added \"noise\" to the video\n");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                    continue;
                }
                else if (BitrateHelpOption == "ABR (Average Bitrate)")
                {
                    Console.Clear();
                    Console.WriteLine("Average Bitrate will attempt to reach the specified bitrate. This is only recommended if you're trying to reach a certain filesize.");
                    Console.WriteLine("As there isn't any quality control, this can lead to either videos with poor quality, or a bloated file size with useless data");
                    Console.WriteLine("If you are still thinking of using ABR, a good idea is to first examine the video for keypoints.");
                    Console.WriteLine("Lots of dark/poorly lit rooms/areas in the video? Fast moving action where there's rarely, or never, repeating frames and objects?");
                    Console.WriteLine("Lots of confetti or snow, or other small objects like particles? These would all require a higher bitrate to still look good.");
                    Console.WriteLine("Meanwhile a video with well lit rooms/areas, still or looping animations with lots of repeating frames and objects, would require");
                    Console.WriteLine("less work and lower bitrate to still reach the same quality.");
                    Console.ReadKey();
                    continue;
                }
                else if (BitrateHelpOption == "CQ (Constrained Quality)")
                {
                    Console.Clear();
                    Console.WriteLine("CQ is a mix of CRF and ABR. Like CRF you select a quality value between 0 and 63, however an Average Bitrate is added in to limit");
                    Console.WriteLine("the amount of bits ffmpeg can use. FFmpeg attempts to match the quality with the quality value, however once it reaches the specified");
                    Console.WriteLine("average bitrate it stops increasing the bitrate until it no longer needs a value which exceeds the specified one.");
                    Console.WriteLine("This can be useful if trying to reach a certain quality, while also not wanting to exceed a certain filesize.");
                    Console.ReadKey();
                    continue;
                }
                else if (BitrateHelpOption == "Return")
                {
                    Console.Clear();
                    return;
                }
            } while (true);
        }
    }
}