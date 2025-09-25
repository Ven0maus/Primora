using System.Collections.Generic;

namespace Primora.Core.Items.Interfaces
{
    internal interface IEditorObject
    {
        string Name { get; set; }
        Dictionary<string, object> Attributes { get; set; }
    }
}
