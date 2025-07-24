using System;
using System.Text;
using Spectre.Console;
using TESE;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Spectre.Console.Rendering;

namespace TESE
{

    class Program
    {
        private static string path;
        private static string category;
        private static string[] editPromptArray;
        private static string editSelection;
        private static string[] saveFile;
        private static bool comingFromInv = false;

        static void Main(string[] args)
        {
            DrawLogo();
            AskForPath();
            DecryptSaveFile(path);

            while (true)
            {
                CategorySelect();
                if (category == "[green]Save File[/] and [red]Exit[/]")
                {
                    OverwriteSaveFile();
                    Environment.Exit(0);
                }
                else if (category == "[red]Exit Without Saving[/]")
                {
                    Environment.Exit(0);
                }

                while (true)
                {
                    
                    EditSelect();
                    if (editSelection == "[green]Select Different Category[/]")
                        break; // Go back to category selection

                    EditTheSelection();
                    if (comingFromInv)
                    {
                        comingFromInv = false;
                        break;
                    }
                }
            }
        }
        private static void AskForPath()
        {
            path = AnsiConsole.Prompt(
                new TextPrompt<string>("File path for [blue]save.dat[/] :"));
            Console.Clear();
        }
        private static void DecryptSaveFile(string path)
        {
            try
            {
                byte[] fileBytes;
                string key = "mothking";

                fileBytes = System.IO.File.ReadAllBytes(path);
                BlowfishCompat decryptionBlowFish = new BlowfishCompat(key);
                byte[] decryptedData = decryptionBlowFish.Decrypt(fileBytes);
                string decryptedString = Encoding.ASCII.GetString(decryptedData);

                // Store the decrypted lines in memory, do NOT overwrite the file
                saveFile = decryptedString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            }
            catch
            {
                DrawLogo();
                AnsiConsole.MarkupLine("[bold red]Error: Invalid file path.[/]");
                AnsiConsole.MarkupLine("[red]Press Enter to continue...[/]");
                Console.Read();
                Console.Clear();

                DrawLogo();
                AskForPath();
            }
        }
        private static void OverwriteSaveFile()
        {
            string key = "mothking";
            string saveText = string.Join("\r\n", saveFile);
            BlowfishCompat blowfish = new BlowfishCompat(key);
            byte[] encryptedData = blowfish.Encrypt(Encoding.ASCII.GetBytes(saveText));
            System.IO.File.WriteAllBytes(path, encryptedData);
        }
        private static void CategorySelect()
        {
            category = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a [green]category[/] to edit.")
                    .AddChoices(new[]
                    {
                        "Player", "Prison", "Inmates", "Guards",
                        "Inmate Inventories", "Guard Inventories",
                        "Desks", "[green]Save File[/] and [red]Exit[/]",
                        "[red]Exit Without Saving[/]"
                    })
            );
            Console.Clear();

            if (category == "[green]Save File[/] and [red]Exit[/]")
            {
                OverwriteSaveFile();
                Environment.Exit(0);
            }
            else if (category == "[red]Exit Without Saving[/]")
            {
                Environment.Exit(0);
            }
        }
        private static void EditSelect()
        {
            switch (category)
            {
                case "Player":
                    editPromptArray = new string[]
                    {
                            "Name", "Cash", "Health", "Heat",
                            "Fatigue", "Strength", "Speed",
                            "Intellect", "Job", "Weapon",
                            "Outfit", "Character", "Inventory",
                            "Playtime", "[green]Select Different Category[/]"
                    };
                    break;
                case "Prison":
                    editPromptArray = new string[]
                    {
                        "Day", "Hints Bought", "[green]Select Different Category[/]"
                    };
                    break;
                case "Inmates":
                    editPromptArray = new string[]
                    {
                        "Names", "Stats",
                        "Weapons", "Characters",
                        "Shop Items", "[green]Select Different Category[/]"
                    };
                    break;
                case "Guards":
                    editPromptArray = new string[]
                    {
                        "Names", "Stats", "[green]Select Different Category[/]"
                    };
                    break;
                default:
                    comingFromInv = true;
                    return;
                    
            }
            comingFromInv = false;
            editSelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select what you want to [green]edit[/].")
                    .PageSize(15)
                    .AddChoices(editPromptArray)
            );
               
        }
        
        private static void EditTheSelection()
        {
            //stuff to set up for certain cases
            int x = -1;
            foreach (string aLine in saveFile)
            {
                x++;
                if (aLine == "[Inmate_Inven]")
                {
                    break;
                }
            }

            int aNumber = GetFirstNum(saveFile[x - 1], '=');

            string[] names = new string[aNumber];

            for (int i = 0; i < aNumber; i++)
            {
                names[i] = (i + 1) + ". " + GetCurrent("Inmates", (i + 1).ToString(), 1);
            }

            int y = -1;
            foreach (string bLine in saveFile)
            {
                y++;
                if (bLine == "[Guard_Inven]")
                {
                    break;
                }
            }

            int bNumber = GetFirstNum(saveFile[y - 1], '=');

            string[] aNames = new string[bNumber];

            for (int i = 0; i < bNumber; i++)
            {
                aNames[i] = (i + 1) + ". " + GetCurrent("Guards", (i + 1).ToString(), 1);
            }

            switch (category)
            {
                case "Player":
                    switch (editSelection)
                    {
                        case "Name":
                            AnsiConsole.MarkupLine("[bold yellow]Current name[/] : " + GetCurrent("Player", "Name"));
                            Console.WriteLine();
                            string name = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New name[/] :"));
                            EditSave("Player", "Name", name);
                            break;
                        case "Cash":
                            AnsiConsole.MarkupLine("[bold yellow]Current cash[/] : " + GetCurrent("Player", "Cash_HP_Heat_Fat", 1));
                            Console.WriteLine();
                            string cash = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New cash[/] :"));
                            EditSave("Player", "Cash_HP_Heat_Fat", cash, 1);
                            break;
                        case "Health":
                            AnsiConsole.MarkupLine("[bold yellow]Current health[/] : " + GetCurrent("Player", "Cash_HP_Heat_Fat", 2));
                            Console.WriteLine();
                            string health = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New health[/] :"));
                            EditSave("Player", "Cash_HP_Heat_Fat", health, 2);
                            break;
                        case "Heat":
                            AnsiConsole.MarkupLine("[bold yellow]Current heat[/] : " + GetCurrent("Player", "Cash_HP_Heat_Fat", 3));
                            Console.WriteLine();
                            string heat = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New heat[/] :"));
                            EditSave("Player", "Cash_HP_Heat_Fat", heat, 3);
                            break;
                        case "Fatigue":
                            AnsiConsole.MarkupLine("[bold yellow]Current fatigue[/] : " + GetCurrent("Player", "Cash_HP_Heat_Fat", 4));
                            Console.WriteLine();
                            string fatigue = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New fatigue[/] :"));
                            EditSave("Player", "Cash_HP_Heat_Fat", fatigue, 4);
                            break;
                        case "Strength":
                            AnsiConsole.MarkupLine("[bold yellow]Current strength[/] : " + GetCurrent("Player", "Stats", 1));
                            Console.WriteLine();
                            string strength = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New strength[/] :"));
                            EditSave("Player", "Stats", strength, 1);
                            break;
                        case "Speed":
                            AnsiConsole.MarkupLine("[bold yellow]Current speed[/] : " + GetCurrent("Player", "Stats", 2));
                            Console.WriteLine();
                            string speed = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New speed[/] :"));
                            EditSave("Player", "Stats", speed, 2);
                            break;
                        case "Intellect":
                            AnsiConsole.MarkupLine("[bold yellow]Current intellect[/] : " + GetCurrent("Player", "Stats", 3));
                            Console.WriteLine();
                            string intellect = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New intellect[/] :"));
                            EditSave("Player", "Stats", intellect, 3);
                            break;
                        case "Job":
                            AnsiConsole.MarkupLine("[bold yellow]Current job[/] : " + GetCurrent("Player", "Job"));
                            Console.WriteLine();
                            AnsiConsole.MarkupLine("[silver](For a list of jobs, check the [green]Key.txt[/] file in this program's folder.)[/]");
                            string job = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New job[/] :"));
                            EditSave("Player", "Job", job);
                            break;
                        case "Weapon":
                            AnsiConsole.MarkupLine("[bold yellow]Current weapon[/] : " + GetCurrent("Player", "Weapon"));
                            Console.WriteLine();
                            AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                            string weapon = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New weapon[/] :"));
                            EditSave("Player", "Weapon", weapon);
                            break;
                        case "Outfit":
                            AnsiConsole.MarkupLine("[bold yellow]Current outfit[/] : " + GetCurrent("Player", "Outfit"));
                            Console.WriteLine();
                            AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                            string outfit = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New outfit[/] :"));
                            EditSave("Player", "Outfit", outfit);
                            break;
                        case "Character":
                            AnsiConsole.MarkupLine("[bold yellow]Current character[/] : " + GetCurrent("Player", "Avatar"));
                            Console.WriteLine();
                            AnsiConsole.MarkupLine("[silver](For a list of characters and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                            string avatar = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New character[/] :"));
                            EditSave("Player", "Avatar", avatar);
                            break;
                        case "Inventory":
                            while (true)
                            {
                                string[] invArray = new string[7];
                                AnsiConsole.MarkupLine("[bold yellow]Current inventory[/] : ");
                                for (int i = 1; i < 7; i++)
                                {
                                    invArray[i - 1] = i + ". " + GetCurrent("Player", "Inv", i);
                                    AnsiConsole.MarkupLine(i + ". " + GetCurrent("Player", "Inv", i));
                                }
                                invArray[6] = "Done";
                                Console.WriteLine();
                                string invSlot = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("Select an item to edit : ")
                                        .AddChoices(invArray)
                                );
                                if (invSlot != "Done")
                                {
                                    AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                                    string item = AnsiConsole.Prompt(
                                        new TextPrompt<string>("[yellow]New item[/] :"));
                                    EditSave("Player", "Inv", item + "_100", Convert.ToInt32(invSlot[0].ToString()));
                                }
                                else
                                {
                                    break;
                                }
                                Console.Clear();
                            }
                            break;
                        case "Playtime":
                            AnsiConsole.MarkupLine("[bold yellow]Current playtime[/] : " + GetCurrent("Player", "Playtime"));
                            Console.WriteLine();
                            string playtime = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New playtime[/] :"));
                            EditSave("Player", "Playtime", playtime);
                            break;
                    }
                    break;
                case "Prison":
                    switch (editSelection)
                    {
                        case "Day":
                            AnsiConsole.MarkupLine("[bold yellow]Current day[/] : " + GetCurrent("Prison", "Day"));
                            Console.WriteLine();
                            string day = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New day[/] :"));
                            EditSave("Prison", "Day", day);
                            break;
                        case "Hints Bought":
                            while (true)
                            {
                                string[] hintArray = new string[4];
                                AnsiConsole.MarkupLine("[bold yellow]Current hint bought statuses[/] : ");
                                for (int i = 1; i < 4; i++)
                                {
                                    hintArray[i - 1] = i + ". " + GetCurrent("Prison", "Hint" + i);
                                    AnsiConsole.MarkupLine(i + ". " + GetCurrent("Prison", "Hint" + i));
                                }
                                hintArray[3] = "Done";
                                Console.WriteLine();
                                string whatHint = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("Select a hint to edit : ")
                                        .AddChoices(hintArray)
                                );
                                if (whatHint != "Done")
                                {
                                    int hintNum = GetFirstNum(whatHint, '.');
                                    string hintKey = "Hint" + hintNum;
                                    AnsiConsole.MarkupLine("[silver](0 = false, 1 = true)[/]");
                                    string hint = AnsiConsole.Prompt(
                                        new TextPrompt<string>("[yellow]New hint bought status[/] :"));
                                    EditSave("Prison", hintKey, hint);
                                }
                                else
                                {
                                    break;
                                }
                                Console.Clear();
                            }
                            break;
                    }
                    break;
                case "Inmates":
                    switch (editSelection)
                    {
                        case "Names":
                            while (true)
                            {
                                AnsiConsole.MarkupLine("[bold yellow]Current names[/] : ");
                                string[] nameChoices = new string[aNumber + 1];
                                for (int i = 1; i < aNumber + 1; i++)
                                {
                                    nameChoices[i - 1] = i + ". " + GetCurrent("Inmates", i.ToString(), 1);
                                    AnsiConsole.MarkupLine(i + ". " + GetCurrent("Inmates", i.ToString(), 1));
                                }
                                nameChoices[aNumber] = "Done";
                                Console.WriteLine();
                                string whatName = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("Select a name to edit : ")
                                        .AddChoices(nameChoices)
                                ); if (whatName != "Done")
                                {
                                    string name = AnsiConsole.Prompt(
                                        new TextPrompt<string>("[yellow]New name[/] :"));

                                    EditSave("Inmates", GetFirstNum(whatName, '.').ToString(), name, 1);
                                }
                                else
                                {
                                    break;
                                }
                                Console.Clear();
                            }
                            break;
                        case "Stats":
                            while (true)
                            {
                                string inmate = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("[yellow]Select an inmate to change the stats of[/] : ")
                                        .AddChoices(names)
                                );

                                int inmateNum = GetFirstNum(inmate, '.');

                                string[] statArray = new string[6];
                                AnsiConsole.MarkupLine("[bold yellow]Current stats[/] : ");

                                statArray[0] = "Strength : " + GetCurrent("Inmates", inmateNum.ToString(), 2);
                                statArray[1] = "Speed : " + GetCurrent("Inmates", inmateNum.ToString(), 3);
                                statArray[2] = "Intellect : " + GetCurrent("Inmates", inmateNum.ToString(), 4);
                                statArray[3] = "Opinion : " + GetCurrent("Inmates", inmateNum.ToString(), 5);
                                AnsiConsole.MarkupLine("Strength : " + GetCurrent("Inmates", inmateNum.ToString(), 2));
                                AnsiConsole.MarkupLine("Speed : " + GetCurrent("Inmates", inmateNum.ToString(), 3));
                                AnsiConsole.MarkupLine("Intellect : " + GetCurrent("Inmates", inmateNum.ToString(), 4));
                                AnsiConsole.MarkupLine("Opinion : " + GetCurrent("Inmates", inmateNum.ToString(), 5));
                                statArray[4] = "Select a different inmate to change the stats of";
                                statArray[5] = "Done";
                                Console.WriteLine();
                                string whatStat = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("Select a stat to edit : ")
                                        .AddChoices(statArray)
                                );
                                if (whatStat == "Select a different inmate to change the stats of")
                                {
                                    Console.Clear();
                                    continue;
                                }
                                else if (whatStat == "Done")
                                {
                                    break;
                                }
                                else
                                {
                                    int statNum = 0;

                                    if (whatStat.StartsWith("Strength"))
                                    {
                                        statNum = 2;
                                    }
                                    else if (whatStat.StartsWith("Speed"))
                                    {
                                        statNum = 3;
                                    }
                                    else if (whatStat.StartsWith("Intellect"))
                                    {
                                        statNum = 4;
                                    }
                                    else if (whatStat.StartsWith("Opinion"))
                                    {
                                        statNum = 5;
                                    }

                                    string stat = AnsiConsole.Prompt(
                                        new TextPrompt<string>("[yellow]New stat[/] :"));
                                    EditSave("Inmates", inmateNum.ToString(), stat, statNum);
                                }
                                Console.Clear();
                            }
                            break;
                        case "Weapons":
                            while (true)
                            {
                                string[] choices = new string[names.Length + 1];
                                names.CopyTo(choices, 0);
                                choices[names.Length] = "Done";

                                string inmate = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("[yellow]Select an inmate to change the weapon of[/] : ")
                                        .AddChoices(choices)
                                );

                                if (inmate == "Done")
                                {
                                    break;
                                }

                                int inmateNum = GetFirstNum(inmate, '.');

                                AnsiConsole.MarkupLine("[bold yellow]Current weapon[/] : " + GetCurrent("Inmate_Inven", inmateNum.ToString(), 1));
                                Console.WriteLine();
                                AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                                string weapon = AnsiConsole.Prompt(
                                    new TextPrompt<string>("[yellow]New weapon[/] :"));
                                EditSave("Inmate_Inven", inmateNum.ToString(), weapon, 1);
                                Console.Clear();
                            }
                            break;
                        case "Characters":
                            while (true)
                            {
                                string[] choices = new string[names.Length + 1];
                                names.CopyTo(choices, 0);
                                choices[names.Length] = "Done";

                                string inmate = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("[yellow]Select an inmate to change the character of[/] : ")
                                        .AddChoices(choices)
                                );

                                if (inmate == "Done")
                                {
                                    break;
                                }

                                int inmateNum = GetFirstNum(inmate, '.');

                                AnsiConsole.MarkupLine("[bold yellow]Current character[/] : " + GetCurrent("Inmates", inmateNum.ToString(), 7));
                                Console.WriteLine();
                                AnsiConsole.MarkupLine("[silver](For a list of characters and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                                string character = AnsiConsole.Prompt(
                                    new TextPrompt<string>("[yellow]New character[/] :"));
                                EditSave("Inmates", inmateNum.ToString(), character, 7);
                                Console.Clear();
                            }
                            break;
                        case "Shop Items":
                            while (true)
                            {
                                string[] choices = new string[names.Length + 1];
                                names.CopyTo(choices, 0);
                                choices[names.Length] = "Done";

                                string inmate = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("[yellow]Select an inmate to change the shop items of[/] : ")
                                        .AddChoices(choices)
                                );

                                if (inmate == "Done")
                                {
                                    break;
                                }

                                int inmateNum = GetFirstNum(inmate, '.');

                                AnsiConsole.MarkupLine("[bold yellow]Current shop items[/] : " + GetCurrent("Inmates", inmateNum.ToString(), 8));
                                AnsiConsole.MarkupLine("[silver](Note: If the item's ID is [green]\"0[/],\" that means there is no item.)[/]");
                                Console.WriteLine();
                                AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                                AnsiConsole.MarkupLine("[silver](Note: Enter the item ID's as a[/] [green]list of numbers separated by commas[/][silver].)[/]");
                                AnsiConsole.MarkupLine("[blue](Example: 123,456,789,132)[/]");
                                string shopItems = AnsiConsole.Prompt(
                                    new TextPrompt<string>("[yellow]New shop items[/] :"));
                                EditSave("Inmates", inmateNum.ToString(), shopItems.Replace(" ", ""), 8);
                                Console.Clear();


                                //for (int i = 0; i < 4; i++)
                                //{
                                //    AnsiConsole.WriteLine(GetCurrent("Inmates", inmateNum.ToString(), 8));
                                //}
                            }
                            break;
                    }
                    break;
                case "Guards":
                    switch (editSelection)
                    {
                        case "Names":
                            while (true)
                            {
                                AnsiConsole.MarkupLine("[bold yellow]Current names[/] : ");
                                string[] guardNameChoices = new string[bNumber + 1];
                                for (int i = 1; i < bNumber + 1; i++)
                                {
                                    guardNameChoices[i - 1] = i + ". " + GetCurrent("Guards", i.ToString(), 1);
                                    AnsiConsole.MarkupLine(i + ". " + GetCurrent("Guards", i.ToString(), 1));
                                }
                                guardNameChoices[bNumber] = "Done";
                                Console.WriteLine();
                                string whatName = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("Select a name to edit : ")
                                        .AddChoices(guardNameChoices)
                                );
                                if (whatName != "Done")
                                {
                                    string name = AnsiConsole.Prompt(
                                        new TextPrompt<string>("[yellow]New name[/] :"));

                                    EditSave("Guards", GetFirstNum(whatName, '.').ToString(), name, 1);
                                }
                                else
                                {
                                    break;
                                }
                                Console.Clear();
                            }
                            break;
                        case "Stats":
                            bool exitGuardStats = false;
                            while (!exitGuardStats)
                            {
                                string guard = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("[yellow]Select a guard to change the stats of[/] : ")
                                        .AddChoices(aNames)
                                );

                                int guardNum = GetFirstNum(guard, '.');

                                while (true)
                                {
                                    string[] statArray = new string[6];
                                    AnsiConsole.MarkupLine("[bold yellow]Current stats[/] : ");

                                    statArray[0] = "Strength : " + GetCurrent("Guards", guardNum.ToString(), 2);
                                    statArray[1] = "Speed : " + GetCurrent("Guards", guardNum.ToString(), 3);
                                    statArray[2] = "Intellect : " + GetCurrent("Guards", guardNum.ToString(), 4);
                                    statArray[3] = "Opinion : " + GetCurrent("Guards", guardNum.ToString(), 5);
                                    AnsiConsole.MarkupLine("Strength : " + GetCurrent("Guards", guardNum.ToString(), 2));
                                    AnsiConsole.MarkupLine("Speed : " + GetCurrent("Guards", guardNum.ToString(), 3));
                                    AnsiConsole.MarkupLine("Intellect : " + GetCurrent("Guards", guardNum.ToString(), 4));
                                    AnsiConsole.MarkupLine("Opinion : " + GetCurrent("Guards", guardNum.ToString(), 5));
                                    statArray[4] = "Select a different guard to change the stats of";
                                    statArray[5] = "Done";
                                    Console.WriteLine();
                                    string whatStat = AnsiConsole.Prompt(
                                        new SelectionPrompt<string>()
                                            .Title("Select a stat to edit : ")
                                            .AddChoices(statArray)
                                    );
                                    if (whatStat == "Select a different guard to change the stats of")
                                    {
                                        Console.Clear();
                                        break; // Go back to guard selection
                                    }
                                    else if (whatStat == "Done")
                                    {
                                        exitGuardStats = true; // Exit both loops and return to edit selection
                                        break;
                                    }
                                    else
                                    {
                                        int statNum = 0;

                                        if (whatStat.StartsWith("Strength"))
                                        {
                                            statNum = 2;
                                        }
                                        else if (whatStat.StartsWith("Speed"))
                                        {
                                            statNum = 3;
                                        }
                                        else if (whatStat.StartsWith("Intellect"))
                                        {
                                            statNum = 4;
                                        }
                                        else if (whatStat.StartsWith("Opinion"))
                                        {
                                            statNum = 5;
                                        }

                                        string stat = AnsiConsole.Prompt(
                                            new TextPrompt<string>("[yellow]New stat[/] :"));
                                        EditSave("Guards", guardNum.ToString(), stat, statNum);
                                    }
                                    Console.Clear();
                                }
                            }
                            break;
                    }
                    break;
                case "Inmate Inventories":
                    while (true) // inmate selection loop
                    {
                        string[] choices = new string[names.Length + 1];
                        names.CopyTo(choices, 0);
                        choices[names.Length] = "Done";

                        string inmate = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[yellow]Select an inmate to change the inventory of[/] : ")
                                .AddChoices(choices)
                        );

                        if (inmate.Trim() == "Done")
                        {
                            break; // Exit to edit selection screen
                        }

                        int inmateNum = GetFirstNum(inmate, '.');

                        AnsiConsole.MarkupLine("[bold yellow]Current items[/] : ");

                        string[] inmateInvArray = new string[8];
                        for (int i = 1; i <= 6; i++)
                        {
                            inmateInvArray[i - 1] = i + ". " + GetCurrent("Inmate_Inven", inmateNum.ToString(), i + 1);
                            AnsiConsole.MarkupLine(i + ". " + GetCurrent("Inmate_Inven", inmateNum.ToString(), i + 1));
                        }
                        inmateInvArray[6] = "Done";
                        inmateInvArray[7] = "Select a different inmate to change the inventory of";

                        string slot = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Select an item to edit : ")
                                .AddChoices<string>(inmateInvArray)
                        );

                        if(slot == "Done")
                        {
                            Console.Clear();
                            break;
                        }
                        else if(slot == "Select a different inmate to change the inventory of")
                        {
                            Console.Clear();
                            continue;
                        }

                        int selectedSlot = GetFirstNum(slot, '.');
                        int subSelection = selectedSlot + 1;

                        AnsiConsole.MarkupLine("[bold yellow]Current item[/] : " + GetCurrent("Inmate_Inven", inmateNum.ToString(), subSelection));
                        Console.WriteLine();
                        AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                        string item = AnsiConsole.Prompt(
                            new TextPrompt<string>("[yellow]New item[/] :"));

                        EditSave("Inmate_Inven", inmateNum.ToString(), item, subSelection);
                        Console.Clear();
                    }
                    break;
                case "Guard Inventories":
                    while (true) // guard selection loop
                    {
                        string[] choices = new string[aNames.Length + 1];
                        aNames.CopyTo(choices, 0);
                        choices[aNames.Length] = "Done";

                        string guard = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[yellow]Select a guard to change the inventory of[/] : ")
                                .AddChoices(choices)
                        );

                        if (guard.Trim() == "Done")
                        {
                            break; // Exit to edit selection screen
                        }

                        int guardNum = GetFirstNum(guard, '.');

                        AnsiConsole.MarkupLine("[bold yellow]Current items[/] : ");

                        while (true) // slot selection loop
                        {
                            string[] guardInvArray = new string[8];
                            for (int i = 1; i <= 6; i++)
                            {
                                guardInvArray[i - 1] = i + ". " + GetCurrent("Guard_Inven", guardNum.ToString(), i + 1);
                                AnsiConsole.MarkupLine(i + ". " + GetCurrent("Guard_Inven", guardNum.ToString(), i + 1));
                            }
                            guardInvArray[6] = "Done";
                            guardInvArray[7] = "Select a different guard to change the inventory of";

                            string slot = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Select an item to edit : ")
                                    .AddChoices<string>(guardInvArray)
                            );

                            if (slot.Trim() == "Done" || slot == "Select a different guard to change the inventory of")
                            {
                                Console.Clear();
                                break; // Return to guard selection
                            }

                            int selectedSlot = GetFirstNum(slot, '.');
                            int subSelection = selectedSlot + 1;

                            AnsiConsole.MarkupLine("[bold yellow]Current item[/] : " + GetCurrent("Guard_Inven", guardNum.ToString(), subSelection));
                            Console.WriteLine();
                            AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                            string item = AnsiConsole.Prompt(
                                new TextPrompt<string>("[yellow]New item[/] :"));

                            EditSave("Guard_Inven", guardNum.ToString(), item, subSelection);
                            Console.Clear();
                        }
                    }
                    break;
                case "Desks":
                    while (true)
                    {
                        string[] choices = new string[names.Length + 2];
                        choices[0] = GetCurrent("Player", "Name") + "'s Desk";
                        for (int i = 0; i < names.Length; i++)
                        {
                            choices[i + 1] = names[i].Split(new[] { ". " }, 2, StringSplitOptions.None)[1] + "'s Desk";
                        }
                        choices[names.Length + 1] = "Done";

                        string desk = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[yellow]Select a desk to change the contents of[/] : ")
                                .AddChoices(choices)
                        );

                        if (desk == "Done")
                        {
                            break;
                        }

                        int deskNum = Array.IndexOf(choices, desk);

                        string[] deskArray = new string[22];

                        for (int i = 0; i < 20; i++)
                        {
                            deskArray[i] = (i + 1) + ". " + GetDeskCurrent(deskNum.ToString(), i + 1);
                        }
                        deskArray[20] = "Select a different desk to change the contents of";
                        deskArray[21] = "Done";

                        string slot = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("[yellow]Select an item to edit[/] : ")
                                .AddChoices(deskArray)
                        );

                        if (slot == "Select a different desk to change the contents of")
                        {
                            continue;
                        }
                        else if (slot == "Done")
                        {
                            break;
                        }

                        AnsiConsole.MarkupLine("[bold yellow]Current item[/] : " + GetDeskCurrent(deskNum.ToString(), GetFirstNum(slot, '.')));
                        Console.WriteLine();
                        AnsiConsole.MarkupLine("[silver](For a list of items and their respective ID's, check the [green]Key.txt[/] file in this program's folder.)[/]");
                        string item = AnsiConsole.Prompt(
                            new TextPrompt<string>("[yellow]New item[/] :"));
                        Console.Clear();
                    }
                    break;

            }
            Console.Clear();
        }
        private static int GetFirstNum(string str, char separator)
        {
            int equalsIndex = str.IndexOf(separator);
            string numberPart = str.Substring(0, equalsIndex);
            int number = int.Parse(numberPart);
            return number;
        }
        private static string GetCurrent(string category, string selection)
        {
            string importantLine = null;

            int lineNum = -1;
            foreach (string line in saveFile)
            {
                lineNum++;
                if (line == "[" + category + "]")
                {
                    lineNum--;
                    foreach (string line2 in saveFile)
                    {
                        lineNum++;
                        if (line2.StartsWith(selection))
                        {
                            importantLine = line2;
                        }
                    }
                }
            }

            string[] parts = importantLine.Split('=');

            if (parts[1].Contains('_'))
            {
                return parts[1].Split('_')[0];
            }
            else
            {
                return parts[1];
            }
        }
        private static string GetCurrent(string category, string selection, int subSelection)
        {
            string importantLine = null;
            bool inCategory = false;

            foreach (string line in saveFile)
            {
                if (line == "[" + category + "]")
                {
                    inCategory = true;
                    continue;
                }
                if (inCategory)
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                        break;
                    if (line.StartsWith(selection))
                    {
                        importantLine = line;
                        break;
                    }
                }
            }

            var split = importantLine.Split('=');

            var subParts = split[1].Split('@');

            string value = subParts[subSelection - 1];

            if (value.Contains('_'))
                return value.Split('_')[0];
            else
                return value;
        }
        private static string GetDeskCurrent(string selection, int subSelection)
        {
            string importantLine = null;
            bool inCategory = false;

            foreach (string line in saveFile)
            {
                if (line == "[Desks]")
                {
                    inCategory = true;
                    continue;
                }
                if (inCategory)
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        break;
                    }
                    if (line.StartsWith(selection))
                    {
                        importantLine = line;
                        break;
                    }
                }
            }

            if (importantLine == null)
                return "";

            int lastAtIndex = importantLine.LastIndexOf('@');
            if (lastAtIndex == -1)
                return "";

            string suffix = importantLine.Substring(lastAtIndex + 1);
            string[] items = suffix.Split('?');

            // subSelection is 1-based, so the value after is at subSelection (0-based)
            if (subSelection < 1 || subSelection >= items.Length)
                return "";

            if (items[subSelection].Contains('_'))
                return items[subSelection].Split('_')[0];
            else
                return items[subSelection];

        }
        private static void EditSave(string category, string selection, string edit)
        {
            int lineNum = -1;
            string importantLine = null;

            bool inCategory = false;
            for (int i = 0; i < saveFile.Length; i++)
            {
                if (saveFile[i] == "[" + category + "]")
                {
                    inCategory = true;
                    continue;
                }
                if (inCategory)
                {
                    if (saveFile[i].StartsWith("[") && saveFile[i].EndsWith("]"))
                        break;
                    if (saveFile[i].StartsWith(selection))
                    {
                        lineNum = i;
                        importantLine = saveFile[i];
                        break;
                    }
                }
            }
            string[] parts = importantLine.Split('=');

            if (parts[1].Contains('_'))
            {
                parts[1] = edit + "_100";
            }
            else
            {
                parts[1] = edit;
            }

            saveFile[lineNum] = parts[0] + "=" + parts[1];
        }
        private static void EditSave(string category, string selection, string edit, int subSelection)
        {
            int lineNum = -1;
            string importantLine = null;
            bool inCategory = false;

            for (int i = 0; i < saveFile.Length; i++)
            {
                if (saveFile[i] == "[" + category + "]")
                {
                    inCategory = true;
                    continue;
                }
                if (inCategory)
                {
                    if (saveFile[i].StartsWith("[") && saveFile[i].EndsWith("]"))
                        break;
                    if (saveFile[i].StartsWith(selection))
                    {
                        lineNum = i;
                        importantLine = saveFile[i];
                        break;
                    }
                }
            }

            var split = importantLine.Split('=');

            var subParts = split[1].Split('@');

            if (subParts[subSelection - 1].Contains('_'))
                subParts[subSelection - 1] = edit + "_100";
            else
                subParts[subSelection - 1] = edit;

            saveFile[lineNum] = split[0] + "=" + string.Join("@", subParts);
        }

        private static void DrawLogo()
        {
            AnsiConsole.MarkupLine("[darkblue]TTTTTTTTTTTTTTTTTTTTTTTEEEEEEEEEEEEEEEEEEEEEE   SSSSSSSSSSSSSSS EEEEEEEEEEEEEEEEEEEEEE\r\nT:::::::::::::::::::::TE::::::::::::::::::::E SS:::::::::::::::SE::::::::::::::::::::E\r\nT:::::::::::::::::::::TE::::::::::::::::::::ES:::::SSSSSS::::::SE::::::::::::::::::::E\r\nT:::::TT:::::::TT:::::TEE::::::EEEEEEEEE::::ES:::::S     SSSSSSSEE::::::EEEEEEEEE::::E\r\nTTTTTT  T:::::T  TTTTTT  E:::::E       EEEEEES:::::S              E:::::E       EEEEEE\r\n        T:::::T          E:::::E             S:::::S              E:::::E             \r\n        T:::::T          E::::::EEEEEEEEEE    S::::SSSS           E::::::EEEEEEEEEE   \r\n        T:::::T          E:::::::::::::::E     SS::::::SSSSS      E:::::::::::::::E   \r\n        T:::::T          E:::::::::::::::E       SSS::::::::SS    E:::::::::::::::E   \r\n        T:::::T          E::::::EEEEEEEEEE          SSSSSS::::S   E::::::EEEEEEEEEE   \r\n        T:::::T          E:::::E                         S:::::S  E:::::E             \r\n        T:::::T          E:::::E       EEEEEE            S:::::S  E:::::E       EEEEEE\r\n      TT:::::::TT      EE::::::EEEEEEEE:::::ESSSSSSS     S:::::SEE::::::EEEEEEEE:::::E\r\n      T:::::::::T      E::::::::::::::::::::ES::::::SSSSSS:::::SE::::::::::::::::::::E\r\n      T:::::::::T      E::::::::::::::::::::ES:::::::::::::::SS E::::::::::::::::::::E\r\n      TTTTTTTTTTT      EEEEEEEEEEEEEEEEEEEEEE SSSSSSSSSSSSSSS   EEEEEEEEEEEEEEEEEEEEEE[/]");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}