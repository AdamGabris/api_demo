const string CONFIG_FILE = ".config";
string pantry_id = "";

if (File.Exists(CONFIG_FILE))
{
    string[] configDetails = File.ReadAllLines(CONFIG_FILE);
    pantry_id = configDetails[0];
}
else
{
    Console.WriteLine("Input your PantryID: ");
    pantry_id = Console.ReadLine() ?? "";
    if (pantry_id.Length > 0)
    {
        File.WriteAllText(CONFIG_FILE, pantry_id);
    }
}