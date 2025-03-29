using System.Text.Json;
using Exam.Class;

namespace Exam.Servies;

public class ConfigServies
{
    private Config _config;
    public ConfigServies()
    {
        _config = new Config();
    }

    public string GetApi()
    {
        using (var fs = new FileStream("..\\..\\..\\api.json", FileMode.Open, FileAccess.Read))
        {
            using (var sr = new StreamReader(fs))
            {
                var json = sr.ReadToEnd();
                Config newConfig = JsonSerializer.Deserialize<Config>(json);
                return newConfig.BOT_API;
            }
        }
    }
    public void SetAPI(string api)
    {
        _config.BOT_API = api;
        using (var fs = new FileStream("..\\..\\..\\api.json", FileMode.OpenOrCreate, FileAccess.Write))
        {
            using (var sw = new StreamWriter(fs))
            {
                string json = JsonSerializer.Serialize(_config);
                sw.WriteLine(json);
            }
        }
    }
}
