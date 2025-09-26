using EditorTool.Components;
using Newtonsoft.Json.Linq;
using Primora.Extensions;
using Primora.GameData.EditorObjects;
using SadConsole.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using static SadConsole.Settings;

namespace EditorTool
{
    public partial class Main : Form
    {
        private readonly Dictionary<string, AttributeObject> _attributes;
        private readonly Dictionary<string, ItemObject> _items;
        private readonly Dictionary<string, NpcObject> _npcs;
        private readonly string _gameDataPath;

        private const string _attributesFileName = "Attributes.json";
        private const string _itemsFileName = "Items.json";
        private const string _npcsFileName = "Npcs.json";

        private readonly MultiSelectCombo _mcmbItemAttributeValue, _mcmbNpcAttributeValue, _mcmbDefaultValue;

        public Main()
        {
            InitializeComponent();

            _attributes = new Dictionary<string, AttributeObject>(StringComparer.OrdinalIgnoreCase);
            _items = new Dictionary<string, ItemObject>(StringComparer.OrdinalIgnoreCase);
            _npcs = new Dictionary<string, NpcObject>(StringComparer.OrdinalIgnoreCase);

            var configJson = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText("configuration.json"));
            _gameDataPath = Path.GetFullPath(configJson["GameDataPath"].GetValue<string>());
            if (!Directory.Exists(_gameDataPath))
                throw new Exception("Invalid GameDataPath specified in configuration.exe");

            // Disable and hide combobox for certain attribs
            CmbItemAttributeValue.Enabled = false;
            CmbItemAttributeValue.Visible = false;
            CmbNpcAttributeValue.Enabled = false;
            CmbNpcAttributeValue.Visible = false;
            MCmbItemAttributeValue.Enabled = false;
            MCmbItemAttributeValue.Visible = false;
            _mcmbItemAttributeValue = new MultiSelectCombo(MCmbItemAttributeValue);
            MCmbNpcAttributeValue.Enabled = false;
            MCmbNpcAttributeValue.Visible = false;
            _mcmbNpcAttributeValue = new MultiSelectCombo(MCmbNpcAttributeValue);

            // Default value
            CmbDefaultValue.Enabled = false;
            CmbDefaultValue.Visible = false;
            MCmbDefaultValue.Enabled = false;
            MCmbDefaultValue.Visible = false;
            _mcmbDefaultValue = new MultiSelectCombo(MCmbDefaultValue);

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
            LoadGameData();
        }

        #region Attributes
        private void CmbAttributeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbAttributeType.SelectedItem is not AttributeType s) return;
            var enumOrArraySelected = s == AttributeType.Enum || s == AttributeType.Array;

            ListBoxAttributeValues.Items.Clear();

            if (!enumOrArraySelected)
            {
                BtnCreateNewValue.Enabled = false;
                BtnRemoveSelectedValue.Enabled = false;
            }
            else
            {
                if (_attributes.TryGetValue(TxtAttributeName.Text, out var attribute))
                {
                    if (attribute.Type == AttributeType.Enum || attribute.Type == AttributeType.Array)
                    {
                        // Preload existing values
                        foreach (var value in attribute?.Values ?? [])
                            ListBoxAttributeValues.Items.Add(value);
                    }

                    if (attribute.Type == AttributeType.Enum)
                    {
                        CmbDefaultValue.Visible = true;
                        CmbDefaultValue.Enabled = true;
                        CmbDefaultValue.Items.Clear();
                        foreach (var value in attribute?.Values ?? [])
                            CmbDefaultValue.Items.Add(value);

                        if (attribute.DefaultValue != null)
                            CmbDefaultValue.SelectedItem = (string)attribute.DefaultValue;
                    }
                    else if (attribute.Type == AttributeType.Array)
                    {
                        MCmbDefaultValue.Visible = true;
                        MCmbDefaultValue.Enabled = true;
                        MCmbDefaultValue.Items.Clear();
                        foreach (var value in attribute?.Values ?? [])
                            MCmbDefaultValue.Items.Add(value);
                        _mcmbDefaultValue.ReInit();
                        _mcmbDefaultValue.ResetSelection();

                        if (attribute.DefaultValue != null)
                            _mcmbDefaultValue.Select((string[])attribute.DefaultValue);
                    }
                    else
                    {
                        if (attribute.DefaultValue != null)
                            TxtDefaultValue.Text = (string)attribute.DefaultValue;
                        else
                            TxtDefaultValue.Text = string.Empty;
                    }
                }
                else
                {
                    if (s == AttributeType.Enum)
                    {
                        CmbDefaultValue.Visible = true;
                        CmbDefaultValue.Enabled = true;
                        CmbDefaultValue.Items.Clear();
                        foreach (var value in attribute?.Values ?? [])
                            CmbDefaultValue.Items.Add(value);

                        CmbDefaultValue.SelectedItem = null;
                    }
                    else if (s == AttributeType.Array)
                    {
                        MCmbDefaultValue.Visible = true;
                        MCmbDefaultValue.Enabled = true;
                        MCmbDefaultValue.Items.Clear();
                        foreach (var value in attribute?.Values ?? [])
                            MCmbDefaultValue.Items.Add(value);
                        _mcmbDefaultValue.ReInit();
                        _mcmbDefaultValue.ResetSelection();
                    }
                    else
                    {
                        TxtDefaultValue.Text = string.Empty;
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

            var atrib = TxtAttributeName.Text.Trim();
            if (_attributes.TryGetValue(atrib, out var attribute))
            {
                ListBoxAttributeValues.Items.Add(value);
                attribute.Values ??= [];
                attribute.Values.Add(value);
            }
            else
            {
                ListBoxAttributeValues.Items.Add(value);
            }

            if (CmbAttributeType.SelectedItem is AttributeType s)
            {
                if (s == AttributeType.Enum)
                {
                    CmbDefaultValue.Items.Add(value);

                    if (attribute != null && attribute.DefaultValue != null)
                        CmbDefaultValue.SelectedItem = (string)attribute.DefaultValue;
                }
                else if (s == AttributeType.Array)
                {
                    MCmbDefaultValue.Items.Add(value);
                    _mcmbDefaultValue.ReInit();
                    _mcmbDefaultValue.ResetSelection();

                    if (attribute != null && attribute.DefaultValue != null)
                        _mcmbDefaultValue.Select((string[])attribute.DefaultValue);
                }
            }

            BtnRemoveSelectedValue.Enabled = true;
        }

        private void BtnRemoveSelectedValue_Click(object sender, EventArgs e)
        {
            if (ListBoxAttributeValues.SelectedIndex == -1) return;

            var atrib = TxtAttributeName.Text.Trim();
            var value = (string)ListBoxAttributeValues.SelectedItem;
            if (_attributes.TryGetValue(atrib, out var attribute))
            {
                attribute.Values?.Remove(value);
                ListBoxAttributeValues.Items.RemoveAt(ListBoxAttributeValues.SelectedIndex);
            }
            else
            {
                ListBoxAttributeValues.Items.RemoveAt(ListBoxAttributeValues.SelectedIndex);
            }

            if (CmbAttributeType.SelectedItem is AttributeType s)
            {
                if (s == AttributeType.Enum)
                {
                    CmbDefaultValue.Items.Remove(value);

                    if (attribute != null && attribute.DefaultValue != null && ((string)attribute.DefaultValue) == value)
                        attribute.DefaultValue = null;

                    if (attribute != null && attribute.DefaultValue != null)
                        CmbDefaultValue.SelectedItem = (string)attribute.DefaultValue;
                }
                else if (s == AttributeType.Array)
                {
                    MCmbDefaultValue.Items.Remove(value);
                    _mcmbDefaultValue.ReInit();
                    _mcmbDefaultValue.ResetSelection();

                    if (attribute != null && attribute.DefaultValue != null)
                    {
                        var values = (string[])attribute.DefaultValue;
                        values = [.. values.Where(a => !a.Equals(value))];
                        if (values.Length == 0)
                            values = null;

                        attribute.DefaultValue = values;
                    }

                    if (attribute != null && attribute.DefaultValue != null)
                        _mcmbDefaultValue.Select((string[])attribute.DefaultValue);
                }
            }

            BtnRemoveSelectedValue.Enabled = ListBoxAttributeValues.Items.Count > 0;
        }

        private void BtnCreateAttribute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtAttributeName.Text))
            {
                MessageBox.Show("Please fill in a valid attribute name.");
                return;
            }

            if (_attributes.TryGetValue(TxtAttributeName.Text.Trim(), out var attribute))
            {
                if (MessageBox.Show("An attribute with this name already exists, you will update this attribute, are you sure?",
                    "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Update
                    attribute.Name = TxtAttributeName.Text;
                    attribute.Type = (AttributeType)CmbAttributeType.SelectedItem;
                    attribute.For = (AttributeFor)CmbAttributeAvailableFor.SelectedItem;
                    attribute.Values = ListBoxAttributeValues.Items.Count > 0 ? [.. ListBoxAttributeValues.Items.Cast<string>()] : null;

                    if (attribute.Type == AttributeType.Enum)
                    {
                        attribute.DefaultValue = CmbDefaultValue.SelectedItem == null ? null : (string)CmbDefaultValue.SelectedItem;
                    }
                    else if (attribute.Type == AttributeType.Array)
                    {
                        var values = _mcmbDefaultValue.SelectedItems.Cast<string>().ToArray();
                        attribute.DefaultValue = values.Length == 0 ? null : values;
                    }
                    else
                    {
                        attribute.DefaultValue = string.IsNullOrWhiteSpace(TxtDefaultValue.Text) ? null : TxtDefaultValue.Text;
                    }
                }
                return;
            }

            attribute = new AttributeObject
            {
                Name = TxtAttributeName.Text,
                Type = (AttributeType)CmbAttributeType.SelectedItem,
                For = (AttributeFor)CmbAttributeAvailableFor.SelectedItem,
                Values = ListBoxAttributeValues.Items.Count > 0 ? [.. ListBoxAttributeValues.Items.Cast<string>()] : null,
            };

            if (attribute.Type == AttributeType.Enum)
            {
                attribute.DefaultValue = CmbDefaultValue.SelectedItem == null ? null : (string)CmbDefaultValue.SelectedItem;
            }
            else if (attribute.Type == AttributeType.Array)
            {
                var values = _mcmbDefaultValue.SelectedItems.Cast<string>().ToArray();
                attribute.DefaultValue = values.Length == 0 ? null : values;
            }
            else
            {
                attribute.DefaultValue = string.IsNullOrWhiteSpace(TxtDefaultValue.Text) ? null : TxtDefaultValue.Text;
            }

            _attributes[TxtAttributeName.Text] = attribute;
            ListBoxAttributes.Items.Add(attribute);
            ListBoxAttributes.SelectedIndex = -1;

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
            // This forces event to be raised
            CmbAttributeType.SelectedIndex = -1;

            CmbDefaultValue.Visible = false;
            CmbDefaultValue.Enabled = false;
            MCmbDefaultValue.Visible = false;
            MCmbDefaultValue.Enabled = false;

            // Fill all values
            if (ListBoxAttributes.SelectedItem is not AttributeObject attributeObject)
            {
                // Empty values
                TxtAttributeName.Text = string.Empty;
                CmbAttributeType.SelectedItem = AttributeType.Text;
                CmbAttributeAvailableFor.SelectedItem = AttributeFor.Shared;
                TxtDefaultValue.Text = string.Empty;
                CmbDefaultValue.SelectedIndex = -1;
                CmbDefaultValue.Items.Clear();
                MCmbDefaultValue.Items.Clear();
                _mcmbDefaultValue.ReInit();
                _mcmbDefaultValue.ResetSelection();
            }
            else
            {
                TxtAttributeName.Text = attributeObject.Name;
                CmbAttributeType.SelectedItem = attributeObject.Type;
                CmbAttributeAvailableFor.SelectedItem = attributeObject.For;

                if (attributeObject.Type == AttributeType.Enum)
                {
                    CmbDefaultValue.Visible = true;
                    CmbDefaultValue.Enabled = true;
                    CmbDefaultValue.Items.Clear();
                    foreach (var value in attributeObject?.Values ?? [])
                        CmbDefaultValue.Items.Add(value);

                    if (attributeObject.DefaultValue != null)
                        CmbDefaultValue.SelectedItem = (string)attributeObject.DefaultValue;
                }
                else if (attributeObject.Type == AttributeType.Array)
                {
                    MCmbDefaultValue.Visible = true;
                    MCmbDefaultValue.Enabled = true;
                    MCmbDefaultValue.Items.Clear();
                    foreach (var value in attributeObject?.Values ?? [])
                        MCmbDefaultValue.Items.Add(value);
                    _mcmbDefaultValue.ReInit();

                    if (attributeObject.DefaultValue != null)
                        _mcmbDefaultValue.Select((string[])attributeObject.DefaultValue);
                }
                else
                {
                    if (attributeObject.DefaultValue != null)
                        TxtDefaultValue.Text = (string)attributeObject.DefaultValue;
                    else
                        TxtDefaultValue.Text = string.Empty;
                }
            }
        }

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

        private void ListBoxAttributes_DoubleClick(object sender, EventArgs e)
        {
            if (ListBoxAttributes.SelectedItem is AttributeObject ao && ao != null)
            {
                var name = InputBox.Show("Rename attribute:")?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Invalid name, must not be empty.");
                    return;
                }

                if (_attributes.ContainsKey(name))
                {
                    MessageBox.Show($"An attribute with the name \"{name}\" already exists.");
                    return;
                }

                _attributes[name] = ao;
                _attributes.Remove(ao.Name);

                var oldName = ao.Name;

                // Rename attribute for items and npcs
                if (ao.For == AttributeFor.Shared || ao.For == AttributeFor.Items)
                {
                    foreach (var item in _items)
                    {
                        if (item.Value.Attributes.TryGetValue(oldName, out var iv))
                        {
                            item.Value.Attributes.Remove(oldName);
                            item.Value.Attributes[name] = iv;
                        }
                    }
                }
                if (ao.For == AttributeFor.Shared || ao.For == AttributeFor.Npcs)
                {
                    foreach (var npc in _npcs)
                    {
                        if (npc.Value.Attributes.TryGetValue(oldName, out var iv))
                        {
                            npc.Value.Attributes.Remove(oldName);
                            npc.Value.Attributes[name] = iv;
                        }
                    }
                }

                void ResetListBoxes(System.Windows.Forms.ListBox listBox, string oldName)
                {
                    int index = -1;
                    bool found = false;
                    foreach (var item in listBox.Items)
                    {
                        if (((AttributeObject)item).Name == oldName)
                        {
                            index++;
                            found = true;
                            ao.Name = name;
                            break;
                        }
                        index++;
                    }

                    if (found)
                    {
                        listBox.Items.RemoveAt(index);
                        listBox.Items.Insert(index, ao);
                    }
                }

                ResetListBoxes(ListBoxAttributes, oldName);
                ResetListBoxes(ListBoxItemAttributes, oldName);
                ResetListBoxes(ListBoxNpcAttributes, oldName);
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
                // Hide all fields first
                // Hide regular item combo
                CmbItemAttributeValue.Enabled = false;
                CmbItemAttributeValue.Visible = false;
                CmbItemAttributeValue.Items.Clear();
                CmbItemAttributeValue.SelectedIndex = -1;
                CmbItemAttributeValue.Text = string.Empty;

                // Item attributes multi picker
                MCmbItemAttributeValue.Enabled = false;
                MCmbItemAttributeValue.Visible = false;
                _mcmbItemAttributeValue.ReInit();
                _mcmbItemAttributeValue.ResetSelection();

                if (attribute.Type == AttributeType.Enum)
                {
                    CmbItemAttributeValue.Enabled = true;
                    CmbItemAttributeValue.Visible = true;
                    CmbItemAttributeValue.Items.Clear();
                    foreach (var atbValue in attribute.Values)
                        CmbItemAttributeValue.Items.Add(atbValue);

                    if (ListBoxItems.SelectedItem is ItemObject itemO &&
                        itemO.Attributes.TryGetValue(attribute.Name, out var dataValue))
                    {
                        CmbItemAttributeValue.SelectedItem = dataValue.ToString();
                    }
                    else
                    {
                        CmbItemAttributeValue.SelectedIndex = -1;
                        CmbItemAttributeValue.Text = string.Empty;
                    }
                }
                else if (attribute.Type == AttributeType.Array)
                {
                    MCmbItemAttributeValue.Enabled = true;
                    MCmbItemAttributeValue.Visible = true;
                    MCmbItemAttributeValue.Items.Clear();
                    foreach (var atbValue in attribute.Values)
                        MCmbItemAttributeValue.Items.Add(atbValue);
                    _mcmbItemAttributeValue.ReInit();

                    if (ListBoxItems.SelectedItem is ItemObject itemO &&
                        itemO.Attributes.TryGetValue(attribute.Name, out var dataValue))
                    {
                        string[] data = [];
                        if (dataValue is JsonElement je)
                            data = [.. ((JsonElement)dataValue).EnumerateArray().Select(a => a.GetString())];
                        else if (dataValue is string[] sA)
                            data = sA;

                        if (data.Length == 0)
                            _mcmbItemAttributeValue.ResetSelection();
                        else
                            _mcmbItemAttributeValue.Select(data);
                    }
                    else
                    {
                        _mcmbItemAttributeValue.ResetSelection();
                    }
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
                    CmbItemAttributeValue.SelectedIndex = -1;
                    CmbItemAttributeValue.Text = string.Empty;

                    // Item attributes multi picker
                    MCmbItemAttributeValue.Enabled = false;
                    MCmbItemAttributeValue.Visible = false;
                    _mcmbItemAttributeValue.ReInit();
                    _mcmbItemAttributeValue.ResetSelection();
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

            // Init defaults
            foreach (var attribute in _attributes)
            {
                if (attribute.Value.For != AttributeFor.Shared || attribute.Value.For != AttributeFor.Items)
                    continue;

                if (attribute.Value.DefaultValue != null)
                {
                    itemObject.Attributes[attribute.Key] = attribute.Value.DefaultValue;
                }
            }

            ListBoxItems.Items.Add(itemObject);
            ListBoxItems.SelectedItem = itemObject;
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
                    itemObject.Attributes.Remove(attribute.Name);
                    return;
                }

                itemObject.Attributes[attribute.Name] = CmbItemAttributeValue.SelectedItem as string;
            }
            else if (attribute.Type == AttributeType.Array)
            {
                var values = _mcmbItemAttributeValue.SelectedItems.Cast<string>().ToArray();
                if (values.Length == 0)
                    itemObject.Attributes.Remove(attribute.Name);
                else
                    itemObject.Attributes[attribute.Name] = values;
            }
            else if (attribute.Type == AttributeType.Color)
            {
                if (string.IsNullOrWhiteSpace(TxtItemAttributeValue.Text) || !TxtItemAttributeValue.Text.IsValidHexColor())
                {
                    if (string.IsNullOrWhiteSpace(TxtItemAttributeValue.Text) && itemObject.Attributes.ContainsKey(attribute.Name))
                    {
                        itemObject.Attributes.Remove(attribute.Name);
                        return;
                    }

                    using var colorDialog = new ColorDialog();
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        TxtItemAttributeValue.Text = colorDialog.Color.ToHex();
                        itemObject.Attributes[attribute.Name] = TxtItemAttributeValue.Text;
                    }
                }
            }
            else
            {
                var value = TxtItemAttributeValue.Text.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    itemObject.Attributes.Remove(attribute.Name);
                    return;
                }

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

        private void ListBoxItems_DoubleClick(object sender, EventArgs e)
        {
            if (ListBoxItems.SelectedItem is ItemObject itemObject && itemObject != null)
            {
                var name = InputBox.Show("Rename item:")?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Invalid name, must not be empty.");
                    return;
                }

                if (_items.ContainsKey(name))
                {
                    MessageBox.Show($"An item with the name \"{name}\" already exists.");
                    return;
                }

                _items[name] = itemObject;
                _items.Remove(itemObject.Name);
                var i = ListBoxItems.Items.IndexOf(itemObject);
                ListBoxItems.Items.RemoveAt(i);
                itemObject.Name = name;
                ListBoxItems.Items.Insert(i, itemObject);
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

            // Init defaults
            foreach (var attribute in _attributes)
            {
                if (attribute.Value.For != AttributeFor.Shared || attribute.Value.For != AttributeFor.Npcs)
                    continue;

                if (attribute.Value.DefaultValue != null)
                {
                    npcObject.Attributes[attribute.Key] = attribute.Value.DefaultValue;
                }
            }

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
                    npcObject.Attributes.Remove(attribute.Name);
                }
                else
                {
                    npcObject.Attributes[attribute.Name] = (string)CmbNpcAttributeValue.SelectedItem;
                }
            }
            else if (attribute.Type == AttributeType.Array)
            {
                var values = _mcmbNpcAttributeValue.SelectedItems.Cast<string>().ToArray();
                if (values.Length == 0)
                    npcObject.Attributes.Remove(attribute.Name);
                else
                    npcObject.Attributes[attribute.Name] = values;
            }
            else if (attribute.Type == AttributeType.Color)
            {
                if (string.IsNullOrWhiteSpace(TxtNpcAttributeValue.Text) || !TxtNpcAttributeValue.Text.IsValidHexColor())
                {
                    if (string.IsNullOrWhiteSpace(TxtNpcAttributeValue.Text) && npcObject.Attributes.ContainsKey(attribute.Name))
                    {
                        npcObject.Attributes.Remove(attribute.Name);
                        return;
                    }

                    using var colorDialog = new ColorDialog();
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        TxtNpcAttributeValue.Text = colorDialog.Color.ToHex();
                        npcObject.Attributes[attribute.Name] = TxtNpcAttributeValue.Text;
                    }
                }
            }
            else
            {
                var value = TxtNpcAttributeValue.Text.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    npcObject.Attributes.Remove(attribute.Name);
                    return;
                }

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
            if (CmbNpcItemPicker.SelectedItem is ItemObject itemObject &&
                ListBoxNpcs.SelectedItem is NpcObject npcObject)
            {
                if (npcObject.LootTable.Contains(itemObject.Name))
                {
                    MessageBox.Show("This npc already has this item in its drop table.");
                    return;
                }

                ListBoxDroppedItems.Items.Add(itemObject.Name);
                _npcs[npcObject.Name].LootTable.Add(itemObject.Name);
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
                // Hide all fields first
                CmbNpcAttributeValue.Enabled = false;
                CmbNpcAttributeValue.Visible = false;
                CmbNpcAttributeValue.Items.Clear();
                CmbNpcAttributeValue.SelectedIndex = -1;
                CmbNpcAttributeValue.Text = string.Empty;

                // Npc attributes multi picker
                MCmbNpcAttributeValue.Enabled = false;
                MCmbNpcAttributeValue.Visible = false;
                _mcmbNpcAttributeValue.ResetSelection();

                if (attribute.Type == AttributeType.Enum)
                {
                    CmbNpcAttributeValue.Enabled = true;
                    CmbNpcAttributeValue.Visible = true;
                    CmbNpcAttributeValue.Items.Clear();
                    foreach (var atbValue in attribute.Values)
                        CmbNpcAttributeValue.Items.Add(atbValue);

                    if (ListBoxNpcs.SelectedItem is NpcObject npcO &&
                        npcO.Attributes.TryGetValue(attribute.Name, out var dataValue))
                    {
                        CmbNpcAttributeValue.SelectedItem = dataValue.ToString();
                    }
                    else
                    {
                        CmbNpcAttributeValue.SelectedIndex = -1;
                        CmbNpcAttributeValue.Text = string.Empty;
                    }

                }
                else if (attribute.Type == AttributeType.Array)
                {
                    MCmbNpcAttributeValue.Enabled = true;
                    MCmbNpcAttributeValue.Visible = true;
                    MCmbNpcAttributeValue.Items.Clear();
                    foreach (var atbValue in attribute.Values)
                        MCmbNpcAttributeValue.Items.Add(atbValue);
                    _mcmbNpcAttributeValue.ReInit();

                    if (ListBoxNpcs.SelectedItem is NpcObject npcO &&
                        npcO.Attributes.TryGetValue(attribute.Name, out var dataValue))
                    {
                        string[] data = [];
                        if (dataValue is JsonElement je)
                            data = [.. ((JsonElement)dataValue).EnumerateArray().Select(a => a.GetString())];
                        else if (dataValue is string[] sA)
                            data = sA;

                        if (data.Length == 0)
                            _mcmbNpcAttributeValue.ResetSelection();
                        else
                            _mcmbNpcAttributeValue.Select(data);
                    }
                    else
                    {
                        _mcmbNpcAttributeValue.ResetSelection();
                    }
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
                    CmbNpcAttributeValue.SelectedIndex = -1;
                    CmbNpcAttributeValue.Text = string.Empty;

                    // Npc attributes multi picker
                    MCmbNpcAttributeValue.Enabled = false;
                    MCmbNpcAttributeValue.Visible = false;
                    _mcmbNpcAttributeValue.ResetSelection();
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

        private void CmbNpcItemPicker_TextChanged(object sender, EventArgs e)
        {
            string text = CmbNpcItemPicker.Text;

            // If text already matches an item, skip filtering
            if (_items.ContainsKey(text))
                return;

            // Filter
            ItemObject[] filtered;
            if (string.IsNullOrEmpty(text))
            {
                filtered = [.. _items.Values];
            }
            else
            {
                filtered = [.. _items
                    .Where(item => item.Key.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
                    .Select(a => a.Value)];
            }

            // Save cursor position
            int selectionStart = CmbNpcItemPicker.SelectionStart;

            // Update dropdown
            CmbNpcItemPicker.BeginUpdate();
            CmbNpcItemPicker.Items.Clear();
            CmbNpcItemPicker.Items.AddRange(filtered);
            CmbNpcItemPicker.EndUpdate();

            // Restore typed text
            CmbNpcItemPicker.SelectionStart = selectionStart;
            CmbNpcItemPicker.SelectionLength = 0;

            CmbNpcItemPicker.DroppedDown = true;
            Cursor.Current = Cursors.Default; // fixes flicker
        }

        private void CmbNpcItemPicker_DropDown(object sender, EventArgs e)
        {
            // If nothing typed, always show the full list
            if (string.IsNullOrEmpty(CmbNpcItemPicker.Text))
            {
                CmbNpcItemPicker.BeginUpdate();
                CmbNpcItemPicker.Items.Clear();
                CmbNpcItemPicker.Items.AddRange([.. _items.Values]);
                CmbNpcItemPicker.EndUpdate();
            }
        }

        private void ListBoxNpcs_DoubleClick(object sender, EventArgs e)
        {
            if (ListBoxNpcs.SelectedItem is NpcObject npcObject && npcObject != null)
            {
                var name = InputBox.Show("Rename npc:")?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Invalid name, must not be empty.");
                    return;
                }

                if (_npcs.ContainsKey(name))
                {
                    MessageBox.Show($"An npc with the name \"{name}\" already exists.");
                    return;
                }

                _npcs[name] = npcObject;
                _npcs.Remove(npcObject.Name);
                var i = ListBoxNpcs.Items.IndexOf(npcObject);
                ListBoxNpcs.Items.RemoveAt(i);
                npcObject.Name = name;
                ListBoxNpcs.Items.Insert(i, npcObject);
            }
        }
        #endregion

        #region Saving and loading
        private void BtnSaveConfiguration_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("This will update all game data in Primora main directory, are you sure?!",
                "Are you sure?", MessageBoxButtons.YesNo);
            if (result == DialogResult.No) return;

            // Serialize all game data files
            try
            {
                File.WriteAllText(Path.Combine(_gameDataPath, _attributesFileName), JsonSerializer.Serialize(_attributes, _serializerOptions));
                File.WriteAllText(Path.Combine(_gameDataPath, _itemsFileName), JsonSerializer.Serialize(_items, _serializerOptions));
                File.WriteAllText(Path.Combine(_gameDataPath, _npcsFileName), JsonSerializer.Serialize(_npcs, _serializerOptions));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to serialize gamedata: " + ex.Message);
            }
        }

        private void LoadGameData()
        {
            // Found the root → build path
            LoadAttributes(_gameDataPath);
            LoadItems(_gameDataPath);
            LoadNpcs(_gameDataPath);

            // Visualize all loaded data
            foreach (var attribute in _attributes.Values)
            {
                ListBoxAttributes.Items.Add(attribute);
                if (attribute.For == AttributeFor.Items || attribute.For == AttributeFor.Shared)
                    ListBoxItemAttributes.Items.Add(attribute);
                if (attribute.For == AttributeFor.Npcs || attribute.For == AttributeFor.Shared)
                    ListBoxNpcAttributes.Items.Add(attribute);
            }
            foreach (var item in _items.Values)
            {
                ListBoxItems.Items.Add(item);
            }
            foreach (var npc in _npcs.Values)
            {
                ListBoxNpcs.Items.Add(npc);
            }
        }

        private void LoadAttributes(string gameDataPath)
        {
            try
            {
                var attributesPath = Path.Combine(gameDataPath, _attributesFileName);
                if (File.Exists(attributesPath))
                {
                    var content = Read<AttributeObject>(attributesPath);
                    foreach (var value in content)
                    {
                        value.Value.Name = value.Key;
                        _attributes[value.Key] = value.Value;
                    }
                }
                else
                {
                    Debug.WriteLine("File not found: " + attributesPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to load attributes json: " + ex.Message);
            }
        }

        private void LoadItems(string gameDataPath)
        {
            try
            {
                var itemsPath = Path.Combine(gameDataPath, _itemsFileName);
                if (File.Exists(itemsPath))
                {
                    var content = Read<ItemObject>(itemsPath);
                    foreach (var value in content)
                    {
                        value.Value.Name = value.Key;
                        _items[value.Key] = value.Value;
                    }
                }
                else
                {
                    Debug.WriteLine("File not found: " + itemsPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to load items json: " + ex.Message);
            }
        }

        private void LoadNpcs(string gameDataPath)
        {
            try
            {
                var npcsPath = Path.Combine(gameDataPath, _npcsFileName);
                if (File.Exists(npcsPath))
                {
                    var content = Read<NpcObject>(npcsPath);
                    foreach (var value in content)
                    {
                        value.Value.Name = value.Key;
                        _npcs[value.Key] = value.Value;
                    }
                }
                else
                {
                    Debug.WriteLine("File not found: " + npcsPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to load npcs json: " + ex.Message);
            }
        }

        private static Dictionary<string, T> Read<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, T>>(json, _serializerOptions);
        }

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true
        };
        #endregion
    }
}
