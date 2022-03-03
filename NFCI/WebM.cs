using Sharprompt; //loads the interactable console package
using System.Diagnostics; //Used with running external programs, such as ffmpeg
using System.Runtime.InteropServices; //Used for OS detection
using System.Text.RegularExpressions; //Used with Regex

namespace NFCI
{
    public class WebM
    {
        public static void ConvertToWebM()
        {
            // Sets variables
            (string RegexString, string FfmpegPass1Output) = MainMenu.GetOS(); //saves the returned variables from GetOS function into variables that can be used in main code
            Regex IllegalChar = new(RegexString); //sets regex using the previously set RegexString variable gotten from the GetOS function
            string FfmpegPass1 = ""; //builds the start of the ffmpeg command for pass 2
            string FfmpegPass2 = ""; //builds the start of the ffmpeg command for pass 2
            string AudioCodecs = "";
            byte CRF; //Variable used for Constant Quality bitrate mode
            string ABR; //variable used for Average Bitrate mode
            string CBR; //variable used for Constant Bitrate mode
            short AudioBitrate;
            string FileInput = "", FileOutput;
            bool GoodExit = true;
            Regex GoodBitrate = new(@"^[0-9]+(?:k|m|K|M)?$"); //create regex only allowing numbers, and k and m as well as in uppercase
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

                MainMenu.ShowFfmpegPass2(FfmpegPass2); //prints current ffmpeg commandline
                var VideoCodecs = Prompt.Select("Please select the video codecs to use. 'vp9' is prefered as it's newer and more efficient", new[] { "libvpx-vp9", "libvpx-vp8" }); //Uses Sharprompt to select videos codecs
                FfmpegPass2 = FfmpegPass2 + ' ' + "-c:v" + ' ' + VideoCodecs;
                FfmpegPass1 = FfmpegPass1 + ' ' + "-c:v" + ' ' + VideoCodecs + ' ' + "-an"; //audio is not needed on the first pass
                if (DisableAudio == false) //ask for audio codec too if WebMDisableAudio is false
                {
                    MainMenu.ShowFfmpegPass2(FfmpegPass2);
                    AudioCodecs = Prompt.Select("Please select the audio codecs to use. 'Libopus' is prefered as it's newer and more efficient", new[] { "libopus", "libvorbis" });//Uses Sharprompt to select audio codecs
                    FfmpegPass2 = FfmpegPass2 + ' ' + "-c:a" + ' ' + AudioCodecs;
                    MainMenu.ShowFfmpegPass2(FfmpegPass2);
                }

                do
                {
                    MainMenu.ShowFfmpegPass2(FfmpegPass2);
                    var BitrateMode = Prompt.Select("Please select the bitrate mode. A help option is also available", new[] { "Variable", "Constant", "Help" }); //Uses Sharprompt to select the bitrate mode
                    if (BitrateMode == "Help")
                    {
                        MainMenu.BitrateHelp();
                        continue;
                    }
                    if (BitrateMode == "Variable") //menu if the Variable bitrate option was chosen
                    {
                        do
                        {
                            MainMenu.ShowFfmpegPass2(FfmpegPass2);
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
                                    else if (GoodBitrate.IsMatch(ABR)) //checks if the bitrate matches the GoodBitrate Regex
                                    {
                                        if (!ABR.Any(char.IsLetter)) //true if CBR does NOT contain any letters
                                        {
                                            FfmpegPass2 = FfmpegPass2 + ' ' + "-b:v" + ' ' + ABR + "M";
                                            FfmpegPass1 = FfmpegPass1 + ' ' + "-b:v" + ' ' + ABR + "M";
                                            break;
                                        }
                                        else
                                        {
                                            FfmpegPass2 = FfmpegPass2 + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                            FfmpegPass1 = FfmpegPass1 + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                            break;
                                        }
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
                                        else if (GoodBitrate.IsMatch(ABR)) //checks if the bitrate matches the GoodBitrate Regex
                                        {
                                            if (!ABR.Any(char.IsLetter)) //true if CBR does NOT contain any letters
                                            {
                                                FfmpegPass2 = FfmpegPass2 + ' ' + "-crf" + ' ' + CRF + ' ' + ' ' + "-b:v" + ' ' + ABR + "M";
                                                FfmpegPass1 = FfmpegPass1 + ' ' + "-crf" + ' ' + CRF + ' ' + ' ' + "-b:v" + ' ' + ABR + "M";
                                                break;
                                            }
                                            else
                                            {
                                                FfmpegPass2 = FfmpegPass2 + ' ' + "-crf" + ' ' + CRF + ' ' + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                                FfmpegPass1 = FfmpegPass1 + ' ' + "-crf" + ' ' + CRF + ' ' + ' ' + "-b:v" + ' ' + ABR.ToUpper();
                                                break;
                                            }
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
                                MainMenu.VariableBitrateHelp();
                                continue;
                            }
                            break;
                        } while (true);
                    }
                    else if (BitrateMode == "Constant") //menu if the Constant bitrate option was chosen
                    {
                        Console.WriteLine("\n");
                        CBR = Prompt.Input<string>("Please specify the bitrate to be used in megabits. Kilobits can also be used by putting a 'k' at the end");
                        if (CBR == null)
                        {
                            Console.WriteLine("Bitrate cannot be empty!");
                        }
                        else if (GoodBitrate.IsMatch(CBR)) //checks if the bitrate matches the GoodBitrate Regex
                        {
                            if (!CBR.Any(char.IsLetter)) //true if CBR does NOT contain any letters
                            {
                                FfmpegPass2 = FfmpegPass2 + ' ' + "-minrate" + ' ' + CBR + "M" + ' ' + "-b:v" + ' ' + CBR + "M" + ' ' + "-maxrate" + ' ' + CBR + "M";
                                FfmpegPass1 = FfmpegPass1 + ' ' + "-minrate" + ' ' + CBR + "M" + ' ' + "-b:v" + ' ' + CBR + "M" + ' ' + "-maxrate" + ' ' + CBR + "M";
                                break;
                            }
                            else
                            {
                                FfmpegPass2 = FfmpegPass2 + ' ' + "-minrate" + ' ' + CBR.ToUpper() + ' ' + "-b:v" + ' ' + CBR.ToUpper() + ' ' + "-maxrate" + ' ' + CBR.ToUpper();
                                FfmpegPass1 = FfmpegPass1 + ' ' + "-minrate" + ' ' + CBR.ToUpper() + ' ' + "-b:v" + ' ' + CBR.ToUpper() + ' ' + "-maxrate" + ' ' + CBR.ToUpper();
                                break;
                            }
                        }
                        else //bitrate must be invalid if the previous 2 were not true
                        {
                            Console.WriteLine("{0} is not a valid bitrate! Only the letter M and K are allowed after, and no spaces are allowed.", CBR);
                        }
                    }
                    break;
                } while (true);
                MainMenu.ShowFfmpegPass2(FfmpegPass2);

                if (DisableAudio == false) //checks if audio is set to be disabled
                {
                    AudioBitrate = Prompt.Input<short>("Please specify the audio bitrate in kilobits e.g. 192"); //prompts for audio bitrate
                    FfmpegPass2 = FfmpegPass2 + ' ' + "-b:a" + ' ' + AudioBitrate + "k";
                    MainMenu.ShowFfmpegPass2(FfmpegPass2);
                }

                var pix_fmt = Prompt.Select("Please select the chroma subsampling. 'yuv420p' is the most compatible and prefered.", new[] { "yuv420p", "yuv422p", "yuv444p" }); //prompts for the pixel format/chroma subsampling
                FfmpegPass2 = FfmpegPass2 + ' ' + "-pix_fmt" + ' ' + pix_fmt;
                FfmpegPass1 = FfmpegPass1 + ' ' + "-pix_fmt" + ' ' + pix_fmt;
                MainMenu.ShowFfmpegPass2(FfmpegPass2);

                var UseMetadata = Prompt.Confirm("Would you like to add metadata to the file? Defaults to No", defaultValue: false); //Uses Sharprompt to ask the user if they'd like to add metadata to the file.
                if (UseMetadata == true)
                {
                    var UseMetadataTitle = Prompt.Confirm("Would you like to add a metadata title? Defaults to No", defaultValue: false); //Uses Sharprompt to ask if the user wants to add a metadata title
                    if (UseMetadataTitle)
                    {
                        var MetadataTitle = Prompt.Input<string>("Please type or paste the metadata title");
                        if (MetadataTitle.Any(char.IsWhiteSpace))
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
                MainMenu.ShowFfmpegPass2(FfmpegPass2);

                var deadline = Prompt.Select("Please select the deadline. Best can make the conversion take 5x as long or more, however yields the best quality-to-bit-ratio", new[] { "best", "good", "realtime" }); //Uses Sharprompt to ask which deadline the user would like to use
                FileOutput = Path.ChangeExtension(FileInput, ".webm"); //takes the input and changes it's extension to webm, and turns it into the output file
                if (FileOutput.Any(char.IsWhiteSpace)) //adds double quotes if the output contains any whitespaces
                {

                    FileOutput = FileOutput.Replace("\"", "");
                    FileOutput = '"' + FileOutput + '"';

                }
                FfmpegPass2 = FfmpegPass2 + ' ' + "-deadline" + ' ' + deadline;
                FfmpegPass1 = FfmpegPass1 + ' ' + "-deadline" + ' ' + deadline;
                MainMenu.ShowFfmpegPass2(FfmpegPass2);


                var UseTwoPass = Prompt.Confirm("Use Two-pass encoding? This will greatly increase quality, but also requires some extra encoding time as it essentially does it twice", defaultValue: true); //Uses Sharprompt to ask if the user wants to use two-pass encoding
                try
                {
                    if (UseTwoPass)
                    {
                        FfmpegPass2 = FfmpegPass2 + ' ' + "-pass" + ' ' + "2" + ' ' + FileOutput;
                        FfmpegPass1 = FfmpegPass1 + ' ' + "-pass" + ' ' + "1" + ' ' + "-f" + ' ' + "null" + ' ' + FfmpegPass1Output;
                        MainMenu.ShowFfmpegPass2(FfmpegPass2);
                        Console.WriteLine("\n");
                        var CorrectFfmpegPass = Prompt.Confirm("Does this look correct?", defaultValue: true);
                        if (!CorrectFfmpegPass) //restarts if the user pressed no
                        {
                            Console.Clear();
                            break;
                        }
                        Process FFmpeg1 = Process.Start("ffmpeg", FfmpegPass1); //starts the first pass encoding
                        FFmpeg1.WaitForExit(); //waits for the started ffmpeg process to finish
                        Process FFmpeg2 = Process.Start("ffmpeg", FfmpegPass2); //starts the second pass encoding
                        FFmpeg2.WaitForExit(); //waits for the started ffmpeg process to finish
                        if (FFmpeg2.ExitCode != 0) GoodExit = false; //sets GoodExit to false if anything over 0 was returned. FFmpeg returns 0 if no errors were detected, and 1 if something went wrong
                    }
                    else
                    {
                        FfmpegPass2 = FfmpegPass2 + ' ' + FileOutput;
                        MainMenu.ShowFfmpegPass2(FfmpegPass2);
                        Console.WriteLine("\n");
                        var CorrectFfmpegPass = Prompt.Confirm("Does this look correct?", defaultValue: true);
                        if (!CorrectFfmpegPass) //restarts if the user pressed no
                        {
                            Console.Clear();
                            break;
                        }
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
    }
}
