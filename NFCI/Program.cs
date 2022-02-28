using Sharprompt; //loads the interactable console package
using System.Diagnostics; //Used with running external programs, such as ffmpeg
using System.Runtime.InteropServices; //Used for OS detection
using System.Text.RegularExpressions; //Used with Regex

namespace NFCI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DetectFFmpeg();
            // Sets variables
            (string RegexString, string FfmpegPass1Output) = GetOS(); //saves the returned variables from GetOS function into variables that can be used in main code
            Regex IllegalChar = new(RegexString); //sets regex using the previously set RegexString variable gotten from the GetOS function
            string FfmpegPass1 = "";
            string FfmpegPass2 = ""; //builds the start of the ffmpeg command
            string AudioCodecs = "";
            bool InputValid = false; //sets the validator for the WebM input to false
            byte CRF;
            short AudioBitrate;
            string FileInput, FileOutput;

            do //main menu loop
            {
                Console.WriteLine("\n---------------Welcome---------------");
                Console.WriteLine("                  to                 ");
                Console.WriteLine("  Nicklo's FFmpeg Console Interface\n");
                var UserChoice = Prompt.Select("Please select an option. Use arrow keys and enter to navigate", new[] { "WebM", "MP4", "Info", "Exit" }); //Uses Sharprompt for the main menu's user input

                switch (UserChoice)
                {
                    case "WebM": //Userchoice for converting to WebM
                        {
                            do //WebM menu loop
                            {
                                Console.Clear();
                                var WebMUserChoice = Prompt.Select("You have selected WebM. Please select a bitrate option", new[] { "Variable", "Constant", "Help" }); //Uses Sharprompt for the WebM's bitrate options
                                switch (WebMUserChoice)
                                {
                                    case "Help": //UserChoice for displaying a help section about bitrate options
                                        {
                                            Console.Clear();//Clears console and prints information about variable and constant bitrate.
                                            Console.WriteLine("\t\t\t\tVariable bitrate:");
                                            Console.WriteLine("The bitrate is variable (can dynamically change) and can increase or decrease per frame,");
                                            Console.WriteLine("depending on how much dedicated data ffmpeg believes each frame needs.");
                                            Console.WriteLine("Variable bitrate is generally better and should always be used if possible,");
                                            Console.WriteLine("especially if there's a mix of fast and slow moving action in the video.");
                                            Console.WriteLine("However if a certain filesize is required, constant bitrate may be prefered.");
                                            Console.WriteLine("For variable bitrate the option 'CRF' is used, which uses numbers between 0 and 63,");
                                            Console.WriteLine("where higher means lower quality, and lower means higher quality. A value between 18 and 28");
                                            Console.WriteLine("is often sufficient, though requires adjusting depending on the videos context.\n\n");
                                            Console.WriteLine("\t\t\t\tConstant bitrate:");
                                            Console.WriteLine("The bitrate is constant and each frame gets the same bitrate regardless of how much it requires.");
                                            Console.WriteLine("This can end up wasting valuable data if there is barely any movement in the video,");
                                            Console.WriteLine("while making faster parts of the video look poorly, if the bitrate isn't high enough.");
                                            Console.WriteLine("This option is only prefered if a certain filesize is required\n");
                                            Console.WriteLine("Press any key to continue");
                                            Console.ReadKey();
                                            break;
                                        }
                                    case "Variable":
                                    case "Constant": //case for both the Variables and Constant choice
                                        {
                                            Console.WriteLine("\n");
                                            var DisableAudio = Prompt.Confirm("Disable audio? Press the Y or N key, or enter to default to No", defaultValue: false);//asks with Sharprompt if the user wishes to disable audio.

                                            do
                                            {
                                                InputValid = false;
                                                Console.WriteLine("\n");
                                                FileInput = Prompt.Input<string>("Please select the video file. You can drag and drop the file to complete the filename and location");
                                                try
                                                {
                                                    File.OpenRead(FileInput.Trim('"'));
                                                    FfmpegPass2 = FfmpegPass2 + ' ' + "-i" + ' ' + FileInput; //build the command variable so it can be used outside
                                                    FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput;
                                                    if (DisableAudio == true) //adds an additional flag for removing audio tracks
                                                    {
                                                        FfmpegPass2 = FfmpegPass2 + ' ' + "-an";
                                                        FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput;
                                                    }
                                                    InputValid = true;
                                                }
                                                catch (NullReferenceException)
                                                {
                                                    Console.WriteLine("\nInput cannot be empty!");
                                                }
                                                catch (UnauthorizedAccessException)
                                                {
                                                    Console.WriteLine("\nFile not found. Either the program doesn't have access, or you specified a directory instead of file.");
                                                }
                                                catch (ArgumentException)
                                                {
                                                    Console.WriteLine("\nInput cannot be empty, only have spaces, or contain illegal characters such as <>*?\\|\"");
                                                }
                                                catch (PathTooLongException)
                                                {
                                                    Console.WriteLine("\nThe specified input path is too long, please move the file to a different folder.");
                                                }
                                                catch (DirectoryNotFoundException)
                                                {
                                                    Console.WriteLine("\nDirectory couldn't be found. Is this on a network drive perhaps?");
                                                }
                                                catch (FileNotFoundException)
                                                {
                                                    Console.WriteLine("\nFile could not be found. Did you enter the name correctly?");
                                                }
                                                catch (NotSupportedException)
                                                {
                                                    Console.WriteLine("\nInput is invalid format. If the filename is correct, please rename the file to a more conventional name.");
                                                }
                                                catch (IOException)
                                                {
                                                    Console.WriteLine("\nFile error. File could not be accessed or an invalid name/path was given.");
                                                }


                                                //if (string.IsNullOrEmpty(FileInput) || string.IsNullOrEmpty(FileInput.Trim('"')))//checks if input is empty or null and fails if true.
                                                //{
                                                //    Console.WriteLine("\nInput cannot be empty!");
                                                //}

                                                //else if (IllegalChar.IsMatch(FileInput.Trim('"')))//checks if input contains any of the previously mentioned illegal characters, and fails if true.
                                                //{
                                                //    Console.WriteLine("\nInput contains invalid characters, please refrain from using any of the following: <>*?/|\"");
                                                //}
                                                //else if (Path.HasExtension(FileInput.Trim('"')) == false)//checks if input is contains a file extension, and fails if false.
                                                //{
                                                //    Console.WriteLine("\nInput requires a file extension at the end!");
                                                //}
                                                //else if (File.Exists(FileInput.Trim('"')) == false)//checks if input is actually accesible or exists, and fails if false.
                                                //{
                                                //    Console.WriteLine("\nInput file cannot be found or this program does not have permission to access it. Please make sure you typed it correctly.");
                                                //}
                                                //else if (FileInput.Any(char.IsWhiteSpace))//checks for double quotes at the start and end of the string, aswell as if there's spaces, and fails if false.
                                                //{
                                                //    FileInput = FileInput.Replace("\"", "");
                                                //    FileInput = '"' + FileInput + '"'; //adds double quotes
                                                //    InputValid = true; //sets InputValid to true if all other conditions are met, and exits the DoWhile loop.
                                                //    FfmpegPass2 = FfmpegPass2 + ' ' + "-i" + ' ' + FileInput; //build the command variable so it can be used outside
                                                //    FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput;
                                                //    if (DisableAudio == true) //adds an additional flag for removing audio tracks
                                                //    {
                                                //        FfmpegPass2 = FfmpegPass2 + ' ' + "-an";
                                                //        FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput;
                                                //    }
                                                //}
                                                //else
                                                //{
                                                //    InputValid = true; //sets InputValid to true if all other conditions are met, and exits the DoWhile loop.
                                                //    FfmpegPass2 = FfmpegPass2 + ' ' + "-i" + ' ' + FileInput; //build the command variable so it can be used outside
                                                //    FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput;
                                                //    if (DisableAudio == true) //adds an additional flag for removing audio tracks if true
                                                //    {
                                                //        FfmpegPass2 = FfmpegPass2 + ' ' + "-an";
                                                //        FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput;
                                                //    }
                                                //}
                                            } while (InputValid == false); //stays in the input loop as long as it's not valid

                                            FfmpegPass2 = FfmpegPass2 + ' ' + "-pass" + ' ' + "2" + ' ' + "-row-mt" + ' ' + "1";
                                            FfmpegPass1 = FfmpegPass1 + ' ' + "-pass" + ' ' + "1" + ' ' + "-row-mt" + ' ' + "1";

                                            ShowFfmpegPass2(FfmpegPass2); //prints current ffmpeg commandline
                                            var VideoCodecs = Prompt.Select("Please select the video codecs to use. 'vp9' is prefered as it's newer and more efficient", new[] { "libvpx-vp9", "libvpx-vp8" }); //used for selecting video codecs
                                            FfmpegPass2 = FfmpegPass2 + ' ' + "-c:v" + ' ' + VideoCodecs;
                                            FfmpegPass1 = FfmpegPass1 + ' ' + "-c:v" + ' ' + VideoCodecs + ' ' + "-an";
                                            if (DisableAudio == false) //ask for audio codec too if WebMDisableAudio is false
                                            {
                                                ShowFfmpegPass2(FfmpegPass2);
                                                AudioCodecs = Prompt.Select("Please select the audio codecs to use. 'Libopus' is prefered as it's newer and more efficient", new[] { "libopus", "libvorbis" });//used for selecting audio codecs
                                                FfmpegPass2 = FfmpegPass2 + ' ' + "-c:a" + ' ' + AudioCodecs;
                                                ShowFfmpegPass2(FfmpegPass2);
                                            }

                                            
                                            if (WebMUserChoice == "Variable") //menu if the Variable bitrate option was chosen
                                            {
                                                ShowFfmpegPass2(FfmpegPass2);
                                                CRF = Prompt.Input<byte>("Please chose a CRF value between 0 and 63"); //Used for selecting a CRF value
                                                if (CRF > 63) //checks if CRF value is 63 or below
                                                {
                                                    Console.WriteLine("CRF values higher than 63 is not allowed. Please try again."); //complains if CRF value is above 63
                                                    Console.ReadKey();
                                                }
                                                else
                                                {
                                                    FfmpegPass2 = FfmpegPass2 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + "0";
                                                    FfmpegPass1 = FfmpegPass1 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + "0";
                                                }
                                            }
                                            else if (WebMUserChoice == "Constant") //menu if the Constant bitrate option was chosen
                                            {

                                            }

                                            ShowFfmpegPass2(FfmpegPass2);

                                            if (DisableAudio == false) //checks if audio is set to be disabled
                                            {
                                                AudioBitrate = Prompt.Input<short>("Please specify the audio bitrate in kilobytes e.g. 192"); //prompts for audio bitrate
                                                FfmpegPass2 = FfmpegPass2 + ' ' + "-b:a" + ' ' + AudioBitrate + "k";
                                                ShowFfmpegPass2(FfmpegPass2);
                                            }

                                            var pix_fmt = Prompt.Select("Please select the chroma subsampling. 'yuv420p' is the most compatible and prefered.", new[] { "yuv420p", "yuv422p", "yuv444p" }); //prompts for the pixel format/chroma subsampling
                                            FfmpegPass2 = FfmpegPass2 + ' ' + "-pix_fmt" + ' ' + pix_fmt;
                                            FfmpegPass1 = FfmpegPass1 + ' ' + "-pix_fmt" + ' ' + pix_fmt;
                                            ShowFfmpegPass2(FfmpegPass2);

                                            var UseMetadata = Prompt.Confirm("Would you like to add metadata to the file? Defaults to No", defaultValue: false);
                                            if (UseMetadata == true)
                                            {
                                                var UseMetadataTitle = Prompt.Confirm("Would you like to add a metadata title? Defaults to No", defaultValue: false);
                                                if (UseMetadataTitle)
                                                {
                                                    var MetadataTitle = Prompt.Input<string>("Please type or paste the metadata title");
                                                    if (MetadataTitle.Any(Char.IsWhiteSpace))
                                                    {
                                                        MetadataTitle = MetadataTitle.Replace("\"", "");
                                                        MetadataTitle = '"' + MetadataTitle + '"';
                                                    }
                                                    FfmpegPass2 = FfmpegPass2 + ' ' + "-metadata title=" + MetadataTitle;
                                                }
                                                var UseMetadataURL = Prompt.Confirm("Would you like to add a metadata URL? Defaults to No", defaultValue: false);
                                                if (UseMetadataURL)
                                                {
                                                    var MetadataURL = Prompt.Input<string>("Please type or paste the metadata URL");
                                                    if (MetadataURL.Any(char.IsWhiteSpace))
                                                    {
                                                        MetadataURL = MetadataURL.Replace("\"", "");
                                                        MetadataURL = '"' + MetadataURL + '"';
                                                    }
                                                    FfmpegPass2 = FfmpegPass2 + ' ' + "-metadata url=" + MetadataURL;
                                                }
                                            }
                                            ShowFfmpegPass2(FfmpegPass2);

                                            var deadline = Prompt.Select("Please select the deadline. 'best' is recommended, however 'good' is fine too. Avoid 'realtime' if possible", new[] { "best", "good", "realtime" });
                                            FileOutput = Path.ChangeExtension(FileInput, ".webm");
                                            if (FileOutput.Any(char.IsWhiteSpace))
                                            {
                                                FileOutput = FileOutput.Replace("\"", "");
                                                FileOutput = '"' + FileOutput + '"'; //adds double quotes
                                            }
                                            FfmpegPass2 = FfmpegPass2 + ' ' + "-deadline" + ' ' + deadline + ' ' + FileOutput;
                                            FfmpegPass1 = FfmpegPass1 + ' ' + "-deadline" + ' ' + deadline + ' ' + "-f" + ' ' + "null" + ' ' + FfmpegPass1Output;
                                            ShowFfmpegPass2(FfmpegPass2);

                                            var UseTwoPass = Prompt.Confirm("Use Two-pass encoding? This will greatly increase quality, but also requires some extra encoding time as it essentially does it twice", defaultValue: true);
                                            try
                                            {
                                                if (UseTwoPass)
                                                {
                                                    Process FFmpeg1 = Process.Start("ffmpeg",FfmpegPass1);
                                                    FFmpeg1.WaitForExit();
                                                    Process FFmpeg2 = Process.Start("ffmpeg",FfmpegPass2);
                                                    FFmpeg2.WaitForExit();
                                                }
                                                else
                                                {
                                                    Process FFmpeg2 = Process.Start("ffmpeg",FfmpegPass2);
                                                    FFmpeg2.WaitForExit();
                                                }
                                            }
                                            catch (System.ComponentModel.Win32Exception e)
                                            {
                                                Console.WriteLine("Looks like the program failed to run ffmpeg. This is either due to it somehow missing\nor the parameters passed to ffmpeg being badly formatted.\nPlease create an issue on Github and provide the below outputs:");
                                                Console.WriteLine("\nUse Two-pass? {0}",UseTwoPass);
                                                Console.WriteLine("\nFFmpeg pass 1: {0}",FfmpegPass1);
                                                Console.WriteLine("\nFFmpeg pass 2: {0}", FfmpegPass2);
                                                Console.WriteLine("\n{0}",e.Message);
                                            }
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("\nthis is the end");
                                            Console.ResetColor();
                                            Console.ReadKey();
                                            break;
                                        }
                                }
                            } while (true);
                        }
                    case "MP4": //Userchoice for converting to MP4
                        {
                            Console.Clear();
                            Console.WriteLine("This feature is work in progress. Please come back later...");
                            Console.WriteLine("\nPress any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                            break;
                        }
                    case "Info": //Displays a small info box about the program
                        {
                            Console.Clear();
                            Console.WriteLine("This program was written with frustration and the will to learn");
                            Console.WriteLine("as well as with the help from a few lovely friends.");
                            Console.WriteLine("Sharprompt was also used to create the interactable menu's");
                            Console.WriteLine("and all credit goes to it's respective developer(s)");
                            Console.WriteLine("at https://github.com/shibayan/Sharprompt \n");
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                            break;
                        }
                    case "Exit": //Exits the program with code 0.
                        {
                            Environment.Exit(0);
                            break;
                        }
                    default: //Not really needed with the Sharprompt interface, but still nice to have.
                        {
                            Console.WriteLine("{0} is not an option", UserChoice);
                            break;
                        }
                }
            } while (true);
        }
        static void ShowFfmpegPass2(string FfmpegPass2) //creates custom function named ShowCommand. Used for displaying the current command used for ffmpeg. Thanks to a certain individual for teaching me this!
        {
            Console.Clear();
            Console.Write("\nYour current ffmpeg commandline looks like the following: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("ffmpeg {0}",FfmpegPass2);
            Console.ResetColor();
            Console.WriteLine("\n");
        }

        static (string,string) GetOS() //creates custom function named GetOS for detecting the OS and setting the Regex and OS variable accordingly. Also tells the function it'll return two string type variables
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
            return (RegexString,FfmpegPass1Output); //returns with two variables, one containing the appropriate Regex, and the other the detected OS type
        }
        static void DetectFFmpeg() //creates a function named DetectFFmpeg used for, well... detecting if ffmpeg is on the system
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
                Console.WriteLine("\nPlease either download and put ffmpeg in {0} or add it's location to PATH",Directory.GetCurrentDirectory());
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
    }
}