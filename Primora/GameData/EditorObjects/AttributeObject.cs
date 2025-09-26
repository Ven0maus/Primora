using Primora.Core.Items.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Primora.GameData.EditorObjects
{
    public class AttributeObject : IEditorObject
    {
        [JsonIgnore]
        public string Name { get; set; }
        public object DefaultValue { get; set; }
        public AttributeType Type { get; set; }
        public AttributeFor For { get; set; }
        public List<string> Values { get; set; }

        [JsonIgnore, Obsolete("Not available for AttributeObject.", true)]
        public Dictionary<string, object> Attributes 
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }

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
        Enum,
        Color,
        Array
    }
}
