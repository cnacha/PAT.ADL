using System;
using System.Drawing;
using System.Windows.Forms;
using Fireball.Docking;

namespace PAT.Main
{
    public class OutputDockingWindow : DockableWindow
    {
        private ToolStripContainer ToolStripContainer;
        private ToolStrip ToolStrip;
        private ToolStripButton Button_Clear;
        private RichTextBox TextBox_Content;

        public RichTextBox TextBox
        {
            get
            {
               return this.TextBox_Content;
            }

        }
        
        public OutputDockingWindow()
        {
            InitializeComponent();

            this.DockableAreas = DockAreas.DockBottom | DockAreas.Float;
            this.TextBox_Content.TextChanged += new EventHandler(TextBox_TextChanged);
        }

        void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox_Content.Font = new Font(TextBox_Content.Font.FontFamily, 8, FontStyle.Regular);
            TextBox_Content.SelectAll();
            TextBox_Content.SelectionFont = TextBox_Content.Font;
            TextBox_Content.SelectionStart = 0;
            TextBox_Content.SelectionLength = 0;
        }


        private void ClearToolStripButton_Click(object sender, EventArgs e)
        {
            TextBox_Content.Text = string.Empty;
        }

        private void InitializeComponent()
        {
            this.ToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.TextBox_Content = new System.Windows.Forms.RichTextBox();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.Button_Clear = new System.Windows.Forms.ToolStripButton();
            this.ToolStripContainer.ContentPanel.SuspendLayout();
            this.ToolStripContainer.TopToolStripPanel.SuspendLayout();
            this.ToolStripContainer.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolStripContainer
            // 
            this.ToolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // ToolStripContainer.ContentPanel
            // 
            this.ToolStripContainer.ContentPanel.Controls.Add(this.TextBox_Content);
            this.ToolStripContainer.ContentPanel.Size = new System.Drawing.Size(292, 248);
            this.ToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ToolStripContainer.LeftToolStripPanelVisible = false;
            this.ToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.ToolStripContainer.Name = "ToolStripContainer";
            this.ToolStripContainer.RightToolStripPanelVisible = false;
            this.ToolStripContainer.Size = new System.Drawing.Size(292, 273);
            this.ToolStripContainer.TabIndex = 0;
            // 
            // ToolStripContainer.TopToolStripPanel
            // 
            this.ToolStripContainer.TopToolStripPanel.Controls.Add(this.ToolStrip);
            // 
            // TextBox_Content
            // 
            this.TextBox_Content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBox_Content.Location = new System.Drawing.Point(0, 0);
            this.TextBox_Content.Name = "TextBox_Content";
            this.TextBox_Content.Size = new System.Drawing.Size(292, 248);
            this.TextBox_Content.TabIndex = 0;
            this.TextBox_Content.Text = "";
            // 
            // ToolStrip
            // 
            this.ToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Button_Clear});
            this.ToolStrip.Location = new System.Drawing.Point(3, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Size = new System.Drawing.Size(35, 25);
            this.ToolStrip.TabIndex = 0;
            // 
            // Button_Clear
            // 
            this.Button_Clear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Button_Clear.Image = global::PAT.Main.Properties.Resources.Clear;
            this.Button_Clear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Button_Clear.Name = "Button_Clear";
            this.Button_Clear.Size = new System.Drawing.Size(23, 22);
            this.Button_Clear.Text = "Clear";
            this.Button_Clear.Click += new System.EventHandler(this.ClearToolStripButton_Click);
            // 
            // OutputDockingWindow
            // 
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.ToolStripContainer);
            this.Name = "OutputDockingWindow";
            this.TabText = "Output Window";
            this.Text = "Output Window";
            this.ToolStripContainer.ContentPanel.ResumeLayout(false);
            this.ToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.ToolStripContainer.TopToolStripPanel.PerformLayout();
            this.ToolStripContainer.ResumeLayout(false);
            this.ToolStripContainer.PerformLayout();
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}