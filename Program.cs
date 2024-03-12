using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Content = System.Collections.Generic.Dictionary<string, string>;

const string CONFIG_FILE = ".config";
const string DEFAULT_BASKET = "save";
const string KEY_PARAMETER = "-n";
const string DEFAULT_KEY = "note_";
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

HttpClient httpClient = InitializeHttpClient();

string baseURL = $"https://getpantry.cloud/apiv1/pantry/{pantry_id}/";
string basketURL = $"{baseURL}basket/save";

HttpResponseMessage response = await httpClient.GetAsync(baseURL);
Content? allNotes = JsonSerializer.Deserialize<Content>(await response.Content.ReadAsStringAsync());

if (args.Length > 0)
{
    string key = "";
    int startIndex = 0;

    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == KEY_PARAMETER)
        {
            key = args[i + 1];
            startIndex = i + 2;
            break;
        }
    }

    string content = String.Join("", args[startIndex..args.Length]);

    if (key == "")
    {
        int index = 0;
        do
        {
            key = $"{DEFAULT_KEY}{index}";
            index++;

        } while (allNotes != null && allNotes.ContainsKey(key));
    }
    response = await httpClient.PutAsJsonAsync(basketURL, new Content() { { key, content } });

    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
        Console.WriteLine("LETSGO");
    }
    else
    {
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}


if (response.StatusCode == System.Net.HttpStatusCode.OK)
{
    string pantryRawContent = (await response.Content.ReadAsStringAsync()) ?? "";
    Pantry? pantry = JsonSerializer.Deserialize<Pantry>(pantryRawContent);


    if (pantry != null)
    {
        BasketListing saveBasket = null;
        if (pantry?.baskets != null)
        {
            foreach (BasketListing basket in pantry.baskets)
            {
                if (basket.name == DEFAULT_BASKET)
                {
                    saveBasket = basket;
                }
            }
        }

        if (saveBasket == null)
        {
            response = await httpClient.PostAsJsonAsync(basketURL, new Content() { });
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
    else
    {
        Console.WriteLine("Error: Could not get pantry");
    }
}

else
{
    Console.WriteLine(response.StatusCode);
    Console.WriteLine();
}



HttpClient InitializeHttpClient()
{
    HttpClient httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return httpClient;
}

class Pantry
{
    public string? name { get; set; }
    public string? description { get; set; }
    public string[]? errors { get; set; }
    public bool? notifications { get; set; }
    public double? percentFull { get; set; }
    public BasketListing[]? baskets { get; set; }
}

class BasketListing
{
    public string? name { get; set; }
    public int? ttl { get; set; }
}