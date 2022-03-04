using Sharprompt; //loads the interactable console package
using System.Diagnostics; //Used with running external programs, such as ffmpeg
using System.Runtime.InteropServices; //Used for OS detection
using System.Text.RegularExpressions; //Used with Regex

namespace NFCI
{
    internal class MainMenu : Methods
    {
        static void Main()
        {
            DetectFFmpeg();
            // Sets variables
            (string RegexString, string FfmpegPass1Output) = GetOS(); //saves the returned variables from GetOS function into variables that can be used in main code
            Regex IllegalChar = new(RegexString); //sets regex using the previously set RegexString variable gotten from the GetOS function
            //string FfmpegPass1 = ""; //builds the start of the ffmpeg command for pass 2
            //string FfmpegPass2 = ""; //builds the start of the ffmpeg command for pass 2
            //string AudioCodecs = "";
            //byte CRF; //Variable used for Constant Quality bitrate mode
            //string ABR; //variable used for Average Bitrate mode
            //string CBR; //variable used for Constant Bitrate mode
            //short AudioBitrate;
            //string FileInput = "", FileOutput;
            //bool GoodExit = true;
            Regex GoodBitrate = new(@"^[0-9]+(?:k|m|K|M)?$"); //create regex only allowing numbers, and k and m as well as in uppercase

            do //main menu loop
            {
                Console.WriteLine("\n---------------Welcome---------------");
                Console.WriteLine("                  to                 ");
                Console.WriteLine("  Nicklo's FFmpeg Console Interface\n");
                var UserChoice = Prompt.Select("Please select an option. Use arrow keys and enter to navigate", new[] { "WebM", "MP4", "Info", "Exit" }); //Uses Sharprompt for the main menu's user input

                if (UserChoice == "WebM")
                {
                    WebM.ConvertToWebM();
                }
                else if (UserChoice == "MP4")
                {
                    Console.Clear();
                    Console.WriteLine("This feature is work in progress. Please come back later...");
                    Console.WriteLine("\nPress any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                }
                else if (UserChoice == "Info")
                {
                    Console.Clear();
                    Console.WriteLine("This program was written with frustration and the will to learn");
                    Console.WriteLine("as well as with a lot of help from a few lovely friends.");
                    Console.WriteLine("Sharprompt was also used to create the interactable menu's");
                    Console.WriteLine("and all credit goes to it's respective developer(s)");
                    Console.WriteLine("at https://github.com/shibayan/Sharprompt \n");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                }
                else if (UserChoice == "Exit")
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("{0} is not an option", UserChoice);
                }
            } while (true);
        }
    }
}