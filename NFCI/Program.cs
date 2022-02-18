using Sharprompt; //loads the interactable console package
using System.Diagnostics; //Used with running external programs, such as ffmpeg
using System.Runtime.InteropServices; //Used for OS detection
using System.Text.RegularExpressions; //Used with Regex

// Sets variables
bool WebMInputValid = false; //sets the validator for the WebM input to false
String RegexString = null; //create empty string variable to allow the OS detection and Regex to use it

//code to detect the type of OS being used, and set the RegexString variable accordingly
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //runs if the OS is Windows
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Windows based OS detected!");
    Console.ResetColor();
    RegexString = @"[<>*?\/\|""]";
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) //runs if the OS is Linux
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Linux based OS detected!");
    Console.ResetColor();
    RegexString = "\"";
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) //runs if the OS is MacOS
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("MacOS detected!");
    Console.ResetColor();
    RegexString = "\"";
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) //runs if the OS is FreeBSD
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("FreeBSD detected!");
    Console.ResetColor();
    RegexString = "\"";
}
else //runs if the OS type could not be detected, or is unknown
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error detecting OS! This shouldn't be an issue unless you're running Windows");
    Console.ResetColor();
    RegexString = "\"";
}
Regex IllegalChar = new(RegexString); //sets regex using the previously set RegexString variable used in the OS detection
string command = "ffmpeg"; //builds the start of the ffmpeg command


do //main menu loop
{
    //if (File.Exists("ffmpeg.exe") == false || (File.Exists("ffmpeg") == false ))
    //{
    //    Console.ForegroundColor = ConsoleColor.Red;
    //    Console.WriteLine("ffmpeg was not found in directory.");
    //    Console.WriteLine("Please add ffmpeg either to PATH, or add it to {0}",Directory.GetCurrentDirectory());
    //    Console.WriteLine("\n");
    //    Console.ForegroundColor= ConsoleColor.Green;
    //    Console.WriteLine("If it exists in PATH you can safely ignore this");
    //    Console.WriteLine("\n");
    //    Console.ResetColor();
    //}
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
                    var WebMUserChoice = Prompt.Select("You have selected WebM. Please select a bitrate option. Use arrow keys and enter to navigate", new[] { "Variable", "Constant", "Help" }); //Uses Sharprompt for the WebM's bitrate options
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
                        case "Constant":
                            {
                                Console.WriteLine("\n");
                                var WebMDisableAudio = Prompt.Confirm("Disable audio? Press the Y or N key, or enter to default to N", defaultValue: false);//asks with Sharprompt if the user wishes to disable audio.

                                do
                                {
                                    WebMInputValid = false;
                                    Console.WriteLine("\n");
                                    var WebMFileInput = Prompt.Input<string>("Please select the video file. You can drag and drop the file to complete the filename and location");
                                    if (String.IsNullOrEmpty(WebMFileInput) || String.IsNullOrEmpty(WebMFileInput.Trim('"')))//checks if input is empty or null and fails if true.
                                    {
                                        Console.WriteLine("\nInput cannot be empty!");
                                    }

                                    else if (IllegalChar.IsMatch(WebMFileInput.Trim('"')))//checks if input contains any of the previously mentioned illegal characters, and fails if true.
                                    {
                                        Console.WriteLine("\nInput contains invalid characters, please refrain from using any of the following: <>*?/|\"");
                                    }
                                    else if (Path.HasExtension(WebMFileInput.Trim('"')) == false)//checks if input is contains a file extension, and fails if false.
                                    {
                                        Console.WriteLine("\nInput requires a file extension at the end!");
                                    }
                                    else if (File.Exists(WebMFileInput.Trim('"')) == false)//checks if input is actually accesible or exists, and fails if false.
                                    {
                                        Console.WriteLine("\nInput file cannot be found or this program does not have permission to access it. Please make sure you typed it correctly.");
                                    }
                                    else if (WebMFileInput.StartsWith('"') == false && (WebMFileInput.EndsWith('"') == false) && (WebMFileInput.Any(char.IsWhiteSpace)))//checks for double quotes at the start and end of the string, and fails if false.
                                    {
                                        Console.WriteLine("\nNo double quotes detected but spaces detected. Automatically adding double quotes...");
                                        WebMFileInput = '"' + WebMFileInput + '"'; //adds double quotes
                                        WebMInputValid = true; //sets WebMIinputValid to true if all other conditions are met, and exits the DoWhile loop.
                                        command = command + ' ' + "-i" + ' ' + WebMFileInput; //build the command variable so it can be used outside
                                        if (WebMDisableAudio == true) //adds an additional flag for removing audio tracks
                                        {
                                            command = command + ' ' + "-an";
                                        }
                                    }
                                    else
                                    {
                                        WebMInputValid = true; //sets WebMIinputValid to true if all other conditions are met, and exits the DoWhile loop.
                                        command = command + ' ' + "-i" + ' ' + WebMFileInput; //build the command variable so it can be used outside
                                        if (WebMDisableAudio == true) //adds an additional flag for removing audio tracks
                                        {
                                            command = command + ' ' + "-an";
                                        }
                                    }
                                } while (WebMInputValid == false);
                                
                                Console.Clear();
                                Console.Write("\nYour current ffmpeg commandline looks like the following: ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(command);
                                Console.ResetColor();
                                Console.WriteLine("\n");
                                var WebMVideoCodecs = Prompt.Select("Please select the video codecs to use. VP9 is prefered as it's newer and more efficient", new[] { "VP9", "VP8" });
                                command = command + ' ' + "-c:v" + ' ' + WebMVideoCodecs;
                                Console.Clear();
                                Console.Write("\nYour current ffmpeg commandline looks like the following: ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(command);
                                Console.ResetColor();
                                Console.WriteLine("\n");
                                if (WebMDisableAudio == false)
                                {
                                    var WebMAudioCodecs = Prompt.Select("Please select the audio codecs to use. Libopus is prefered as it's newer and more efficient", new[] { "libopus", "libvorbis" });
                                    command = command + ' ' + "-c:a" + ' ' + WebMAudioCodecs;
                                    Console.Clear();
                                    Console.Write("\nYour current ffmpeg commandline looks like the following: ");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(command);
                                    Console.ResetColor();
                                    Console.WriteLine("\n");
                                }

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("this is the end");
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
        case "Info": //Displays a small info box.
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