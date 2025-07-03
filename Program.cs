Console.OutputEncoding = System.Text.Encoding.UTF8;

List<Transaction> transactions = new();
char cunit = '€';

// Read Settings
string[] settings = File.ReadAllLines(@"..\..\..\Settings.txt");

// More to come
for (int i = 0; i < settings.Length; i++)
{
    string[] part = settings[i].Split(':');
    if (part.Length != 2)
    {
        Console.WriteLine($"INVALID SETTING FILE SYNTAX AT LINE {i + 1}!!!");
    }
    else
    {
        if (part[0].Trim() == "currency_unit")
        {
            if (part[1].Trim().ToLower() == "default")
                cunit = '€';
            else
                cunit = part[1].Trim()[0];
        }
    }
}

// To Come Soon: A CMD-Line like way to use all of the previously added commands. A way to edit already entered Transaction objects.
// Main
while (true)
{
    Console.WriteLine("Please choose an action!");
    Console.WriteLine("1. Add Income Source or Expense");
    Console.WriteLine("2. Show net balance");
    Console.WriteLine("3. Show transaction history");
    Console.WriteLine("4. Save data to file");
    Console.WriteLine("5. Read input file into memory");
    Console.WriteLine("6. Show transaction history after applying a filter");
    Console.WriteLine("7. Delete Entry");
    Console.WriteLine("0. Exit the application");
    Console.WriteLine("CLR to clear the window");
    Console.Write("Enter num of wanted command --> ");
    string user = Console.ReadLine()!;
    if (user.ToLower().Trim() == "clr")
    {
        Console.WriteLine("Clearing Console...");
        Timeout();
        Console.Clear();
        continue;
    }
    bool ok = int.TryParse(user, out int cmd);
    Console.WriteLine();
    if (!ok)
    {
        Console.WriteLine("Invalid Input!");
        Timeout();
        Console.Clear();
        continue;
    }

    if (cmd == 1)
    {
        transactions = AddSource(transactions);
    }
    else if (cmd == 2)
    {
        double netBalance = CalculateNetBalance(transactions);
        Console.WriteLine("Your current net balance is " + netBalance);
        Console.WriteLine();
    }
    else if (cmd == 3)
    {
        ShowTransactionHistory(transactions, cunit);
    }
    else if (cmd == 4)
        SaveData(transactions);
    else if (cmd == 5)
    {
        List<Transaction>? newTrList = ReadData();
        if (newTrList != null)
            transactions = newTrList;
    }
    else if (cmd == 6)
    {
        ShowFilteredHistory(transactions, cunit);
    }
    else if (cmd == 7)
        transactions = DeleteEntry(transactions);
    else if (cmd == 0)
        return;
    else
    {
        Console.WriteLine("Invalid Input!");
        Timeout();
        Console.Clear();
    }


}

static List<Transaction> AddSource(List<Transaction> prevList)
{

    string name = ReadString("Enter name of the income source/expense");
    double value = ReadDouble("Enter the amount of money made/lost by the income source/expense (for expenses make the numbers negative)");
    string category = ReadString("Enter the category of the income source/expense");
    string desc = ReadString("Enter the description of the income source/expense");
    DateOnly date = ReadDate("Enter date of the income source/expense");
    Console.WriteLine();

    Transaction tr = new()
    {
        Name = name,
        Category = category,
        Description = desc,
        Amount = value,
        Date = date
    };

    prevList.Add(tr);

    return prevList;
}

static double CalculateNetBalance(List<Transaction> transactions)
{
    double output = 0;
    for (int i = 0; i < transactions.Count; i++)
        output += transactions[i].Amount;
    return output;
}

static void ShowTransactionHistory(List<Transaction> transactionsPrev, char cunit = '€')
{
    List<Transaction> transactions = transactionsPrev.ToList();
    const byte CURRENTMAXCMD = 3;
    int user;
    while (true)
    {
        Console.WriteLine("Please enter the wanted sorting order!");
        Console.WriteLine("1. Date");
        Console.WriteLine("2. Name");
        Console.WriteLine("3. Amount");
        user = ReadInt("Wanted command");
        Console.WriteLine();
        if (user >= 1 && user <= CURRENTMAXCMD)
            break;
        else
        {
            Console.WriteLine("Invalid Input | Input out of Range");
            Timeout();
            Console.Clear();
        }
    }
    Console.WriteLine("---------------------------------------------------------------------------------");
    Console.WriteLine($"| {"Name",-15} | {"Amount",-11} | {"Category",-14} | {"Description",-15} | {"Date",-10} |");
    Console.WriteLine("---------------------------------------------------------------------------------");
    if (user == 1)
    {
        int length = transactions.Count;
        DateOnly smallestDate = DateOnly.MaxValue;
        int indexOfSmallest = -1;

        for (int j = 0; j < length; j++)
        {
            // Find earliest Date
            for (int i = 0; i < transactions.Count; i++)
            {
                if (transactions[i].Date < smallestDate)
                {
                    smallestDate = transactions[i].Date;
                    indexOfSmallest = i;
                }
            }

            Transaction tr = transactions[indexOfSmallest];
            WriteLineOfHistory(tr);
            transactions.Remove(tr);

            smallestDate = DateOnly.MaxValue;
            indexOfSmallest = -1;
        }
    }
    else if (user == 2)
    {
        int length = transactions.Count;
        int smallestSumOfCharVals = int.MaxValue;
        int indexOfSmallest = -1;
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < transactions.Count; j++)
            {
                string name = transactions[j].Name;
                int currentSum = 0;
                for (int x = 0; x < name.Length; x++)
                {
                    currentSum += (x + 1) * (name.ToLower()[x] - 'z');
                }
                if (currentSum < smallestSumOfCharVals)
                {
                    smallestSumOfCharVals = currentSum;
                    indexOfSmallest = j;
                }


            }

            Transaction tr = transactions[indexOfSmallest];
            WriteLineOfHistory(tr);
            transactions.Remove(tr);

            smallestSumOfCharVals = int.MaxValue;
            indexOfSmallest = -1;

        }
    }
    else if (user == 3)
    {
        int length = transactions.Count;
        double smallestAmount = double.MaxValue;
        int indexOfSmallest = -1;

        for (int j = 0; j < length; j++)
        {
            // Find smallest Amount
            for (int i = 0; i < transactions.Count; i++)
            {
                if (transactions[i].Amount < smallestAmount)
                {
                    smallestAmount = transactions[i].Amount;
                    indexOfSmallest = i;
                }
            }

            Transaction tr = transactions[indexOfSmallest];
            WriteLineOfHistory(tr);
            transactions.Remove(tr);

            smallestAmount = double.MaxValue;
            indexOfSmallest = -1;
        }
    }
    Console.WriteLine("---------------------------------------------------------------------------------");
    Console.WriteLine();
}

static void SaveData(List<Transaction> transactions)
{
    // Name,Amount,Category,Description,Date
    string[] fileOutput = new string[transactions.Count];
    for (int i = 0; i < transactions.Count; i++)
    {
        Transaction tr = transactions[i];
        fileOutput[i] = $"{tr.Name},{tr.Amount},{tr.Category},{tr.Description},{tr.Date}";
    }
    string path = ReadString("Please enter the wanted file path (use ~ for default)").Trim();
    if (path == "~")
        path = @"..\..\..\";

    string fName = ReadString("Please enter the wanted file name");
    File.WriteAllLines(path + fName + ".txt", fileOutput);
    Console.WriteLine("Successfully wrote the output file at " + Path.GetFullPath(path + fName + ".txt"));
    Console.WriteLine();
}

static List<Transaction>? ReadData()
{
    string path = ReadString("Please enter the file path of the input file (use ~ for default)").Trim();
    if (path == "~")
        path = @"..\..\..\";

    string fName = ReadString("Please enter the name of the file");

    if (!File.Exists(path + fName + ".txt"))
    {
        Console.WriteLine("Couldn't find path or file! Please check your inputs and try again!");
        Console.WriteLine();
        return null;
    }
    // Name,Amount,Category,Description,Date
    string[] fileContent = File.ReadAllLines(path + fName + ".txt");
    List<Transaction> output = new List<Transaction>();
    for (int i = 0; i < fileContent.Length; i++)
    {
        string[] part = fileContent[i].Split(',');
        if (part.Length != 5)
        {
            Console.WriteLine($"LINE {i + 1} IS MALFORMED! PLEASE CHECK INPUT FILE! Skipping Line...");
            continue;
        }
        Transaction tr = new Transaction();

        tr.Name = part[0];

        bool ok = double.TryParse(part[1], out double value);
        if (!ok)
        {
            Console.WriteLine($"LINE {i + 1} IS MALFORMED! PLEASE CHECK INPUT FILE! Skipping Line...");
            continue;
        }

        tr.Amount = value;
        tr.Category = part[2];
        tr.Description = part[3];

        ok = DateOnly.TryParse(part[4], out DateOnly date);
        if (!ok)
        {
            Console.WriteLine($"LINE {i + 1} IS MALFORMED! PLEASE CHECK INPUT FILE! Skipping Line...");
            continue;
        }

        tr.Date = date;

        output.Add(tr);
    }
    Console.WriteLine("Successfully read input file!");
    Console.WriteLine();
    return output;
}

static void ShowFilteredHistory(List<Transaction> transactions, char cunit = '€')
{
    Console.WriteLine($"Select a filter option for each category. Leave the filter blank to not use that option. For amount enter 0 and for date {DateOnly.MinValue}");
    string name = ReadString("Name");
    double value = ReadDouble("Value");
    string category = ReadString("Category");
    string description = ReadString("Description");
    DateOnly date = ReadDate("Date");

    Console.WriteLine("---------------------------------------------------------------------------------");
    Console.WriteLine($"| {"Name",-15} | {"Amount",-11} | {"Category",-14} | {"Description",-15} | {"Date",-10} |");
    Console.WriteLine("---------------------------------------------------------------------------------");

    for (int i = 0; i < transactions.Count; i++)
    {
        if ((transactions[i].Name.Contains(name) || string.IsNullOrWhiteSpace(name)) && (transactions[i].Amount == value || value == 0) && (transactions[i].Category == category || string.IsNullOrWhiteSpace(category)) && (transactions[i].Description.Contains(description) || string.IsNullOrWhiteSpace(description)) && (transactions[i].Date == date || date == DateOnly.MinValue))
        {
            WriteLineOfHistory(transactions[i], cunit);
        }
    }
    Console.WriteLine("---------------------------------------------------------------------------------");
    Console.WriteLine();
}
static void Timeout(int time = 1500)
{
    Thread.Sleep(time);
}

static void ColoredWrite(string text, ConsoleColor color = ConsoleColor.Red, bool newLine = true)
{
    Console.ForegroundColor = color;
    if (newLine)
        Console.WriteLine(text);
    else
        Console.Write(text);
    Console.ResetColor();
}

static void WriteLineOfHistory(Transaction tr, char cunit = '€')
{
    if (tr.Amount < 0)
    {
        Console.Write($"| {tr.Name,-15} | ");
        ColoredWrite($"{tr.Amount,-9:F2} {cunit} ", newLine: false);
        Console.WriteLine($"| {tr.Category,-14} | {tr.Description,-15} | {tr.Date,-10} |");
    }
    else
        Console.WriteLine($"| {tr.Name,-15} | {tr.Amount,-9:F2} {cunit} | {tr.Category,-14} | {tr.Description,-15} | {tr.Date,-10} |");
}

static void WriteIndexedLineOfHistory(Transaction tr, int index, char cunit = '€')
{
    if (tr.Amount < 0)
    {
        Console.Write($"{index} | {tr.Name,-15} | ");
        ColoredWrite($"{tr.Amount,-9:F2} {cunit} ", newLine: false);
        Console.WriteLine($"| {tr.Category,-14} | {tr.Description,-15} | {tr.Date,-10} |");
    }
    else
        Console.WriteLine($"{index} | {tr.Name,-15} | {tr.Amount,-9:F2} {cunit} | {tr.Category,-14} | {tr.Description,-15} | {tr.Date,-10} |");
}

static List<Transaction> DeleteEntry(List<Transaction> currentList)
{
    const byte CURRENTMAXCMD = 6;

    if (currentList.Count == 0)
    {
        Console.WriteLine("The transaction list is empty! Returning to main menu...");
        Console.WriteLine();
        return currentList;
    }

    int user;
    while (true)
    {
        Console.WriteLine("Select the wanted criteria for deletion!");
        Console.WriteLine("1. Index");
        Console.WriteLine("2. Name");
        Console.WriteLine("3. Amount");
        Console.WriteLine("4. Category");
        Console.WriteLine("5. Description");
        Console.WriteLine("6. Date");

        user = ReadInt("Selection");
        if (user >= 1 && user <= CURRENTMAXCMD)
        {
            break;
        }
        Console.WriteLine("Invalid Input!");
        Timeout();
        Console.Clear();
    }

    // Index
    if (user == 1)
    {
        while (true)
        {
            PrintList(currentList);
            string deletionsAsString = ReadString("Enter the wanted deletion number or range (1 | 2 | 1-6 | ...). Enter e to exit to main menu");
            if (deletionsAsString.Trim() == "e")
                return currentList;
            string[] parts = deletionsAsString.Split("-");
            // Range
            if (parts.Length < 3)
            {
                bool ok = int.TryParse(parts[0], out int startIndex);
                if (!ok)
                {
                    Error();
                    continue;
                }
                // Convert to index
                startIndex--;

                int endIndex;
                if (parts.Length == 1)
                {
                    endIndex = startIndex;
                }
                else
                {
                    ok = int.TryParse(parts[1], out endIndex);
                    if (!ok)
                    {
                        Error();
                        continue;
                    }
                    endIndex--;
                }
                // Convert to index


                int amountRemove = endIndex - startIndex;

                if (amountRemove < 0)
                {
                    Console.WriteLine("Endindex cannot be smaller than startindex!");
                    Error(false);
                    continue;
                }

                if (endIndex >= currentList.Count || startIndex < 0)
                {
                    Console.WriteLine("The start- or endindex is out of bounds!");
                    continue;
                }

                for (int i = 0; i <= amountRemove; i++)
                    currentList.Remove(currentList[startIndex]);

                Console.WriteLine("List after removal of number/range:");
                PrintList(currentList);
                return currentList;
            }
            // Invalid
            else
            {
                Console.WriteLine("Too many dashes. Use format: '1' or '2-4'.");
                Error(false);
                continue;
            }
        }

    }
    // Name
    else if (user == 2)
    {
        while (true)
        {
            PrintList(currentList);
            string name = ReadString("Enter the name of the object you want to delete or e to exit").Trim();
            if (name == "e")
                return currentList;

            int amountRemoved = currentList.RemoveAll(t => t.Name == name);

            Console.Write("Removed " + amountRemoved + " ");
            if (amountRemoved != 1)
                Console.WriteLine("entries from list.");
            else
                Console.WriteLine("entry from list.");

            Console.WriteLine("List after deletions:");
            PrintList(currentList);
            return currentList;
        }
    }
    // Amount
    else if (user == 3)
    {
        int amountInt;
        while (true)
        {
            PrintList(currentList);
            string amount = ReadString("Enter the amount of the object you want to delete or e to exit").Trim();
            if (amount == "e")
                return currentList;

            bool ok = int.TryParse(amount, out amountInt);
            if (!ok)
            {
                Error();
                continue;
            }
            break;
        }
        while (true)
        {
            string mode = ReadString("What mode do you want to use?\n = -> The amount has to be equal to the entered value to get deleted \n< -> The amount has to be smaller than the entered value to get deleted \n> -> The amount has to be greater than the entered value to get deleted \n e -> Exit to main menu \nWanted mode");
            mode = mode.Trim();
            if (mode == "e")
                return currentList;

            int amountDeleted;
            if (mode == "=")
                amountDeleted = currentList.RemoveAll(t => t.Amount == amountInt);
            else if(mode== "<")
                amountDeleted = currentList.RemoveAll(t=>t.Amount < amountInt);
            else if(mode == ">")
                amountDeleted= currentList.RemoveAll(t=>t.Amount > amountInt);
            else
            {
                Error();
                continue;
            }

            Console.Write("Removed " + amountDeleted + " ");
            if (amountDeleted != 1)
                Console.WriteLine("entries from list.");
            else
                Console.WriteLine("entry from list.");

            Console.WriteLine("List after deletions:");
            PrintList(currentList);
            return currentList;

        }
    }
    // Category
    else if(user==4)
    {
        while (true)
        {
            PrintList(currentList);
            string category = ReadString("Enter the category of the object you want to delete or e to exit").Trim();
            if (category == "e")
                return currentList;

            int amountRemoved = currentList.RemoveAll(t => t.Category == category);

            Console.Write("Removed " + amountRemoved + " ");
            if (amountRemoved != 1)
                Console.WriteLine("entries from list.");
            else
                Console.WriteLine("entry from list.");

            Console.WriteLine("List after deletions:");
            PrintList(currentList);
            return currentList;
        }
    }
    // Description
    else if(user==5)
    {
        while (true)
        {
            PrintList(currentList);
            string description = ReadString("Enter the description of the object you want to delete or e to exit").Trim();
            if (description == "e")
                return currentList;

            int amountRemoved = currentList.RemoveAll(t => t.Description == description);

            Console.Write("Removed " + amountRemoved + " ");
            if (amountRemoved != 1)
                Console.WriteLine("entries from list.");
            else
                Console.WriteLine("entry from list.");

            Console.WriteLine("List after deletions:");
            PrintList(currentList);
            return currentList;
        }
    }
    // Date
    else if(user==6)
    {
        DateOnly dateDate;
        while (true)
        {
            PrintList(currentList);
            string dateStr = ReadString("Enter the date (format: DD.MM.YYYY) of the object you want to delete or e to exit").Trim();
            if (dateStr == "e")
                return currentList;

            bool ok = DateOnly.TryParse(dateStr, out dateDate);
            if (!ok)
            {
                Error();
                continue;
            }
            break;
        }
        while (true)
        {
            string mode = ReadString("What mode do you want to use?\n = -> The date has to be equal to the entered value to get deleted \n< -> The date has to be smaller (=earlier) than the entered value to get deleted \n> -> The date has to be greater (=later) than the entered value to get deleted \n e -> Exit to main menu \nWanted mode");
            mode = mode.Trim();
            if (mode == "e")
                return currentList;

            int amountDeleted;
            if (mode == "=")
                amountDeleted = currentList.RemoveAll(t => t.Date == dateDate);
            else if (mode == "<")
                amountDeleted = currentList.RemoveAll(t => t.Date < dateDate);
            else if (mode == ">")
                amountDeleted = currentList.RemoveAll(t => t.Date > dateDate);
            else
            {
                Error();
                continue;
            }

            Console.Write("Removed " + amountDeleted + " ");
            if (amountDeleted != 1)
                Console.WriteLine("entries from list.");
            else
                Console.WriteLine("entry from list.");

            Console.WriteLine("List after deletions:");
            PrintList(currentList);
            return currentList;

        }
    }

    static void Error(bool writeMessage = true)
    {
        if (writeMessage)
            Console.WriteLine("Invalid Syntax!");

        Timeout();
        Console.Clear();
    }

    return currentList;
}

static void PrintList(List<Transaction> transactions, char cunit = '€')
{
    Console.WriteLine("---------------------------------------------------------------------------------");
    Console.WriteLine($"| {"Name",-15} | {"Amount",-11} | {"Category",-14} | {"Description",-15} | {"Date",-10} |");
    Console.WriteLine("---------------------------------------------------------------------------------");
    for (int i = 0; i < transactions.Count; i++)
    {
        WriteIndexedLineOfHistory(transactions[i], (i + 1), cunit);
    }
    Console.WriteLine("---------------------------------------------------------------------------------");
    Console.WriteLine();
}

// Reading
static int ReadInt(string question)
{
    while (true)
    {
        Console.Write(question + ": ");
        string user = Console.ReadLine()!;
        bool ok = int.TryParse(user, out int value);
        if (ok)
        {
            return value;
        }
        Console.WriteLine("Invalid Input!");
        Timeout();
        Console.Clear();
    }
}
static double ReadDouble(string question)
{
    while (true)
    {
        Console.Write(question + ": ");
        string user = Console.ReadLine()!;
        bool ok = double.TryParse(user, out double value);
        if (ok)
        {
            return value;
        }
        Console.WriteLine("Invalid Input!");
        Timeout();
        Console.Clear();
    }
}

static string ReadString(string question)
{
    Console.Write(question + ": ");
    return Console.ReadLine()!;
}

static DateOnly ReadDate(string question)
{
    while (true)
    {
        Console.Write(question + ": ");
        string user = Console.ReadLine()!;
        bool ok = DateOnly.TryParse(user, out DateOnly value);
        if (ok)
        {
            return value;
        }
        Console.WriteLine("Invalid Input!");
        Timeout();
        Console.Clear();
    }
}