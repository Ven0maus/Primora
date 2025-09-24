using System.Collections.Generic;

namespace EditorTool.Objects
{
    public class ItemObject
    {
        public string Name { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
