using EditorTool.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EditorTool
{
    public partial class Main : Form
    {
        private readonly Dictionary<string, AttributeObject> _attributes;
        private readonly Dictionary<string, ItemObject> _items;
        private readonly Dictionary<string, NpcObject> _npcs;

        public Main()
        {
            InitializeComponent();

            _attributes = new Dictionary<string, AttributeObject>(StringComparer.OrdinalIgnoreCase);
            _items = new Dictionary<string, ItemObject>(StringComparer.OrdinalIgnoreCase);
            _npcs = new Dictionary<string, NpcObject>(StringComparer.OrdinalIgnoreCase);

            // Disable and hide combobox for certain attribs
            CmbItemAttributeValue.Enabled = false;
            CmbItemAttributeValue.Visible = false;
            CmbNpcAttributeValue.Enabled = false;
            CmbNpcAttributeValue.Visible = false;

            // Init enum values
            foreach (var value in Enum.GetValues<AttributeType>())
                CmbAttributeType.Items.Add(value);
            CmbAttributeType.SelectedItem = AttributeType.Text;
            foreach (var value in Enum.GetValues<AttributeFor>())
                CmbAttributeAvailableFor.Items.Add(value);
            CmbAttributeAvailableFor.SelectedItem = AttributeFor.Shared;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // Find game data folder
            string exePath = AppContext.BaseDirectory;
            var dir = new DirectoryInfo(exePath);

            while (dir != null && dir.GetFiles("*.sln").Length == 0 && dir.GetDirectories("Primora").Length == 0)
            {
                dir = dir.Parent;
            }

            if (dir == null) throw new Exception("Cannot find solution root.");

            // Found the root → build path
            string gameDataPath = Path.Combine(dir.FullName, "Primora", "GameData");

            // Collect attributes
            var attributesPath = Path.Combine(gameDataPath, "Attributes.json");

            // Collect items
            var itemsPath = Path.Combine(gameDataPath, "Items.json");

            // Collect npcs
            var npcsPath = Path.Combine(gameDataPath, "Npcs.json");

            // TODO:
        }

        #region Attributes
        private void CmbAttributeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var enumSelected = CmbAttributeType.SelectedItem is AttributeType s && s == AttributeType.Enum;

            ListBoxAttributeValues.Items.Clear();

            if (!enumSelected)
            {
                BtnCreateNewValue.Enabled = false;
                BtnRemoveSelectedValue.Enabled = false;
            }
            else
            {
                if (_attributes.TryGetValue(TxtAttributeName.Text, out var attribute))
                {
                    if (attribute.Type == AttributeType.Enum)
                    {
                        // Preload existing values
                        foreach (var value in attribute.Values)
                            ListBoxAttributeValues.Items.Add(value);
                    }
                }

                BtnCreateNewValue.Enabled = true;
                BtnRemoveSelectedValue.Enabled = ListBoxAttributeValues.Items.Count > 0;
            }
        }

        private void BtnCreateNewValue_Click(object sender, EventArgs e)
        {
            var value = InputBox.Show("New value:");
            if (string.IsNullOrWhiteSpace(value)) return;
            ListBoxAttributeValues.Items.Add(value);
            BtnRemoveSelectedValue.Enabled = true;
        }

        private void BtnRemoveSelectedValue_Click(object sender, EventArgs e)
        {
            if (ListBoxAttributeValues.SelectedIndex == -1) return;
            ListBoxAttributeValues.Items.RemoveAt(ListBoxAttributeValues.SelectedIndex);
            BtnRemoveSelectedValue.Enabled = ListBoxAttributeValues.Items.Count > 0;
        }

        private void BtnCreateAttribute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtAttributeName.Text))
            {
                MessageBox.Show("Please fill in a valid attribute name.");
                return;
            }

            if (_attributes.ContainsKey(TxtAttributeName.Text.Trim()))
            {
                MessageBox.Show("An attribute with this name already exists.");
                return;
            }

            var attribute = new AttributeObject
            {
                Name = TxtAttributeName.Text,
                Type = (AttributeType)CmbAttributeType.SelectedItem,
                For = (AttributeFor)CmbAttributeAvailableFor.SelectedItem,
                Values = ListBoxAttributeValues.Items.Count > 0 ? [.. ListBoxAttributeValues.Items.Cast<string>()] : null
            };

            _attributes[TxtAttributeName.Text] = attribute;
            ListBoxAttributes.Items.Add(attribute);
            ListBoxAttributes.SelectedItem = attribute;

            if (attribute.For == AttributeFor.Items || attribute.For == AttributeFor.Shared)
                ListBoxItemAttributes.Items.Add(attribute); // Add also for items
            if (attribute.For == AttributeFor.Npcs || attribute.For == AttributeFor.Shared)
                ListBoxNpcAttributes.Items.Add(attribute); // Add also for npcs

            BtnRemoveSelectedAttribute.Enabled = true;

            // Adjust filter to show the "For" that was added if not visible yet
            AdjustForFilter(attribute.For);
            
        }

        private void AdjustForFilter(AttributeFor @for)
        {
            var filter = CmbAttributeFilter.SelectedItem as string;

            switch (@for)
            {
                case AttributeFor.Npcs:
                    if (filter == "Items")
                        CmbAttributeFilter.SelectedItem = "Npcs";
                    break;
                case AttributeFor.Items:
                    if (filter == "Npcs")
                        CmbAttributeFilter.SelectedItem = "Items";
                    break;
            }
        }

        private void BtnRemoveSelectedAttribute_Click(object sender, EventArgs e)
        {
            if (ListBoxAttributes.SelectedItem is AttributeObject attribute)
            {
                _attributes.Remove(attribute.Name);
                if (ListBoxAttributes.SelectedIndex != -1)
                    ListBoxAttributes.SelectedIndex--;
                ListBoxAttributes.Items.Remove(attribute);
                BtnRemoveSelectedAttribute.Enabled = ListBoxAttributes.Items.Count > 0;

                if (attribute.For == AttributeFor.Items || attribute.For == AttributeFor.Shared)
                {
                    // Remove also for items
                    ListBoxItemAttributes.Items.Remove(attribute);
                    foreach (var item in _items.Values)
                        item.Attributes.Remove(attribute.Name);
                }
                if (attribute.For == AttributeFor.Npcs || attribute.For == AttributeFor.Shared)
                {
                    // Remove also for npcs
                    ListBoxNpcAttributes.Items.Remove(attribute);
                    foreach (var npc in _npcs.Values)
                        npc.Attributes.Remove(attribute.Name);
                }
            }
        }

        private void ListBoxAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Fill all values
            if (ListBoxAttributes.SelectedItem is not AttributeObject attributeObject)
            {
                // Empty values
                TxtAttributeName.Text = string.Empty;
                CmbAttributeType.SelectedItem = AttributeType.Text;
                CmbAttributeAvailableFor.SelectedItem = AttributeFor.Shared;
            }
            else
            {
                TxtAttributeName.Text = attributeObject.Name;
                CmbAttributeType.SelectedItem = attributeObject.Type;
                CmbAttributeAvailableFor.SelectedItem = attributeObject.For;
            }
        }
        #endregion

        #region Items
        private void ListBoxItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Trigger selected index change
            ListBoxItemAttributes.SelectedIndex = -1;
            if (ListBoxItemAttributes.Items.Count > 0)
                ListBoxItemAttributes.SelectedIndex = 0;

            // Fill all values
            if (ListBoxItems.SelectedItem is not ItemObject itemObject)
            {
                // Empty values
                TxtItemName.Text = string.Empty;
            }
            else
            {
                TxtItemName.Text = itemObject.Name;
            }
        }

        private void ListBoxItemAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var attribute = ListBoxItemAttributes.SelectedItem as AttributeObject;
            if (attribute != null)
            {
                if (attribute.Type == AttributeType.Enum)
                {
                    CmbItemAttributeValue.Enabled = true;
                    CmbItemAttributeValue.Visible = true;
                    CmbItemAttributeValue.Items.Clear();
                    foreach (var atbValue in attribute.Values)
                        CmbItemAttributeValue.Items.Add(atbValue);
                }
                else
                {
                    CmbItemAttributeValue.Enabled = false;
                    CmbItemAttributeValue.Visible = false;
                    CmbItemAttributeValue.Items.Clear();
                }
            }

            if (attribute == null || ListBoxItems.SelectedItem is not ItemObject itemObject)
            {
                TxtItemAttributeValue.Text = string.Empty;

                if (attribute == null)
                {
                    CmbItemAttributeValue.Enabled = false;
                    CmbItemAttributeValue.Visible = false;
                    CmbItemAttributeValue.Items.Clear();
                }
            }
            else
            {
                if (itemObject.Attributes.TryGetValue(attribute.Name, out var value))
                    TxtItemAttributeValue.Text = value.ToString();
                else
                    TxtItemAttributeValue.Text = string.Empty;
            }
        }

        private void BtnCreateItem_Click(object sender, EventArgs e)
        {
            var name = TxtItemName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a valid item name.");
                return;
            }

            if (_items.ContainsKey(name))
            {
                MessageBox.Show("An item with this name already exists.");
                return;
            }

            var itemObject = new ItemObject
            {
                Name = name,
                Attributes = []
            };

            ListBoxItems.Items.Add(itemObject);
            ListBoxItems.SelectedItem = itemObject;
            CmbNpcItemPicker.Items.Add(itemObject.Name);
            _items[name] = itemObject;
        }

        private void BtnRemoveSelectedItem_Click(object sender, EventArgs e)
        {
            if (ListBoxItems.SelectedItem is ItemObject itemObject)
            {
                if (ListBoxItems.SelectedIndex != -1)
                    ListBoxItems.SelectedIndex--;
                ListBoxItems.Items.Remove(itemObject);
                ListBoxDroppedItems.Items.Remove(itemObject.Name);
                CmbNpcItemPicker.Items.Remove(itemObject.Name);
                _items.Remove(itemObject.Name);

                // Remove from dropped items from all npcs
                foreach (var npc in _npcs.Values)
                    npc.LootTable.Remove(itemObject.Name);
            }
        }

        private void BtnSetItemAttributeValue_Click(object sender, EventArgs e)
        {
            if (ListBoxItems.SelectedItem is not ItemObject itemObject) return;
            if (ListBoxItemAttributes.SelectedItem is not AttributeObject attribute) return;

            if (attribute.Type == AttributeType.Enum)
            {
                if (CmbItemAttributeValue.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a valid value.");
                    return;
                }

                itemObject.Attributes[attribute.Name] = CmbItemAttributeValue.SelectedItem as string;
            }
            else
            {
                var value = TxtItemAttributeValue.Text;
                if (attribute.Type == AttributeType.Char)
                {
                    if (!char.TryParse(value, out var c))
                    {
                        MessageBox.Show("Please set a valid 'char' type value.");
                        return;
                    }
                    itemObject.Attributes[attribute.Name] = c;
                }
                else if (attribute.Type == AttributeType.Number)
                {
                    if (!double.TryParse(value, out var n))
                    {
                        MessageBox.Show("Please set a valid 'number' type value.");
                        return;
                    }
                    itemObject.Attributes[attribute.Name] = n;
                }
                else if (attribute.Type == AttributeType.Boolean)
                {
                    if (!bool.TryParse(value, out var b))
                    {
                        MessageBox.Show("Please set a valid 'boolean' type value.");
                        return;
                    }
                    itemObject.Attributes[attribute.Name] = b;
                }
                else
                {
                    itemObject.Attributes[attribute.Name] = value;
                }
            }
        }
        #endregion

        #region Npcs
        private void BtnCreateNpc_Click(object sender, EventArgs e)
        {
            var name = TxtNpcName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a valid npc name.");
                return;
            }

            if (_npcs.ContainsKey(name))
            {
                MessageBox.Show("An item with this name already exists.");
                return;
            }

            var npcObject = new NpcObject
            {
                Name = name,
                Attributes = [],
                LootTable = []
            };

            ListBoxNpcs.Items.Add(npcObject);
            ListBoxNpcs.SelectedItem = npcObject;
            _npcs[name] = npcObject;
        }

        private void BtnDeleteSelectedNpc_Click(object sender, EventArgs e)
        {
            if (ListBoxNpcs.SelectedItem is NpcObject npcObject)
            {
                if (ListBoxNpcs.SelectedIndex != -1)
                    ListBoxNpcs.SelectedIndex--;
                ListBoxNpcs.Items.Remove(npcObject);
                _npcs.Remove(npcObject.Name);
            }
        }

        private void BtnSetNpcAttributeValue_Click(object sender, EventArgs e)
        {
            if (ListBoxNpcs.SelectedItem is not NpcObject npcObject) return;
            if (ListBoxNpcAttributes.SelectedItem is not AttributeObject attribute) return;

            if (attribute.Type == AttributeType.Enum)
            {
                if (CmbNpcAttributeValue.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a valid value.");
                    return;
                }

                npcObject.Attributes[attribute.Name] = CmbNpcAttributeValue.SelectedItem as string;
            }
            else
            {
                var value = TxtNpcAttributeValue.Text;
                if (attribute.Type == AttributeType.Char)
                {
                    if (!char.TryParse(value, out var c))
                    {
                        MessageBox.Show("Please set a valid 'char' type value.");
                        return;
                    }
                    npcObject.Attributes[attribute.Name] = c;
                }
                else if (attribute.Type == AttributeType.Number)
                {
                    if (!double.TryParse(value, out var n))
                    {
                        MessageBox.Show("Please set a valid 'number' type value.");
                        return;
                    }
                    npcObject.Attributes[attribute.Name] = n;
                }
                else if (attribute.Type == AttributeType.Boolean)
                {
                    if (!bool.TryParse(value, out var b))
                    {
                        MessageBox.Show("Please set a valid 'boolean' type value.");
                        return;
                    }
                    npcObject.Attributes[attribute.Name] = b;
                }
                else
                {
                    npcObject.Attributes[attribute.Name] = value;
                }
            }
        }

        private void BtnAddNpcItem_Click(object sender, EventArgs e)
        {
            if (CmbNpcItemPicker.SelectedItem is string itemName &&
                ListBoxNpcs.SelectedItem is NpcObject npcObject)
            {
                if (npcObject.LootTable.Contains(itemName))
                {
                    MessageBox.Show("This npc already has this item in its drop table.");
                    return;
                }

                ListBoxDroppedItems.Items.Add(itemName);
                _npcs[npcObject.Name].LootTable.Add(itemName);
                CmbNpcItemPicker.SelectedIndex = -1;
            }
        }

        private void BtnRemoveNpcItem_Click(object sender, EventArgs e)
        {
            if (ListBoxDroppedItems.SelectedItem is string itemName &&
                ListBoxNpcs.SelectedItem is NpcObject npcObject)
            {
                ListBoxDroppedItems.Items.Remove(itemName);
                _npcs[npcObject.Name].LootTable.Remove(itemName);
            }
        }

        private void ListBoxNpcs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBoxDroppedItems.Items.Clear();

            // Trigger selected index change
            ListBoxNpcAttributes.SelectedIndex = -1;
            if (ListBoxNpcAttributes.Items.Count > 0)
                ListBoxNpcAttributes.SelectedIndex = 0;

            if (ListBoxNpcs.SelectedItem is NpcObject npc)
            {
                TxtNpcName.Text = npc.Name;
                foreach (var item in npc.LootTable)
                    ListBoxDroppedItems.Items.Add(item);
            }
            else
            {
                TxtNpcName.Text = string.Empty;
            }
        }

        private void ListBoxNpcAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var attribute = ListBoxNpcAttributes.SelectedItem as AttributeObject;
            if (attribute != null)
            {
                if (attribute.Type == AttributeType.Enum)
                {
                    CmbNpcAttributeValue.Enabled = true;
                    CmbNpcAttributeValue.Visible = true;
                    CmbNpcAttributeValue.Items.Clear();
                    foreach (var atbValue in attribute.Values)
                        CmbNpcAttributeValue.Items.Add(atbValue);
                }
                else
                {
                    CmbNpcAttributeValue.Enabled = false;
                    CmbNpcAttributeValue.Visible = false;
                    CmbNpcAttributeValue.Items.Clear();
                }
            }

            if (attribute == null || ListBoxNpcs.SelectedItem is not NpcObject npcObject)
            {
                TxtNpcAttributeValue.Text = string.Empty;

                if (attribute == null)
                {
                    CmbNpcAttributeValue.Enabled = false;
                    CmbNpcAttributeValue.Visible = false;
                    CmbNpcAttributeValue.Items.Clear();
                }
            }
            else
            {
                if (npcObject.Attributes.TryGetValue(attribute.Name, out var value))
                    TxtNpcAttributeValue.Text = value.ToString();
                else
                    TxtNpcAttributeValue.Text = string.Empty;
            }
        }
        #endregion

        private void CmbAttributeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbAttributeFilter.SelectedItem is not string s) return;
            ListBoxAttributes.Items.Clear();

            switch (s)
            {
                case "Show All":
                    // Add all
                    foreach (var attribute in _attributes.Values)
                        ListBoxAttributes.Items.Add(attribute);
                    break;

                case "Shared":
                    // Add only shared
                    foreach (var attribute in _attributes.Values.Where(a => a.For == AttributeFor.Shared))
                        ListBoxAttributes.Items.Add(attribute);
                    break;

                case "Items":
                    // Add only items
                    foreach (var attribute in _attributes.Values.Where(a => a.For == AttributeFor.Items))
                        ListBoxAttributes.Items.Add(attribute);
                    break;

                case "Npcs":
                    // Add only npcs
                    foreach (var attribute in _attributes.Values.Where(a => a.For == AttributeFor.Npcs))
                        ListBoxAttributes.Items.Add(attribute);
                    break;

                default:
                    throw new NotImplementedException($"Not implemented case \"{s}\".");
            }
        }
    }
}
