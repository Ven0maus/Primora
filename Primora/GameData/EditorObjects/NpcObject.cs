using Primora.Core.Items.Interfaces;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Primora.GameData.EditorObjects
{
    public class NpcObject : IEditorObject
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
