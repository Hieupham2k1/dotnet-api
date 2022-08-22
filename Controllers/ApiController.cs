using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Text.Encodings.Web.JavaScriptEncoder;

namespace mvc_dotnet.Controllers;

public class ApiController : Controller
{
    HttpClient client;

    public ApiController() {
        this.client = new HttpClient();
    }

    public async Task<string> Lotto(int year, int month, int date)
    {
        if(date != 0) {
            return await this.getStatisticOfDate(date, month, year);
        }
        return await this.getStatisticOfMonth(month, year);
    }
    
    async Task<string> getStatisticOfDate(int date, int month, int year) {
        Dictionary<String, Dictionary<string, int>> statistic = new Dictionary<String, Dictionary<string, int>>();
        statistic["de"] = new Dictionary<string, int>();
        statistic["lo"] = new Dictionary<string, int>();

        JsonSerializerOptions jso = new JsonSerializerOptions();
        jso.Encoder = UnsafeRelaxedJsonEscaping;

        string table = between(JsonSerializer.Serialize(await fetch($"https://xosoketqua.com/xsmb-{ date }-{ month }-{ year }.html"), jso), "table table-bordered table-striped table-xsmb", "</table>");
        string de = between(between(table, "special-prize-lg div-horizontal", "</span>"), ">", "a");
        de = de.Substring(de.Length - 2, 2);
        if(!int.TryParse(de, out _)) return JsonSerializer.Serialize(statistic, jso);
        statistic["de"][de] = 1;

        string[] loArray = table.Split("</span>");
        foreach(string lo in loArray) {
            string temp = lo.Substring(lo.Length - 2, 2);
            if(int.TryParse(temp, out _)) {
                if(!statistic["lo"].ContainsKey(temp)) statistic["lo"][temp] = 1;
                else statistic["lo"][temp] = statistic["lo"][temp] + 1;
            }
        }
        return JsonSerializer.Serialize(statistic, jso);
    }

    async Task<string> getStatisticOfMonth(int month, int year) {
        int[] maxDateOfMonth = new int[] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        Dictionary<String, Dictionary<string, int>> statistic = new Dictionary<String, Dictionary<string, int>>();
        statistic["de"] = new Dictionary<string, int>();
        statistic["lo"] = new Dictionary<string, int>();

        JsonSerializerOptions jso = new JsonSerializerOptions();
        jso.Encoder = UnsafeRelaxedJsonEscaping;

        List<Task> TaskList = new List<Task>();
        for(int date = 1; date <= maxDateOfMonth[month]; date++) {
            int scopedDate = date;
            TaskList.Add( 
                Task.Run(async () => {
                    string table = between(JsonSerializer.Serialize(await fetch($"https://xosoketqua.com/xsmb-{ scopedDate }-{ month }-{ year }.html"), jso), "table table-bordered table-striped table-xsmb", "</table>");
                    string de = between(between(table, "special-prize-lg div-horizontal", "</span>"), ">", "a");
                    de = de.Substring(de.Length - 2, 2);
                    if(!int.TryParse(de, out int _)) {
                        return;
                    }

                    if(!statistic["de"].ContainsKey(de)) statistic["de"][de] = 1;
                    else statistic["de"][de] = statistic["de"][de] + 1;

                    string[] loArray = table.Split("</span>");
                    foreach(string lo in loArray) {
                        string temp = lo.Substring(lo.Length - 2, 2);
                        if(int.TryParse(temp, out _)) {
                            if(!statistic["lo"].ContainsKey(temp)) statistic["lo"][temp] = 1;
                            else statistic["lo"][temp] = statistic["lo"][temp] + 1;
                        }
                    }
                })
            );
        }
        await Task.WhenAll(TaskList.ToArray());
        return JsonSerializer.Serialize(statistic, jso);
    }

    string between(string str, string firstString, string lastString)
    {    
        int pos1 = str.IndexOf(firstString) + firstString.Length;
        if(pos1 > str.Length) return "";
        if(str.IndexOf(firstString) == -1) pos1 = 0;
        int pos2 = str.Substring(pos1).IndexOf(lastString);
        if(lastString == "" || pos2 == -1) pos2 = str.Length - pos1;
        return str.Substring(pos1, pos2);
    }

    async Task<string> fetch(string baseUrl) {
            //Have your using statements within a try/catch block
            try
            {
                //In the next using statement you will initiate the Get Request, use the await keyword so it will execute the using statement in order.
                using (HttpResponseMessage res = await this.client.GetAsync(baseUrl))
                {
                    //Then get the content from the response in the next using statement, then within it you will get the data, and convert it to a c# object.
                    using (HttpContent content = res.Content)
                    {
                        //Now assign your content to your data variable, by converting into a string using the await keyword.
                        var data = await content.ReadAsStringAsync();
                        //If the data isn't null return log convert the data using newtonsoft JObject Parse class method on the data.
                        if (data != null)
                        {
                            return data;
                        }
                        else 
                        {
                            Console.WriteLine("NO Data----------");
                            return "";
                        }
                    }
                }
            } catch(Exception exception)
            {
                Console.WriteLine("Exception Hit------------");
                Console.WriteLine(exception);
                return "";
            }
    }
}