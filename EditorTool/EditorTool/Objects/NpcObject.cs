using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EditorTool.Objects
{
    public class NpcObject
    {
        [JsonIgnore]
        public string Name { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public List<string> LootTable { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
