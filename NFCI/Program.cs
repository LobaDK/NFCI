using Sharprompt; //loads the interactable console package
using System.Diagnostics; //Used with running external programs, such as ffmpeg
using System.Runtime.InteropServices; //Used for OS detection
using System.Text.RegularExpressions; //Used with Regex

namespace NFCI
{
    internal class Program
    {
        static void Main()
        {
            DetectFFmpeg();
            // Sets variables
            (string RegexString, string FfmpegPass1Output) = GetOS(); //saves the returned variables from GetOS function into variables that can be used in main code
            Regex IllegalChar = new(RegexString); //sets regex using the previously set RegexString variable gotten from the GetOS function
            string FfmpegPass1 = ""; //builds the start of the ffmpeg command for pass 2
            string FfmpegPass2 = ""; //builds the start of the ffmpeg command for pass 2
            string AudioCodecs = "";
            byte CRF; //Variable used for Constant Quality bitrate mode
            string ABR; //variable used for Average Bitrate mode
            short AudioBitrate;
            string FileInput = "", FileOutput;
            bool GoodExit = true;

            do //main menu loop
            {
                Console.WriteLine("\n---------------Welcome---------------");
                Console.WriteLine("                  to                 ");
                Console.WriteLine("  Nicklo's FFmpeg Console Interface\n");
                var UserChoice = Prompt.Select("Please select an option. Use arrow keys and enter to navigate", new[] { "WebM", "MP4", "Info", "Exit" }); //Uses Sharprompt for the main menu's user input

                if (UserChoice == "WebM")
                {
                    do
                    {
                        Console.Clear();
                        var DisableAudio = Prompt.Confirm("Disable audio? Press the Y or N key, or enter to default to No", defaultValue: false); //Uses Sharprompt to ask the user if they wanna disable audio
                        do
                        {
                            Console.WriteLine("\n");
                            FileInput = Prompt.Input<string>("Please select the video file. You can drag and drop the file to complete the filename and location"); //Uses sharprompt to ask for the input file
                            if (FileInput == null)
                            {
                                Console.WriteLine("Inout cannot be empty!");
                                continue;
                            }
                            FileInput = FileInput.Trim(); //removes any whitespaces from the start and end of the file. Mainly used because some Linux distro's
                                                              //adds a trailing whitespace when drag & dropping a file into the terminal, to complete the path

                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //removes any double quotes if the detected OS is Windows
                            {
                                FileInput = FileInput.Replace("\"", "");
                            }
                            else
                            {
                                FileInput = FileInput.Replace("'", ""); //removes any single quotes if the detected OS is anything but Windows
                            }
                            try
                            {
                                File.OpenRead(FileInput); //attempt to open the file. If it fails the program will catch the exception and the input won't be valid
                                FileInput = '"' + FileInput + '"'; //adds double quotes back to the input file
                                FfmpegPass2 = FfmpegPass2 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput; //build the command variable so it can be used outside for the second pass
                                FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput; //build the command variable so it can be used outside for the first pass
                                if (DisableAudio == true) //adds an additional flag for removing audio tracks
                                {
                                    FfmpegPass2 = FfmpegPass2 + ' ' + "-y" + ' ' + "-an";
                                    FfmpegPass1 = FfmpegPass1 + ' ' + "-y" + ' ' + "-i" + ' ' + FileInput; //doesn't add flag yet, will be added when the video codecs is chosen
                                }
                                break;
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

                        } while (true); //stays in the input loop as long as it's not valid

                        FfmpegPass2 = FfmpegPass2 + ' ' + "-row-mt" + ' ' + "1";
                        FfmpegPass1 = FfmpegPass1 + ' ' + "-row-mt" + ' ' + "1";

                        ShowFfmpegPass2(FfmpegPass2); //prints current ffmpeg commandline
                        var VideoCodecs = Prompt.Select("Please select the video codecs to use. 'vp9' is prefered as it's newer and more efficient", new[] { "libvpx-vp9", "libvpx-vp8" }); //Uses Sharprompt to select videos codecs
                        FfmpegPass2 = FfmpegPass2 + ' ' + "-c:v" + ' ' + VideoCodecs;
                        FfmpegPass1 = FfmpegPass1 + ' ' + "-c:v" + ' ' + VideoCodecs + ' ' + "-an"; //audio is not needed on the first pass
                        if (DisableAudio == false) //ask for audio codec too if WebMDisableAudio is false
                        {
                            ShowFfmpegPass2(FfmpegPass2);
                            AudioCodecs = Prompt.Select("Please select the audio codecs to use. 'Libopus' is prefered as it's newer and more efficient", new[] { "libopus", "libvorbis" });//Uses Sharprompt to select audio codecs
                            FfmpegPass2 = FfmpegPass2 + ' ' + "-c:a" + ' ' + AudioCodecs;
                            ShowFfmpegPass2(FfmpegPass2);
                        }

                        do
                        {
                            ShowFfmpegPass2(FfmpegPass2);
                            var BitrateMode = Prompt.Select("Please select the bitrate mode. A help option is also available", new[] { "Variable", "Constant", "Help" }); //Uses Sharprompt to select the bitrate mode
                            if (BitrateMode == "Help")
                            {
                                BitrateHelp();
                            }
                            if (BitrateMode == "Variable") //menu if the Variable bitrate option was chosen
                            {
                                do
                                {
                                    ShowFfmpegPass2(FfmpegPass2);
                                    var BitrateOption = Prompt.Select("Please select a bitrate option. Reading the 'Help' section is recommended if these options confuse you", new[] { "CRF (Constant Quality)", "ABR (Average Bitrate)", "CQ (Constrained Quality)", "Help" }); //Uses Sharprompt to select the bitrate option

                                    if (BitrateOption == "CRF (Constant Quality)")
                                    {
                                        do
                                        {
                                            Console.WriteLine("\n");
                                            CRF = Prompt.Input<byte>("Please chose a CRF value between 0 and 63"); //Used for selecting a CRF value
                                            if (CRF > 63) //checks if CRF value is above 63
                                            {
                                                Console.WriteLine("CRF values higher than 63 is not allowed. Please try again."); //complains if CRF value is above 63
                                                Console.ReadKey();
                                            }
                                            else
                                            {
                                                FfmpegPass2 = FfmpegPass2 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + "0";
                                                FfmpegPass1 = FfmpegPass1 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + "0";
                                                break;
                                            }
                                        } while (true);
                                    }
                                    else if (BitrateOption == "ABR (Average Bitrate)")
                                    {
                                        do
                                        {
                                            Console.WriteLine("\n");
                                            ABR = Prompt.Input<string>("Please specify the bitrate to be used in megabits. Kilobits can also be used by putting a 'k' at the end");
                                            if (ABR == null) //checks if ABR is empty/null
                                            {
                                                Console.WriteLine("Bitrate cannot be empty!");
                                            }
                                            else if (ABR.Any(char.IsDigit) && ABR.Count(char.IsLetter) < 2 && ABR.EndsWith("k", StringComparison.OrdinalIgnoreCase) && !ABR.Any(char.IsWhiteSpace) || ABR.Any(Char.IsDigit) && ABR.Count(char.IsLetter) < 2 && ABR.EndsWith("m", StringComparison.OrdinalIgnoreCase) && !ABR.Any(char.IsWhiteSpace)) //checks if the bitrate contains digits, less than 2 letters, if it ends with k or m, to ignore case sensitivity and if there's any whitespaces
                                            {
                                                FfmpegPass2 = FfmpegPass2 + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                                FfmpegPass1 = FfmpegPass1 + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                                break;
                                            }
                                            else if (ABR.Any(char.IsDigit) && !ABR.Any(char.IsLetter) && !ABR.Any(char.IsWhiteSpace)) //checks if the bitrate contains digits, but has no letters or whitespaces
                                            {
                                                FfmpegPass2 = FfmpegPass2 + ' ' + "-b:v" + ' ' + ABR + "M";
                                                FfmpegPass1 = FfmpegPass1 + ' ' + "-b:v" + ' ' + ABR + "M";
                                                break;
                                            }
                                            else //bitrate must be invalid if the previous 2 were not true
                                            {
                                                Console.WriteLine("{0} is not a valid bitrate! Only the letter M and K are allowed after, and no spaces are allowed.", ABR);
                                            }
                                        } while (true);
                                    }
                                    else if (BitrateOption == "CQ (Constrained Quality)")
                                    {
                                        do
                                        {
                                            Console.WriteLine("\n");
                                            CRF = Prompt.Input<byte>("Please chose a CRF value between 0 and 63"); //Used for selecting a CRF value
                                            if (CRF > 63) //checks if CRF value is above 63
                                            {
                                                Console.WriteLine("CRF values higher than 63 is not allowed. Please try again."); //complains if CRF value is above 63
                                                Console.ReadKey();
                                                continue;
                                            }
                                            do
                                            {
                                                Console.WriteLine("\n");
                                                ABR = Prompt.Input<string>("Please specify the bitrate to be used in megabits. Kilobits can also be used by putting a 'k' at the end");
                                                if (ABR == null)
                                                {
                                                    Console.WriteLine("Bitrate cannot be empty!");
                                                }
                                                else if (ABR.Any(char.IsDigit) && ABR.Count(char.IsLetter) < 2 && ABR.EndsWith("k", StringComparison.OrdinalIgnoreCase) && !ABR.Any(char.IsWhiteSpace) || ABR.Any(Char.IsDigit) && ABR.Count(char.IsLetter) < 2 && ABR.EndsWith("m", StringComparison.OrdinalIgnoreCase) && !ABR.Any(char.IsWhiteSpace)) //checks if the bitrate contains digits, less than 2 letters, if it ends with k or m, to ignore case sensitivity and if there's any whitespaces
                                                {
                                                    FfmpegPass2 = FfmpegPass2 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                                    FfmpegPass1 = FfmpegPass1 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                                    break;
                                                }
                                                else if (ABR.Any(char.IsDigit) && !ABR.Any(char.IsLetter) && !ABR.Any(char.IsWhiteSpace)) //checks if the bitrate contains digits, but has no letters or whitespaces
                                                {
                                                    FfmpegPass2 = FfmpegPass2 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + ABR + "M";
                                                    FfmpegPass1 = FfmpegPass1 + ' ' + "-crf" + ' ' + CRF + ' ' + "-b:v" + ' ' + ABR + "M";
                                                    break;
                                                }
                                                else //bitrate must be invalid if the previous 2 were not true
                                                {
                                                    Console.WriteLine("{0} is not a valid bitrate! Only the letter M and K are allowed after, and no spaces are allowed.", ABR);
                                                }
                                            } while (true);
                                            break;
                                        } while (true);
                                    }
                                    else if (BitrateOption == "Help")
                                    {
                                        VariableBitrateHelp();
                                        continue;
                                    }
                                    break;
                                }while (true);
                            }
                            else if (BitrateMode == "Constant") //menu if the Constant bitrate option was chosen
                            {

                            }
                            break;
                        } while (true);
                        ShowFfmpegPass2(FfmpegPass2);

                        if (DisableAudio == false) //checks if audio is set to be disabled
                        {
                            AudioBitrate = Prompt.Input<short>("Please specify the audio bitrate in kilobits e.g. 192"); //prompts for audio bitrate
                            FfmpegPass2 = FfmpegPass2 + ' ' + "-b:a" + ' ' + AudioBitrate + "k";
                            ShowFfmpegPass2(FfmpegPass2);
                        }

                        var pix_fmt = Prompt.Select("Please select the chroma subsampling. 'yuv420p' is the most compatible and prefered.", new[] { "yuv420p", "yuv422p", "yuv444p" }); //prompts for the pixel format/chroma subsampling
                        FfmpegPass2 = FfmpegPass2 + ' ' + "-pix_fmt" + ' ' + pix_fmt;
                        FfmpegPass1 = FfmpegPass1 + ' ' + "-pix_fmt" + ' ' + pix_fmt;
                        ShowFfmpegPass2(FfmpegPass2);

                        var UseMetadata = Prompt.Confirm("Would you like to add metadata to the file? Defaults to No", defaultValue: false); //Uses Sharprompt to ask the user if they'd like to add metadata to the file.
                        if (UseMetadata == true)
                        {
                            var UseMetadataTitle = Prompt.Confirm("Would you like to add a metadata title? Defaults to No", defaultValue: false); //Uses Sharprompt to ask if the user wants to add a metadata title
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
                            var UseMetadataURL = Prompt.Confirm("Would you like to add a metadata URL? Defaults to No", defaultValue: false); //Uses Sharprompt to ask if the user wants to add a metadata URL
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

                        var deadline = Prompt.Select("Please select the deadline. Best can make the conversion take 5x as long or more, however yields the best quality-to-bit-ratio", new[] { "best", "good", "realtime" }); //Uses Sharprompt to ask which deadline the user would like to use
                        FileOutput = Path.ChangeExtension(FileInput, ".webm"); //takes the input and changes it's extension to webm, and turns it into the output file
                        if (FileOutput.Any(char.IsWhiteSpace)) //adds double quotes if the output contains any whitespaces
                        {

                            FileOutput = FileOutput.Replace("\"", "");
                            FileOutput = '"' + FileOutput + '"';

                        }
                        FfmpegPass2 = FfmpegPass2 + ' ' + "-deadline" + ' ' + deadline;
                        FfmpegPass1 = FfmpegPass1 + ' ' + "-deadline" + ' ' + deadline;
                        ShowFfmpegPass2(FfmpegPass2);


                        var UseTwoPass = Prompt.Confirm("Use Two-pass encoding? This will greatly increase quality, but also requires some extra encoding time as it essentially does it twice", defaultValue: true); //Uses Sharprompt to ask if the user wants to use two-pass encoding
                        try
                        {
                            if (UseTwoPass)
                            {
                                FfmpegPass2 = FfmpegPass2 + ' ' + "-pass" + ' ' + "2" + ' ' + FileOutput;
                                FfmpegPass1 = FfmpegPass1 + ' ' + "-pass" + ' ' + "1" + ' ' + "-f" + ' ' + "null" + ' ' + FfmpegPass1Output;
                                ShowFfmpegPass2(FfmpegPass2);
                                Console.WriteLine("\n");
                                var CorrectFfmpegPass = Prompt.Confirm("Does this look correct?",defaultValue: true);
                                if (!CorrectFfmpegPass) break; //restarts if the user pressed no
                                Process FFmpeg1 = Process.Start("ffmpeg", FfmpegPass1); //starts the first pass encoding
                                FFmpeg1.WaitForExit(); //waits for the started ffmpeg process to finish
                                Process FFmpeg2 = Process.Start("ffmpeg", FfmpegPass2); //starts the second pass encoding
                                FFmpeg2.WaitForExit(); //waits for the started ffmpeg process to finish
                                if (FFmpeg2.ExitCode != 0) GoodExit = false; //sets GoodExit to false if anything over 0 was returned. FFmpeg returns 0 if no errors were detected, and 1 if something went wrong
                            }
                            else
                            {
                                FfmpegPass2 = FfmpegPass2 + ' ' + FileOutput;
                                ShowFfmpegPass2(FfmpegPass2);
                                Console.WriteLine("\n");
                                var CorrectFfmpegPass = Prompt.Confirm("Does this look correct?", defaultValue: true);
                                if (!CorrectFfmpegPass) break; //restarts if the user pressed no
                                Process FFmpeg2 = Process.Start("ffmpeg", FfmpegPass2); //starts the second pass encoding
                                FFmpeg2.WaitForExit(); //waits for the started ffmpeg process to finish
                                if (FFmpeg2.ExitCode != 0) GoodExit = false; //sets GoodExit to false if anything over 0 was returned. FFmpeg returns 0 if no errors were detected, and 1 if something went wrong
                            }
                        }
                        catch (System.ComponentModel.Win32Exception e) //exception if the program fails to run ffmpeg
                        {
                            Console.WriteLine("Looks like the program failed to run ffmpeg. This is either due to it somehow missing\nOr the parameters passed to ffmpeg being badly formatted.\nPlease create an issue on Github and provide the below information:");
                            Console.WriteLine("\nUse Two-pass? {0}", UseTwoPass);
                            Console.WriteLine("\nFFmpeg pass 1: {0}", FfmpegPass1);
                            Console.WriteLine("\nFFmpeg pass 2: {0}", FfmpegPass2);
                            Console.WriteLine("\n{0}", e.Message);
                        }
                        Console.WriteLine("\n");
                        if (!GoodExit)
                        {
                            Console.WriteLine("FFmpeg have appeared to have ran into a problem. As I can't see the error, please report the issue on github.");
                            Console.WriteLine("Please include any warnings or errors outputted by ffmpeg, usually in red or yellow, as well as the following:");
                            Console.WriteLine("\nUse Two-pass? {0}", UseTwoPass);
                            Console.WriteLine("\nFFmpeg pass 1: {0}", FfmpegPass1);
                            Console.WriteLine("\nFFmpeg pass 2: {0}", FfmpegPass2);
                        }
                        else
                        {
                            var FfmpegDone = Prompt.Select("Would you like to convert another video, return to the main menu, or exit the program?", new[] { "Convert", "Return", "Exit" });
                            if (FfmpegDone == "Convert")
                            {
                                Console.Clear();
                                //Resets variables
                                FfmpegPass1 = "";
                                FfmpegPass2 = "";
                                AudioCodecs = "";
                                CRF = 0;
                                ABR = "";
                                AudioBitrate = 0;
                                FileInput = "";
                                FileOutput = "";
                                GoodExit = true;
                                UseMetadata = false;
                                continue;
                            }
                            else if (FfmpegDone == "Return")
                            {
                                Console.Clear();
                                //Resets variables
                                FfmpegPass1 = "";
                                FfmpegPass2 = "";
                                AudioCodecs = "";
                                CRF = 0;
                                ABR = "";
                                AudioBitrate = 0;
                                FileInput = "";
                                FileOutput = "";
                                GoodExit = true;
                                UseMetadata = false;
                                break;
                            }
                            else if (FfmpegDone == "Exit") Environment.Exit(0);
                        }
                    } while (true);
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
        static void ShowFfmpegPass2(string FfmpegPass2) //creates custom function named ShowCommand. Used for displaying the current command used for ffmpeg. Thanks to a certain individual for teaching me this!
        {
            Console.Clear();
            Console.Write("\nYour current ffmpeg commandline looks like the following: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("ffmpeg{0}", FfmpegPass2);
            Console.ResetColor();
            Console.WriteLine("\n");
        }
        static (string, string) GetOS() //creates custom function named GetOS for detecting the OS and setting the Regex and OS variable accordingly. Also tells the function it'll return two string type variables
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
        static void BitrateHelp()
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
            Console.WriteLine("The bitrate is constant and each frame gets the same bitrate regardless of how much it requires.");
            Console.WriteLine("This can end up wasting valuable data if there is barely any movement in the video,");
            Console.WriteLine("while making faster parts of the video look poorly, if the bitrate isn't high enough.");
            Console.WriteLine("This option is only prefered if a certain filesize is required\n");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
        static void VariableBitrateHelp()
        {

        }
    }
}