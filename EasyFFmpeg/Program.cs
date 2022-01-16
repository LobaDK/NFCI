// Sets variables
string UserChoice;
string WebMUserChoice;

//main menu
Start:
Console.WriteLine("---------------Welcome---------------");
Console.WriteLine("                  to                 ");
Console.WriteLine("  Nicklo's FFmpeg Console Interface\n");
Console.WriteLine("Please chose an option:");
Console.WriteLine("Convert to (W)ebM");
Console.WriteLine("Convert to (M)P4");
Console.WriteLine("(E)xit the program");
UserChoice = Console.ReadLine().ToUpper();

//checks UserChoice
if (UserChoice == "W") //UserChoice for converting to WebM
{
    Console.Clear();
    Console.WriteLine("You've chosen to convert to WebM\n");
    Console.WriteLine("Please select whether you'd like to use Variable or Constant bitrate");
    Console.WriteLine("(V)ariable");
    Console.WriteLine("(C)onstant");
    Console.WriteLine("(H)elp");
    WebMUserChoice = Console.ReadLine();

    //checks WebmUserChoice
    if (WebMUserChoice == "V") //WebmUserChoice for variable bitrate
    {

    }
    else if (WebMUserChoice == "C") //WebMUserChoice for constant bitrate
    {

    }

}
else if (UserChoice == "M") //UserChoice for converting to MP4
{

}
else if (UserChoice == "E") //UserChoice for exiting the program
{
    return;
}
else
{
    Console.WriteLine("{0} is not a valid option. Returning...",UserChoice);
    Thread.Sleep(3000);
    Console.Clear();
    goto Start;
}