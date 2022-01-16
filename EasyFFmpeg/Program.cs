using Sharprompt; //loads the interactable console package
// Sets variables
//string UserChoice;
//string WebMUserChoice;

//main menu
Start:
Console.WriteLine("---------------Welcome---------------");
Console.WriteLine("                  to                 ");
Console.WriteLine("  Nicklo's FFmpeg Console Interface\n");
var UserChoice = Prompt.Select("Please select an option", new[] { "WebM", "MP4", "Exit"});

//console.writeline("please chose an option:");
//console.writeline("convert to (w)ebm");
//console.writeline("convert to (m)p4");
//console.writeline("(e)xit the program");
//userchoice = console.readline().toupper();

//checks UserChoice
if (UserChoice == "WebM") //UserChoice for converting to WebM
{
    Console.Clear();
    Console.WriteLine("You've chosen to convert to WebM\n");
    Console.WriteLine("Please select whether you'd like to use Variable or Constant bitrate");
    Console.WriteLine("(V)ariable");
    Console.WriteLine("(C)onstant");
    Console.WriteLine("(H)elp");
    WebMUserChoice = Console.ReadLine().ToUpper();

    if (WebMUserChoice == "H")
    {
        Console.Clear();
        Console.WriteLine("\t\tVariable bitrate:");
        Console.WriteLine("As it suggests, this means the bitrate is variable, and can increase or decrease per frame,");
        Console.WriteLine("depending on how much dedicated data ffmpeg believes each frame needs");
        Console.ReadKey();



    }
    else
    {

    }
}
else if (UserChoice == "MP4") //UserChoice for converting to MP4
{

}
else if (UserChoice == "Exit") //UserChoice for exiting the program
{
    return;
}
//else
//{
//    Console.WriteLine("{0} is not a valid option. Returning...",UserChoice);
//    Thread.Sleep(3000);
//    Console.Clear();
//    goto Start;
//}