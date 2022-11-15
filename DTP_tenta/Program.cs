using System.Collections.Generic;
using System.IO;
using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace DTP_tenta
{
    public class Todo
    {
        public static string loadedFile = null;
        public static List<TodoItem> list = new List<TodoItem>();

        public const int Active = 1;
        public const int Waiting = 2;
        public const int Ready = 3;
        public static string StatusToString(int status)
        {
            switch (status)
            {
                case Active: return "aktiv";
                case Waiting: return "väntande";
                case Ready: return "avklarad";
                default: return "(felaktig)";
            }
        }
        public class TodoItem
        {
            public int status;
            public int priority;
            public string task;
            public string taskDescription;
            public TodoItem(int priority, string task)
            {
                this.status = Active;
                this.priority = priority;
                this.task = task;
                this.taskDescription = "";
            }
            public TodoItem(string todoLine)
            {
                string[] field = todoLine.Split('|');
                status = Int32.Parse(field[0]);
                try
                {
                    priority = Int32.Parse(field[1]);
                }
                catch
                {
                    Console.WriteLine("Prioritet var ett felaktigt värde. Vänligen ange i prioriterinsordning 1-3");
                    return;
                }
                task = field[2];
                taskDescription = field[3];
            }
            public void Print(string command, bool verbose = false)
            {
                string statusString = StatusToString(status);
                Console.Write($"|{statusString,-12}|{priority,-6}|{task,-26}|");
                if (command == "beskriv" || command == "beskriv allt")
                    Console.WriteLine($"{taskDescription,-80}|");
                else
                    Console.WriteLine();
            }
        }
        public static void ReadListFromFile(string command)
        {
            string todoFileName = saveOrLoadFile(command);
            loadedFile = todoFileName;
            if (File.Exists(todoFileName))
            {
                Console.Write($"Läser från fil {todoFileName} ... ");
                StreamReader sr = new StreamReader(todoFileName);
                int numRead = 0;

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    TodoItem item = new TodoItem(line);
                    list.Add(item);
                    numRead++;
                }
                sr.Close();
                Console.WriteLine($"Läste {numRead} rader.");
            }
            else
                Console.WriteLine("Filen fanns inte");
        }

        private static string saveOrLoadFile(string command)
        {
            string status = command.Trim();
            string[] cwords = status.Split(' ');
            string todoFileName = "";
            if (cwords.Length == 1)
                todoFileName = "todo.lis";
            else if (cwords.Length == 2)
                todoFileName = $"{cwords[1]}";

            return todoFileName;
        }
        public static void saveListToFile(string command)
        {
            string todoFileName = saveOrLoadFile(command);
            Console.WriteLine($"Sparar i fil {todoFileName}...");
            int numSaved = 0;
            using (TextWriter sr = new StreamWriter(todoFileName))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string line = $"{list[i].status}|{list[i].priority}|{list[i].task}|{list[i].taskDescription}";
                    sr.WriteLine(line);
                    numSaved++;
                }
            }
            Console.WriteLine($"Sparade {numSaved} rader.");
        }
        private static void PrintHeadOrFoot(string command, bool head, bool verbose)
        {
            if (head)
            {
                Console.Write("|status      |prio  |namn                      |");
                if (command == "beskriv" || command == "beskriv allt") Console.WriteLine("beskrivning                                                                     |");
                else Console.WriteLine();
            }
            Console.Write("|------------|------|--------------------------|");
            if (command == "beskriv" || command == "beskriv allt") Console.WriteLine("--------------------------------------------------------------------------------|");
            else Console.WriteLine();
        }
        private static void PrintHead(string command, bool verbose)
        {
            PrintHeadOrFoot(command, head: true, verbose);
        }
        private static void PrintFoot(string command, bool verbose)
        {
            PrintHeadOrFoot(command, head: false, verbose);
        }
        public static void PrintTodoList(string command, bool verbose = false)
        {
            if (Todo.list.Count == 0) Console.WriteLine("Ingen information i databasen. Ladda en fil för att fortsätta");
            else
            {
                PrintHead(command, verbose);
                if (!verbose)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (Todo.list[i].status == Active)
                        {
                            Todo.list[i].Print(command);
                        }
                    }
                }
                else
                {
                    foreach (TodoItem item in list)
                    {
                        item.Print(command);
                    }
                }
                PrintFoot(command, verbose);
            }
        }
        public static void PrintHelp()
        {
            Console.WriteLine("Kommandon:");
            Console.WriteLine("hjälp                  lista denna hjälp");
            Console.WriteLine("ny                     skapa en ny uppgift (sätts till aktiv)");
            Console.WriteLine("ny /uppgift/           skapa en ny uppgift med namnet /uppgift/ (sätts till aktiv)");
            Console.WriteLine("redigera /uppgift/     redigera en uppgift med namnet /uppgift/");
            Console.WriteLine("kopiera /uppgift/      kopiera en uppgift med namnet /uppgift/");
            Console.WriteLine("beskriv                lista beskrivningar (visar enbart aktiva uppgifter)");
            Console.WriteLine("beskriv allt           lista beskrivningar (visar alla uppgifter uppgifter)");
            Console.WriteLine("lista                  lista att-göra-listan (visar enbart aktiva uppgifter)");
            Console.WriteLine("lista allt             lista att-göra-listan (visar alla uppgifter)");
            Console.WriteLine("ladda                  laddar filen todo.lis");
            Console.WriteLine("ladda /fil/            ladda filen /fil/");
            Console.WriteLine("spara                  sparar filen todo.lis");
            Console.WriteLine("spara /fil/            sparar filen /fil/");
            Console.WriteLine("aktivera /uppgift/     sätt status på en uppgift till 'aktiv'");
            Console.WriteLine("klar /uppgift/         sätt status på en uppgift till 'avklarad'");
            Console.WriteLine("vänta /uppgift/        sätt status på en uppgift till 'väntande'");
            Console.WriteLine("sluta                  spara att-göra-listan och sluta");
        }
        public static void newEntryOrEdit(string command)
        {
            string status = command.Trim();
            string[] cwords = status.Split(' ');
            string line = "";
            if (cwords[0] == "ny")
            {
                if (cwords.Length == 1)
                {
                    string uppgNamn = MyIO.ReadCommand("Uppgiftens namn: ");
                    string uppgift = uppgNamn.Trim();
                    string[] uppgiftArr = uppgift.Split(' ');
                    do
                    {
                        if (uppgiftArr.Length != 2)
                        {
                            Console.WriteLine("Uppgiften måste innehålla två ord (ex Köpa Kaffe)");
                            break;
                        }
                        string priority = MyIO.ReadCommand("Prioritet: ");
                        string beskrivning = MyIO.ReadCommand("Beskrivning: ");
                        line = $"1|{priority}|{uppgNamn}|{beskrivning}";
                        TodoItem item = new TodoItem(line);
                        list.Add(item);
                    } while (false);
                }
                else if (cwords.Length == 3)
                {
                    string priority = MyIO.ReadCommand("Prioritet: ");
                    string beskrivning = MyIO.ReadCommand("Beskrivning: ");
                    line = $"1|{priority}|{cwords[1]} {cwords[2]}|{beskrivning}";
                    TodoItem item = new TodoItem(line);
                    list.Add(item);
                }
            }
            else if (cwords.Length != 3 && cwords.Length != 1)
            {
                Console.WriteLine("Uppgiften måste innehålla två ord (ex Köpa Kaffe)");
            }
            else if (cwords[0] == "redigera")
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string uppgift = $"{cwords[1]} {cwords[2]}";
                    if (uppgift == list[i].task.ToLower())
                    {
                        Console.WriteLine("Tryck [enter] om du inte vill ändra!");
                        string uppgNamn = MyIO.ReadCommand($"Uppgiftens namn ({list[i].task}): ");
                        string uppgift2 = uppgNamn.Trim();
                        string[] cwords2 = uppgift2.Split(' ');
                        if (cwords2.Length == 2)
                            list[i].task = uppgNamn;
                        else if (cwords2.Length != 2 && uppgNamn != "")
                        {
                            Console.WriteLine("Uppgiften måste innehålla två ord (ex Köpa Kaffe)");
                            break;
                        }
                        string status2 = MyIO.ReadCommand($"Status 'aktiv/klar/vänta' ({StatusToString(list[i].status)}): ");
                        string priority = MyIO.ReadCommand($"Prioritet ({list[i].priority}): ");
                        string beskrivning = MyIO.ReadCommand($"Beskrivning ({list[i].taskDescription}): ");
                        if (status2 != "")
                        {
                            if (status2 == "klar")
                                status2 = "3";
                            else if (status2 == "aktiv")
                                status2 = "1";
                            else if (status2 == "vänta")
                                status2 = "2";
                            list[i].status = Convert.ToInt32(status2);
                        }
                        if (priority != "")
                            list[i].priority = Convert.ToInt32(priority);
                        if (beskrivning != "")
                            list[i].taskDescription = beskrivning;
                    }
                }
            }
        }
        public static void changeStatusOrCopy(string command)
        {
            string status = command.Trim();
            string[] cwords = status.Split(' ');
            bool found = false;
            if (cwords.Length == 1)
                Console.WriteLine("Du måste ange en uppgift");
            else if (cwords.Length == 3)
            {
                string uppgift = $"{cwords[1]} {cwords[2]}";
                for (int i = 0; i < list.Count; i++)
                {
                    if (uppgift == list[i].task.ToLower())
                    {
                        if (cwords[0] == "aktivera")
                        {
                            list[i].status = Todo.Active;
                            Console.WriteLine($"Du ändrade status på: '{Todo.list[i].task}' till 'aktiv'");
                            found = true;

                        }
                        else if (cwords[0] == "klar")
                        {
                            list[i].status = Todo.Ready;
                            Console.WriteLine($"Du ändrade status på: '{Todo.list[i].task}' till 'avklarad'");
                            found = true;
                        }
                        else if (cwords[0] == "vänta")
                        {
                            list[i].status = Todo.Waiting;
                            Console.WriteLine($"Du ändrade status på: '{Todo.list[i].task}' till 'väntande'");
                            found = true;
                        }
                        else if (cwords[0] == "kopiera")
                        {
                            string line = $"{Todo.Active}|{list[i].priority}|{list[i].task}, 2|{list[i].taskDescription}";
                            TodoItem item = new TodoItem(line);
                            list.Add(item);
                            found = true;
                            Console.WriteLine($"Du kopierade {list[i].task}");
                        }
                    }
                }
            }
            if (!found && cwords.Length > 1)
                Console.WriteLine($"Uppgiften du angav finns inte. Testa igen");
        }
        class MainClass
        {
            public static void Main(string[] args)
            {
                Console.SetWindowSize(150, 30);
                Console.WriteLine("Välkommen till att-göra-listan!");
                Todo.PrintHelp();
                string command;
                do
                {
                    command = MyIO.ReadCommand("> ").ToLower();
                    if (MyIO.Equals(command, "hjälp"))
                    {
                        Todo.PrintHelp();
                    }
                    else if (MyIO.Equals(command, "sluta"))
                    {
                        Console.WriteLine("Hej då!");
                        Todo.saveListToFile($"spara {Todo.loadedFile}");
                        break;
                    }
                    else if (MyIO.Equals(command, "ny"))
                    {
                        Todo.newEntryOrEdit(command);
                    }
                    else if (MyIO.Equals(command, "redigera"))
                    {
                        Todo.newEntryOrEdit(command);
                    }
                    else if (MyIO.Equals(command, "kopiera"))
                    {
                        changeStatusOrCopy(command);
                    }
                    else if (MyIO.Equals(command, "spara"))
                    {
                        Todo.saveListToFile(command);
                    }
                    else if (MyIO.Equals(command, "ladda"))
                    {
                        Todo.ReadListFromFile(command);
                    }
                    else if (MyIO.Equals(command, "beskriv"))
                    {
                        if (MyIO.HasArgument(command, "allt"))
                            Todo.PrintTodoList(command, verbose: true);
                        else
                            Todo.PrintTodoList(command, verbose: false);
                    }
                    else if (MyIO.Equals(command, "lista"))
                    {
                        if (MyIO.HasArgument(command, "allt"))
                            Todo.PrintTodoList(command, verbose: true);
                        else
                            Todo.PrintTodoList(command, verbose: false);
                    }
                    else if (MyIO.Equals(command, "aktivera"))
                    {
                        changeStatusOrCopy(command);
                    }
                    else if (MyIO.Equals(command, "vänta"))
                    {
                        changeStatusOrCopy(command);
                    }
                    else if (MyIO.Equals(command, "klar"))
                    {
                        changeStatusOrCopy(command);
                    }
                    else
                    {
                        Console.WriteLine($"Okänt kommando: {command}");
                    }
                }
                while (true);
            }
        }
        class MyIO
        {
            static public string ReadCommand(string prompt)
            {
                Console.Write(prompt);
                return Console.ReadLine();
            }
            static public bool Equals(string rawCommand, string expected)
            {
                string command = rawCommand.Trim();
                if (command == "") return false;
                else
                {
                    string[] cwords = command.Split(' ');
                    if (cwords[0] == expected) return true;
                }
                return false;
            }
            static public bool HasArgument(string rawCommand, string expected)
            {
                string command = rawCommand.Trim();
                if (command == "") return false;
                else
                {
                    string[] cwords = command.Split(' ');
                    if (cwords.Length < 2) return false;
                    if (cwords[1] == expected) return true;
                }
                return false;
            }
        }
    }
}
