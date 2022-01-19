using Sharprompt; //loads the interactable console package
using System.Diagnostics;
using System.Text.RegularExpressions;
// Sets variables
//string WebMFileInput;
Boolean WebMInputValid = false;
//string TrimmedWebMInput;
var IllegalChar = new Regex(@"[<>*?\/\|""]"); //sets regex for the following bad/illegal characters: <>*?/|
string command = "ffmpeg.exe";

//main menu
do
{
    Console.WriteLine("---------------Welcome---------------");
    Console.WriteLine("                  to                 ");
    Console.WriteLine("  Nicklo's FFmpeg Console Interface\n");
    var UserChoice = Prompt.Select("Please select an option", new[] { "WebM", "MP4", "Info", "Exit" }); //Uses Sharprompt for the main manu's user input

    //checks UserChoice
    if (UserChoice == "WebM") //UserChoice for converting to WebM
    {
        do
        {
            Console.Clear();
            var WebMUserChoice = Prompt.Select("You have selected WebM. Please select a bitrate option", new[] { "Variable", "Constant", "Help" }); //Uses Sharprompt for the WebM's bitrate options

            if (WebMUserChoice == "Help") //UserChoice for displaying a help section about bitrate options
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
            }
            else
            {
                var WebMDisableAudio = Prompt.Confirm("Disable audio?", defaultValue: false);//asks with Sharprompt if the user wishes to disable audio.
                do
                {
                    WebMInputValid = false;
                    Console.WriteLine("\n");
                    var WebMFileInput = Prompt.Input<string>("Please select the video file. You can drag and drop the file to complete the filename and location.");
                    if (String.IsNullOrEmpty(WebMFileInput))//checks if input is empty or null and fails if true.
                    {
                        Console.WriteLine("\n");
                        Console.WriteLine("Input cannot be empty!");
                    }

                    else if (IllegalChar.IsMatch(WebMFileInput.Trim('"')))//checks if input contains any of the previously mentioned illegal characters, and fails if true.
                    {
                        Console.WriteLine("\n");
                        Console.WriteLine("Input contains invalid characters, please refrain from using any of the following: <>*?/|\"");
                    }
                    else if (Path.HasExtension(WebMFileInput.Trim('"')) == false)//checks if input is contains a file extension, and fails if false.
                    {
                        Console.WriteLine("\n");
                        Console.WriteLine("Input requires a file extension at the end!");
                    }
                    else if (File.Exists(WebMFileInput.Trim('"')) == false)//checks if input is actually accesible or exists, and fails if false.
                    {
                        Console.WriteLine("\n");
                        Console.WriteLine("Input file cannot be found or this program does not have permission to access it. Please make sure you typed it correctly.");
                    }
                    else if (WebMFileInput.StartsWith('"') == false  && (WebMFileInput.EndsWith('"')  == false) && (WebMFileInput.Any(char.IsWhiteSpace)))//checks for double quotes at the start and end of the string, and fails if false.
                    {
                        Console.WriteLine("\n");
                        Console.WriteLine("No double quotes detected but spaces detected. Automatically adding double quotes...");
                        WebMFileInput = '"' + WebMFileInput + '"';//adds double quotes
                        WebMInputValid=true;//sets WebMIinputValid to true if all other conditions are false, and exits the DoWhile loop.
                    }
                    else
                    {
                        WebMInputValid=true;//sets WebMIinputValid to true if all other conditions are false, and exits the DoWhile loop.
                    }
                }while (WebMInputValid == false);
                Console.WriteLine("This is outside the loop");
                Console.ReadKey();

            }
        } while (true);
    }
    if (UserChoice == "MP4") //UserChoice for converting to MP4
    {

    }
    if (UserChoice == "Exit") //UserChoice for exiting the program
    {
        Environment.Exit(0);
    }
    if (UserChoice == "Info") //UserChoice for information about the program
    {
        Console.Clear();
        Console.WriteLine("This program was written with frustration and the will to learn.");
        Console.WriteLine("Sharprompt was also used to create the interactable menu's");
        Console.WriteLine("and all credit goes to it's respective developer(s)");
        Console.WriteLine("at https://github.com/shibayan/Sharprompt \n");
        Console.WriteLine("Press any key to continue");
        Console.ReadKey();
        Console.Clear();
    }
} while (true);