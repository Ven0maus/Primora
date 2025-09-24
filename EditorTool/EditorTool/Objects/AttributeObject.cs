using System.Collections.Generic;

namespace EditorTool.Objects
{
    public class AttributeObject
    {
        public string Name { get; set; }
        public AttributeType Type { get; set; }
        public AttributeFor For { get; set; }
        public List<string> Values { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum AttributeFor
    {
        Shared,
        Items,
        Npcs
    }

    public enum AttributeType
    {
        Text,
        Char,
        Number,
        Boolean,
        Enum
    }
}
