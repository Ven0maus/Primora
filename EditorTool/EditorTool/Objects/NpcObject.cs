using System.Collections.Generic;

namespace EditorTool.Objects
{
    public class NpcObject
    {
        public string Name { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public List<string> LootTable { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
