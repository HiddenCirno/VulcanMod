using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace VulcanMod
{
    public class ConfigManager
    {
        public static string modPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string dataPath = "moddata/";
        public static string configJsoncContent = File.ReadAllText(System.IO.Path.Combine(modPath, "config.jsonc"));
        public static VulcanModConfigClass GetConfig()
        {
            return JsonSerializer.Deserialize<VulcanModConfigClass>(configJsoncContent, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip // ∆Ù”√◊¢ ÕΩ‚Œˆ
            });
        }
    }
    public class VulcanModConfigClass
    {
        [JsonPropertyName("EnableReshalaEdit")]
        public bool EnableReshalaEdit { get; set; }

        [JsonPropertyName("EnableMCHead")]
        public bool EnableMCHead { get; set; }

        [JsonPropertyName("MCHeadData")]
        public List<MCHeadDataClass> MCHeadData { get; set; }

        [JsonPropertyName("EnableAlterBoss")]
        public bool EnableAlterBoss { get; set; }

        [JsonPropertyName("AlterBossChance")]
        public AlterBossChanceClass AlterBossChance { get; set; }

        [JsonPropertyName("EnableKabanInShoreline")]
        public bool EnableKabanInShoreline { get; set; }

        [JsonPropertyName("KabanInShorelineChance")]
        public int KabanInShorelineChance { get; set; }

        [JsonPropertyName("EnableBlackDivision")]
        public bool EnableBlackDivision { get; set; }

        [JsonPropertyName("BlackDivisionChance")]
        public int BlackDivisionChance { get; set; }

        [JsonPropertyName("BlackDivisionMapConfig")]
        public Dictionary<string, bool> BlackDivisionMapConfig { get; set; }

        [JsonPropertyName("EnableRecipeEdit")]
        public bool EnableRecipeEdit { get; set; }
    }

    public class MCHeadDataClass
    {
        [JsonPropertyName("Bot")]
        public List<string> Bot { get; set; }

        [JsonPropertyName("Data")]
        public List<List<string>> Data { get; set; }
    }

    public class AlterBossChanceClass
    {
        [JsonPropertyName("Goons")]
        public int Goons { get; set; }

        [JsonPropertyName("Sanitar")]
        public int Sanitar { get; set; }

        [JsonPropertyName("Gluhar")]
        public int Gluhar { get; set; }

        [JsonPropertyName("Kolontay")]
        public int Kolontay { get; set; }
    }
}