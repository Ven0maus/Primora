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
        private readonly Dictionary<string, List<ItemObject>> _items;
        private readonly Dictionary<string, List<NpcObject>> _npcs;

        public Main()
        {
            InitializeComponent();

            _attributes = new Dictionary<string, AttributeObject>(StringComparer.OrdinalIgnoreCase);
            _items = new Dictionary<string, List<ItemObject>>(StringComparer.OrdinalIgnoreCase);
            _npcs = new Dictionary<string, List<NpcObject>>(StringComparer.OrdinalIgnoreCase);

            // Disable and hide combobox for certain attribs
            CmbItemAttributeValue.Enabled = false;
            CmbItemAttributeValue.Visible = false;
            CmbNpcAttributeValue.Enabled = false;
            CmbNpcAttributeValue.Visible = false;
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
            var itemsPath = Path.Combine(gameDataPath, "Items");

            // Collect npcs
            var npcsPath = Path.Combine(gameDataPath, "Npcs");


        }

        #region Attributes
        private void CmbAttributeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var enumSelected = CmbAttributeType.SelectedItem is string s && s.Equals("enum", StringComparison.OrdinalIgnoreCase);

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
                Type = Enum.Parse<AttributeType>(CmbAttributeType.SelectedItem as string, true),
                For = Enum.Parse<AttributeFor>(CmbAttributeAvailableFor.SelectedItem as string, true),
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
        }

        private void BtnRemoveSelectedAttribute_Click(object sender, EventArgs e)
        {
            var attribute = ListBoxAttributes.SelectedItem as AttributeObject;
            if (attribute != null)
            {
                _attributes.Remove(attribute.Name);
                ListBoxAttributes.Items.Remove(attribute);
                if (attribute.For == AttributeFor.Items || attribute.For == AttributeFor.Shared)
                    ListBoxItemAttributes.Items.Remove(attribute); // Remove also for items
                if (attribute.For == AttributeFor.Npcs || attribute.For == AttributeFor.Shared)
                    ListBoxNpcAttributes.Items.Remove(attribute); // Remove also for npcs
                BtnRemoveSelectedAttribute.Enabled = ListBoxAttributes.Items.Count > 0;
            }
        }

        private void ListBoxAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Fill all values
            var attributeObject = ListBoxAttributes.SelectedItem as AttributeObject;
            if (attributeObject == null)
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
            // Fill all values
            var itemObject = ListBoxItems.SelectedItem as ItemObject;
            if (itemObject == null)
            {
                // Empty values
                TxtItemName.Text = string.Empty;
            }
            else
            {
                TxtItemName.Text = itemObject.Name;

                // Trigger selected index change
                ListBoxItemAttributes.SelectedIndex = -1;
                if (ListBoxItemAttributes.Items.Count > 0)
                    ListBoxItemAttributes.SelectedIndex = 0;
            }
        }

        private void ListBoxItemAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var itemObject = ListBoxItems.SelectedItem as ItemObject;
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

            if (attribute == null || itemObject == null)
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

            if (ListBoxItems.Items.Cast<ItemObject>()
                .Any(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
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
        }

        private void BtnRemoveSelectedItem_Click(object sender, EventArgs e)
        {
            var itemObject = ListBoxItems.SelectedItem as ItemObject;
            if (itemObject != null)
            {
                ListBoxItems.Items.Remove(itemObject);
                _items.Remove(itemObject.Name);
            }
        }

        private void BtnSetItemAttributeValue_Click(object sender, EventArgs e)
        {
            var itemObject = ListBoxItems.SelectedItem as ItemObject;
            if (itemObject == null) return;

            var attribute = ListBoxItemAttributes.SelectedItem as AttributeObject;
            if (attribute == null) return;

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
    }
}
