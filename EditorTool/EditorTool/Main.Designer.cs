using System.Drawing;
using System.Windows.Forms;

namespace EditorTool
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabAttributes = new TabPage();
            CmbAttributeFilter = new ComboBox();
            label10 = new Label();
            CmbAttributeAvailableFor = new ComboBox();
            label16 = new Label();
            BtnRemoveSelectedValue = new Button();
            BtnCreateNewValue = new Button();
            label9 = new Label();
            ListBoxAttributeValues = new ListBox();
            CmbAttributeType = new ComboBox();
            label3 = new Label();
            BtnRemoveSelectedAttribute = new Button();
            TxtAttributeName = new TextBox();
            label2 = new Label();
            BtnCreateAttribute = new Button();
            ListBoxAttributes = new ListBox();
            label1 = new Label();
            tabItems = new TabPage();
            CmbItemAttributeValue = new ComboBox();
            TxtItemAttributeValue = new TextBox();
            label8 = new Label();
            BtnSetItemAttributeValue = new Button();
            label7 = new Label();
            label6 = new Label();
            ListBoxItemAttributes = new ListBox();
            BtnRemoveSelectedItem = new Button();
            TxtItemName = new TextBox();
            label5 = new Label();
            BtnCreateItem = new Button();
            ListBoxItems = new ListBox();
            tabNpcs = new TabPage();
            CmbNpcItemPicker = new ComboBox();
            label4 = new Label();
            BtnRemoveNpcItem = new Button();
            BtnAddNpcItem = new Button();
            BtnDeleteSelectedNpc = new Button();
            BtnCreateNpc = new Button();
            CmbNpcAttributeValue = new ComboBox();
            TxtNpcAttributeValue = new TextBox();
            label15 = new Label();
            BtnSetNpcAttributeValue = new Button();
            label14 = new Label();
            ListBoxNpcAttributes = new ListBox();
            label13 = new Label();
            ListBoxDroppedItems = new ListBox();
            label11 = new Label();
            TxtNpcName = new TextBox();
            label12 = new Label();
            ListBoxNpcs = new ListBox();
            BtnSaveConfiguration = new Button();
            tabControl1.SuspendLayout();
            tabAttributes.SuspendLayout();
            tabItems.SuspendLayout();
            tabNpcs.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabAttributes);
            tabControl1.Controls.Add(tabItems);
            tabControl1.Controls.Add(tabNpcs);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(471, 460);
            tabControl1.TabIndex = 0;
            // 
            // tabAttributes
            // 
            tabAttributes.Controls.Add(CmbAttributeFilter);
            tabAttributes.Controls.Add(label10);
            tabAttributes.Controls.Add(CmbAttributeAvailableFor);
            tabAttributes.Controls.Add(label16);
            tabAttributes.Controls.Add(BtnRemoveSelectedValue);
            tabAttributes.Controls.Add(BtnCreateNewValue);
            tabAttributes.Controls.Add(label9);
            tabAttributes.Controls.Add(ListBoxAttributeValues);
            tabAttributes.Controls.Add(CmbAttributeType);
            tabAttributes.Controls.Add(label3);
            tabAttributes.Controls.Add(BtnRemoveSelectedAttribute);
            tabAttributes.Controls.Add(TxtAttributeName);
            tabAttributes.Controls.Add(label2);
            tabAttributes.Controls.Add(BtnCreateAttribute);
            tabAttributes.Controls.Add(ListBoxAttributes);
            tabAttributes.Controls.Add(label1);
            tabAttributes.Location = new Point(4, 24);
            tabAttributes.Name = "tabAttributes";
            tabAttributes.Padding = new Padding(3);
            tabAttributes.Size = new Size(463, 432);
            tabAttributes.TabIndex = 2;
            tabAttributes.Text = "Attributes";
            tabAttributes.UseVisualStyleBackColor = true;
            // 
            // CmbAttributeFilter
            // 
            CmbAttributeFilter.FormattingEnabled = true;
            CmbAttributeFilter.Items.AddRange(new object[] { "Show All", "Shared", "Items", "Npcs" });
            CmbAttributeFilter.Location = new Point(6, 31);
            CmbAttributeFilter.Name = "CmbAttributeFilter";
            CmbAttributeFilter.Size = new Size(216, 23);
            CmbAttributeFilter.TabIndex = 15;
            CmbAttributeFilter.Text = "Show All";
            CmbAttributeFilter.SelectedIndexChanged += CmbAttributeFilter_SelectedIndexChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 10F);
            label10.Location = new Point(6, 9);
            label10.Name = "label10";
            label10.Size = new Size(42, 19);
            label10.TabIndex = 14;
            label10.Text = "Filter:";
            // 
            // CmbAttributeAvailableFor
            // 
            CmbAttributeAvailableFor.FormattingEnabled = true;
            CmbAttributeAvailableFor.Location = new Point(233, 129);
            CmbAttributeAvailableFor.Name = "CmbAttributeAvailableFor";
            CmbAttributeAvailableFor.Size = new Size(224, 23);
            CmbAttributeAvailableFor.TabIndex = 13;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Segoe UI", 10F);
            label16.Location = new Point(233, 107);
            label16.Name = "label16";
            label16.Size = new Size(90, 19);
            label16.TabIndex = 12;
            label16.Text = "Available For:";
            // 
            // BtnRemoveSelectedValue
            // 
            BtnRemoveSelectedValue.Enabled = false;
            BtnRemoveSelectedValue.Location = new Point(233, 290);
            BtnRemoveSelectedValue.Name = "BtnRemoveSelectedValue";
            BtnRemoveSelectedValue.Size = new Size(224, 23);
            BtnRemoveSelectedValue.TabIndex = 11;
            BtnRemoveSelectedValue.Text = "Remove Selected Value";
            BtnRemoveSelectedValue.UseVisualStyleBackColor = true;
            BtnRemoveSelectedValue.Click += BtnRemoveSelectedValue_Click;
            // 
            // BtnCreateNewValue
            // 
            BtnCreateNewValue.Enabled = false;
            BtnCreateNewValue.Location = new Point(233, 261);
            BtnCreateNewValue.Name = "BtnCreateNewValue";
            BtnCreateNewValue.Size = new Size(224, 23);
            BtnCreateNewValue.TabIndex = 10;
            BtnCreateNewValue.Text = "Create New Value";
            BtnCreateNewValue.UseVisualStyleBackColor = true;
            BtnCreateNewValue.Click += BtnCreateNewValue_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 10F);
            label9.Location = new Point(233, 156);
            label9.Name = "label9";
            label9.Size = new Size(168, 19);
            label9.TabIndex = 9;
            label9.Text = "Attribute Available Values:";
            // 
            // ListBoxAttributeValues
            // 
            ListBoxAttributeValues.FormattingEnabled = true;
            ListBoxAttributeValues.Location = new Point(233, 178);
            ListBoxAttributeValues.Name = "ListBoxAttributeValues";
            ListBoxAttributeValues.Size = new Size(224, 79);
            ListBoxAttributeValues.TabIndex = 8;
            // 
            // CmbAttributeType
            // 
            CmbAttributeType.FormattingEnabled = true;
            CmbAttributeType.Location = new Point(233, 79);
            CmbAttributeType.Name = "CmbAttributeType";
            CmbAttributeType.Size = new Size(224, 23);
            CmbAttributeType.TabIndex = 7;
            CmbAttributeType.SelectedIndexChanged += CmbAttributeType_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10F);
            label3.Location = new Point(233, 57);
            label3.Name = "label3";
            label3.Size = new Size(99, 19);
            label3.TabIndex = 6;
            label3.Text = "Attribute Type:";
            // 
            // BtnRemoveSelectedAttribute
            // 
            BtnRemoveSelectedAttribute.Enabled = false;
            BtnRemoveSelectedAttribute.Location = new Point(233, 369);
            BtnRemoveSelectedAttribute.Name = "BtnRemoveSelectedAttribute";
            BtnRemoveSelectedAttribute.Size = new Size(224, 32);
            BtnRemoveSelectedAttribute.TabIndex = 5;
            BtnRemoveSelectedAttribute.Text = "Remove Selected Attribute";
            BtnRemoveSelectedAttribute.UseVisualStyleBackColor = true;
            BtnRemoveSelectedAttribute.Click += BtnRemoveSelectedAttribute_Click;
            // 
            // TxtAttributeName
            // 
            TxtAttributeName.Location = new Point(233, 31);
            TxtAttributeName.Name = "TxtAttributeName";
            TxtAttributeName.Size = new Size(224, 23);
            TxtAttributeName.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F);
            label2.Location = new Point(233, 9);
            label2.Name = "label2";
            label2.Size = new Size(107, 19);
            label2.TabIndex = 3;
            label2.Text = "Attribute Name:";
            // 
            // BtnCreateAttribute
            // 
            BtnCreateAttribute.Location = new Point(233, 335);
            BtnCreateAttribute.Name = "BtnCreateAttribute";
            BtnCreateAttribute.Size = new Size(224, 32);
            BtnCreateAttribute.TabIndex = 2;
            BtnCreateAttribute.Text = "Create Attribute";
            BtnCreateAttribute.UseVisualStyleBackColor = true;
            BtnCreateAttribute.Click += BtnCreateAttribute_Click;
            // 
            // ListBoxAttributes
            // 
            ListBoxAttributes.FormattingEnabled = true;
            ListBoxAttributes.Location = new Point(6, 66);
            ListBoxAttributes.Name = "ListBoxAttributes";
            ListBoxAttributes.Size = new Size(216, 334);
            ListBoxAttributes.TabIndex = 1;
            ListBoxAttributes.SelectedIndexChanged += ListBoxAttributes_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(79, 408);
            label1.Name = "label1";
            label1.Size = new Size(322, 21);
            label1.TabIndex = 0;
            label1.Text = "Attributes are shared between item and npcs.";
            // 
            // tabItems
            // 
            tabItems.Controls.Add(CmbItemAttributeValue);
            tabItems.Controls.Add(TxtItemAttributeValue);
            tabItems.Controls.Add(label8);
            tabItems.Controls.Add(BtnSetItemAttributeValue);
            tabItems.Controls.Add(label7);
            tabItems.Controls.Add(label6);
            tabItems.Controls.Add(ListBoxItemAttributes);
            tabItems.Controls.Add(BtnRemoveSelectedItem);
            tabItems.Controls.Add(TxtItemName);
            tabItems.Controls.Add(label5);
            tabItems.Controls.Add(BtnCreateItem);
            tabItems.Controls.Add(ListBoxItems);
            tabItems.Location = new Point(4, 24);
            tabItems.Name = "tabItems";
            tabItems.Padding = new Padding(3);
            tabItems.Size = new Size(463, 432);
            tabItems.TabIndex = 0;
            tabItems.Text = "Items";
            tabItems.UseVisualStyleBackColor = true;
            // 
            // CmbItemAttributeValue
            // 
            CmbItemAttributeValue.FormattingEnabled = true;
            CmbItemAttributeValue.Location = new Point(228, 282);
            CmbItemAttributeValue.Name = "CmbItemAttributeValue";
            CmbItemAttributeValue.Size = new Size(229, 23);
            CmbItemAttributeValue.TabIndex = 23;
            // 
            // TxtItemAttributeValue
            // 
            TxtItemAttributeValue.Location = new Point(228, 282);
            TxtItemAttributeValue.Name = "TxtItemAttributeValue";
            TxtItemAttributeValue.Size = new Size(229, 23);
            TxtItemAttributeValue.TabIndex = 21;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 10F);
            label8.Location = new Point(228, 260);
            label8.Name = "label8";
            label8.Size = new Size(104, 19);
            label8.TabIndex = 20;
            label8.Text = "Attribute Value:";
            // 
            // BtnSetItemAttributeValue
            // 
            BtnSetItemAttributeValue.Location = new Point(228, 311);
            BtnSetItemAttributeValue.Name = "BtnSetItemAttributeValue";
            BtnSetItemAttributeValue.Size = new Size(229, 34);
            BtnSetItemAttributeValue.TabIndex = 19;
            BtnSetItemAttributeValue.Text = "Set Value";
            BtnSetItemAttributeValue.UseVisualStyleBackColor = true;
            BtnSetItemAttributeValue.Click += BtnSetItemAttributeValue_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10F);
            label7.Location = new Point(6, 231);
            label7.Name = "label7";
            label7.Size = new Size(105, 19);
            label7.TabIndex = 18;
            label7.Text = "Item Attributes:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10F);
            label6.Location = new Point(6, 3);
            label6.Name = "label6";
            label6.Size = new Size(46, 19);
            label6.TabIndex = 17;
            label6.Text = "Items:";
            // 
            // ListBoxItemAttributes
            // 
            ListBoxItemAttributes.FormattingEnabled = true;
            ListBoxItemAttributes.Location = new Point(6, 253);
            ListBoxItemAttributes.Name = "ListBoxItemAttributes";
            ListBoxItemAttributes.Size = new Size(216, 169);
            ListBoxItemAttributes.TabIndex = 16;
            ListBoxItemAttributes.SelectedIndexChanged += ListBoxItemAttributes_SelectedIndexChanged;
            // 
            // BtnRemoveSelectedItem
            // 
            BtnRemoveSelectedItem.Location = new Point(228, 119);
            BtnRemoveSelectedItem.Name = "BtnRemoveSelectedItem";
            BtnRemoveSelectedItem.Size = new Size(229, 34);
            BtnRemoveSelectedItem.TabIndex = 12;
            BtnRemoveSelectedItem.Text = "Remove Selected Item";
            BtnRemoveSelectedItem.UseVisualStyleBackColor = true;
            BtnRemoveSelectedItem.Click += BtnRemoveSelectedItem_Click;
            // 
            // TxtItemName
            // 
            TxtItemName.Location = new Point(228, 53);
            TxtItemName.Name = "TxtItemName";
            TxtItemName.Size = new Size(229, 23);
            TxtItemName.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10F);
            label5.Location = new Point(228, 31);
            label5.Name = "label5";
            label5.Size = new Size(80, 19);
            label5.TabIndex = 10;
            label5.Text = "Item Name:";
            // 
            // BtnCreateItem
            // 
            BtnCreateItem.Location = new Point(228, 79);
            BtnCreateItem.Name = "BtnCreateItem";
            BtnCreateItem.Size = new Size(229, 34);
            BtnCreateItem.TabIndex = 9;
            BtnCreateItem.Text = "Create Item";
            BtnCreateItem.UseVisualStyleBackColor = true;
            BtnCreateItem.Click += BtnCreateItem_Click;
            // 
            // ListBoxItems
            // 
            ListBoxItems.FormattingEnabled = true;
            ListBoxItems.Location = new Point(6, 25);
            ListBoxItems.Name = "ListBoxItems";
            ListBoxItems.Size = new Size(216, 199);
            ListBoxItems.TabIndex = 8;
            ListBoxItems.SelectedIndexChanged += ListBoxItems_SelectedIndexChanged;
            // 
            // tabNpcs
            // 
            tabNpcs.Controls.Add(CmbNpcItemPicker);
            tabNpcs.Controls.Add(label4);
            tabNpcs.Controls.Add(BtnRemoveNpcItem);
            tabNpcs.Controls.Add(BtnAddNpcItem);
            tabNpcs.Controls.Add(BtnDeleteSelectedNpc);
            tabNpcs.Controls.Add(BtnCreateNpc);
            tabNpcs.Controls.Add(CmbNpcAttributeValue);
            tabNpcs.Controls.Add(TxtNpcAttributeValue);
            tabNpcs.Controls.Add(label15);
            tabNpcs.Controls.Add(BtnSetNpcAttributeValue);
            tabNpcs.Controls.Add(label14);
            tabNpcs.Controls.Add(ListBoxNpcAttributes);
            tabNpcs.Controls.Add(label13);
            tabNpcs.Controls.Add(ListBoxDroppedItems);
            tabNpcs.Controls.Add(label11);
            tabNpcs.Controls.Add(TxtNpcName);
            tabNpcs.Controls.Add(label12);
            tabNpcs.Controls.Add(ListBoxNpcs);
            tabNpcs.Location = new Point(4, 24);
            tabNpcs.Name = "tabNpcs";
            tabNpcs.Padding = new Padding(3);
            tabNpcs.Size = new Size(463, 432);
            tabNpcs.TabIndex = 1;
            tabNpcs.Text = "Npcs";
            tabNpcs.UseVisualStyleBackColor = true;
            // 
            // CmbNpcItemPicker
            // 
            CmbNpcItemPicker.FormattingEnabled = true;
            CmbNpcItemPicker.Location = new Point(6, 360);
            CmbNpcItemPicker.Name = "CmbNpcItemPicker";
            CmbNpcItemPicker.Size = new Size(216, 23);
            CmbNpcItemPicker.TabIndex = 36;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10F);
            label4.Location = new Point(6, 338);
            label4.Name = "label4";
            label4.Size = new Size(40, 19);
            label4.TabIndex = 34;
            label4.Text = "Item:";
            // 
            // BtnRemoveNpcItem
            // 
            BtnRemoveNpcItem.Location = new Point(83, 389);
            BtnRemoveNpcItem.Name = "BtnRemoveNpcItem";
            BtnRemoveNpcItem.Size = new Size(139, 34);
            BtnRemoveNpcItem.TabIndex = 33;
            BtnRemoveNpcItem.Text = "Remove Selected Item";
            BtnRemoveNpcItem.UseVisualStyleBackColor = true;
            BtnRemoveNpcItem.Click += BtnRemoveNpcItem_Click;
            // 
            // BtnAddNpcItem
            // 
            BtnAddNpcItem.Location = new Point(6, 389);
            BtnAddNpcItem.Name = "BtnAddNpcItem";
            BtnAddNpcItem.Size = new Size(71, 34);
            BtnAddNpcItem.TabIndex = 32;
            BtnAddNpcItem.Text = "Add Item";
            BtnAddNpcItem.UseVisualStyleBackColor = true;
            BtnAddNpcItem.Click += BtnAddNpcItem_Click;
            // 
            // BtnDeleteSelectedNpc
            // 
            BtnDeleteSelectedNpc.Location = new Point(228, 117);
            BtnDeleteSelectedNpc.Name = "BtnDeleteSelectedNpc";
            BtnDeleteSelectedNpc.Size = new Size(229, 34);
            BtnDeleteSelectedNpc.TabIndex = 31;
            BtnDeleteSelectedNpc.Text = "Delete Selected NPC";
            BtnDeleteSelectedNpc.UseVisualStyleBackColor = true;
            BtnDeleteSelectedNpc.Click += BtnDeleteSelectedNpc_Click;
            // 
            // BtnCreateNpc
            // 
            BtnCreateNpc.Location = new Point(228, 81);
            BtnCreateNpc.Name = "BtnCreateNpc";
            BtnCreateNpc.Size = new Size(229, 34);
            BtnCreateNpc.TabIndex = 30;
            BtnCreateNpc.Text = "Create NPC";
            BtnCreateNpc.UseVisualStyleBackColor = true;
            BtnCreateNpc.Click += BtnCreateNpc_Click;
            // 
            // CmbNpcAttributeValue
            // 
            CmbNpcAttributeValue.FormattingEnabled = true;
            CmbNpcAttributeValue.Location = new Point(228, 360);
            CmbNpcAttributeValue.Name = "CmbNpcAttributeValue";
            CmbNpcAttributeValue.Size = new Size(229, 23);
            CmbNpcAttributeValue.TabIndex = 29;
            // 
            // TxtNpcAttributeValue
            // 
            TxtNpcAttributeValue.Location = new Point(228, 360);
            TxtNpcAttributeValue.Name = "TxtNpcAttributeValue";
            TxtNpcAttributeValue.Size = new Size(229, 23);
            TxtNpcAttributeValue.TabIndex = 28;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 10F);
            label15.Location = new Point(228, 338);
            label15.Name = "label15";
            label15.Size = new Size(104, 19);
            label15.TabIndex = 27;
            label15.Text = "Attribute Value:";
            // 
            // BtnSetNpcAttributeValue
            // 
            BtnSetNpcAttributeValue.Location = new Point(228, 389);
            BtnSetNpcAttributeValue.Name = "BtnSetNpcAttributeValue";
            BtnSetNpcAttributeValue.Size = new Size(229, 34);
            BtnSetNpcAttributeValue.TabIndex = 26;
            BtnSetNpcAttributeValue.Text = "Set Value";
            BtnSetNpcAttributeValue.UseVisualStyleBackColor = true;
            BtnSetNpcAttributeValue.Click += BtnSetNpcAttributeValue_Click;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 10F);
            label14.Location = new Point(228, 159);
            label14.Name = "label14";
            label14.Size = new Size(101, 19);
            label14.TabIndex = 25;
            label14.Text = "Npc Attributes:";
            // 
            // ListBoxNpcAttributes
            // 
            ListBoxNpcAttributes.FormattingEnabled = true;
            ListBoxNpcAttributes.Location = new Point(228, 181);
            ListBoxNpcAttributes.Name = "ListBoxNpcAttributes";
            ListBoxNpcAttributes.Size = new Size(229, 154);
            ListBoxNpcAttributes.TabIndex = 24;
            ListBoxNpcAttributes.SelectedIndexChanged += ListBoxNpcAttributes_SelectedIndexChanged;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 10F);
            label13.Location = new Point(6, 206);
            label13.Name = "label13";
            label13.Size = new Size(104, 19);
            label13.TabIndex = 23;
            label13.Text = "Dropped Items:";
            // 
            // ListBoxDroppedItems
            // 
            ListBoxDroppedItems.FormattingEnabled = true;
            ListBoxDroppedItems.Location = new Point(6, 226);
            ListBoxDroppedItems.Name = "ListBoxDroppedItems";
            ListBoxDroppedItems.Size = new Size(216, 109);
            ListBoxDroppedItems.TabIndex = 22;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 10F);
            label11.Location = new Point(6, 8);
            label11.Name = "label11";
            label11.Size = new Size(42, 19);
            label11.TabIndex = 21;
            label11.Text = "Npcs:";
            // 
            // TxtNpcName
            // 
            TxtNpcName.Location = new Point(228, 54);
            TxtNpcName.Name = "TxtNpcName";
            TxtNpcName.Size = new Size(229, 23);
            TxtNpcName.TabIndex = 20;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 10F);
            label12.Location = new Point(228, 32);
            label12.Name = "label12";
            label12.Size = new Size(79, 19);
            label12.TabIndex = 19;
            label12.Text = "NPC Name:";
            // 
            // ListBoxNpcs
            // 
            ListBoxNpcs.FormattingEnabled = true;
            ListBoxNpcs.Location = new Point(6, 30);
            ListBoxNpcs.Name = "ListBoxNpcs";
            ListBoxNpcs.Size = new Size(216, 169);
            ListBoxNpcs.TabIndex = 18;
            ListBoxNpcs.SelectedIndexChanged += ListBoxNpcs_SelectedIndexChanged;
            // 
            // BtnSaveConfiguration
            // 
            BtnSaveConfiguration.Location = new Point(12, 474);
            BtnSaveConfiguration.Name = "BtnSaveConfiguration";
            BtnSaveConfiguration.Size = new Size(467, 35);
            BtnSaveConfiguration.TabIndex = 3;
            BtnSaveConfiguration.Text = "Save Game Data Configuration";
            BtnSaveConfiguration.UseVisualStyleBackColor = true;
            BtnSaveConfiguration.Click += BtnSaveConfiguration_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(496, 514);
            Controls.Add(BtnSaveConfiguration);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Main";
            Text = "Primora Editor Tool";
            Load += Main_Load;
            tabControl1.ResumeLayout(false);
            tabAttributes.ResumeLayout(false);
            tabAttributes.PerformLayout();
            tabItems.ResumeLayout(false);
            tabItems.PerformLayout();
            tabNpcs.ResumeLayout(false);
            tabNpcs.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabAttributes;
        private TabPage tabItems;
        private TabPage tabNpcs;
        private Button BtnRemoveSelectedAttribute;
        private TextBox TxtAttributeName;
        private Label label2;
        private Button BtnCreateAttribute;
        private ListBox ListBoxAttributes;
        private Label label1;
        private ComboBox CmbAttributeType;
        private Label label3;
        private Button BtnRemoveSelectedItem;
        private TextBox TxtItemName;
        private Label label5;
        private Button BtnCreateItem;
        private ListBox ListBoxItems;
        private Label label7;
        private Label label6;
        private ListBox ListBoxItemAttributes;
        private TextBox TxtItemAttributeValue;
        private Label label8;
        private Button BtnSetItemAttributeValue;
        private Button BtnRemoveSelectedValue;
        private Button BtnCreateNewValue;
        private Label label9;
        private ListBox ListBoxAttributeValues;
        private ComboBox CmbItemAttributeValue;
        private Button BtnDeleteSelectedNpc;
        private Button BtnCreateNpc;
        private ComboBox CmbNpcAttributeValue;
        private TextBox TxtNpcAttributeValue;
        private Label label15;
        private Button BtnSetNpcAttributeValue;
        private Label label14;
        private ListBox ListBoxNpcAttributes;
        private Label label13;
        private ListBox ListBoxDroppedItems;
        private Label label11;
        private TextBox TxtNpcName;
        private Label label12;
        private ListBox ListBoxNpcs;
        private Button BtnRemoveNpcItem;
        private Button BtnAddNpcItem;
        private ComboBox CmbAttributeAvailableFor;
        private Label label16;
        private Button BtnSaveConfiguration;
        private ComboBox CmbNpcItemPicker;
        private Label label4;
        private ComboBox CmbAttributeFilter;
        private Label label10;
    }
}
