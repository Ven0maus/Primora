using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EditorTool.Objects
{
    public class ItemObject
    {
        [JsonIgnore]
        public string Name { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
