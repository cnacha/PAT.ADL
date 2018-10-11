namespace PAT.Main
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.StatusBar = new System.Windows.Forms.StatusStrip();
            this.StatusLabel_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.stFileLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.stLineLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.stColumLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.stKeyMod = new System.Windows.Forms.ToolStripStatusLabel();
            this.FunctionalToolBar = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripLabel();
            this.ToolbarButton_Specification = new System.Windows.Forms.ToolStripLabel();
            this.ToolbarButton_CheckGrammar = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarButton_Simulation = new System.Windows.Forms.ToolStripButton();
            this.ToolbarButton_Verification = new System.Windows.Forms.ToolStripButton();
            this.StandardToolBar = new System.Windows.Forms.ToolStrip();
            this.ToolbarButton_Open = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarButton_Save = new System.Windows.Forms.ToolStripButton();
            this.ToolbarButton_SaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarButton_Cut = new System.Windows.Forms.ToolStripButton();
            this.ToolbarButton_Copy = new System.Windows.Forms.ToolStripButton();
            this.ToolbarButton_Paste = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolbarButton_Undo = new System.Windows.Forms.ToolStripButton();
            this.ToolbarButton_Redo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.MenuButton_File = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_New = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Open = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuButton_Save = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_SaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator34 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuButton_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Edit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Undo = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Redo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuButton_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Cut = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuButton_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_View = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_OutputPanel = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_ErrorList = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuButton_Examples = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.ToolbarButton_New = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.StatusBar.SuspendLayout();
            this.FunctionalToolBar.SuspendLayout();
            this.StandardToolBar.SuspendLayout();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.StatusBar);
            // 
            // toolStripContainer1.ContentPanel
            // 
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.MenuStrip);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.StandardToolBar);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.FunctionalToolBar);
            // 
            // StatusBar
            // 
            resources.ApplyResources(this.StatusBar, "StatusBar");
            this.StatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel_Status,
            this.stFileLabel,
            this.stLineLabel,
            this.stColumLabel,
            this.stKeyMod});
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            // 
            // StatusLabel_Status
            // 
            this.StatusLabel_Status.Name = "StatusLabel_Status";
            resources.ApplyResources(this.StatusLabel_Status, "StatusLabel_Status");
            // 
            // stFileLabel
            // 
            this.stFileLabel.AutoToolTip = true;
            this.stFileLabel.DoubleClickEnabled = true;
            this.stFileLabel.Name = "stFileLabel";
            resources.ApplyResources(this.stFileLabel, "stFileLabel");
            this.stFileLabel.Spring = true;
            // 
            // stLineLabel
            // 
            this.stLineLabel.Name = "stLineLabel";
            resources.ApplyResources(this.stLineLabel, "stLineLabel");
            // 
            // stColumLabel
            // 
            this.stColumLabel.Name = "stColumLabel";
            resources.ApplyResources(this.stColumLabel, "stColumLabel");
            // 
            // stKeyMod
            // 
            this.stKeyMod.Name = "stKeyMod";
            resources.ApplyResources(this.stKeyMod, "stKeyMod");
            // 
            // FunctionalToolBar
            // 
            resources.ApplyResources(this.FunctionalToolBar, "FunctionalToolBar");
            this.FunctionalToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator18,
            this.ToolbarButton_Specification,
            this.ToolbarButton_CheckGrammar,
            this.toolStripSeparator20,
            this.ToolbarButton_Simulation,
            this.ToolbarButton_Verification});
            this.FunctionalToolBar.Name = "FunctionalToolBar";
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            resources.ApplyResources(this.toolStripSeparator18, "toolStripSeparator18");
            // 
            // ToolbarButton_Specification
            // 
            this.ToolbarButton_Specification.Name = "ToolbarButton_Specification";
            resources.ApplyResources(this.ToolbarButton_Specification, "ToolbarButton_Specification");
            // 
            // ToolbarButton_CheckGrammar
            // 
            this.ToolbarButton_CheckGrammar.Image = global::PAT.Main.Properties.Resources._04378;
            resources.ApplyResources(this.ToolbarButton_CheckGrammar, "ToolbarButton_CheckGrammar");
            this.ToolbarButton_CheckGrammar.Name = "ToolbarButton_CheckGrammar";
            this.ToolbarButton_CheckGrammar.Click += new System.EventHandler(this.Button_SpecParse_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            resources.ApplyResources(this.toolStripSeparator20, "toolStripSeparator20");
            // 
            // ToolbarButton_Simulation
            // 
            resources.ApplyResources(this.ToolbarButton_Simulation, "ToolbarButton_Simulation");
            this.ToolbarButton_Simulation.Name = "ToolbarButton_Simulation";
            this.ToolbarButton_Simulation.Tag = "Simulation";
            this.ToolbarButton_Simulation.Click += new System.EventHandler(this.Button_Simulation_Click);
            // 
            // ToolbarButton_Verification
            // 
            resources.ApplyResources(this.ToolbarButton_Verification, "ToolbarButton_Verification");
            this.ToolbarButton_Verification.Name = "ToolbarButton_Verification";
            this.ToolbarButton_Verification.Tag = "Verification";
            this.ToolbarButton_Verification.Click += new System.EventHandler(this.Button_ModelChecking_Click);
            // 
            // StandardToolBar
            // 
            resources.ApplyResources(this.StandardToolBar, "StandardToolBar");
            this.StandardToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolbarButton_New,
            this.ToolbarButton_Open,
            this.toolStripSeparator5,
            this.ToolbarButton_Save,
            this.ToolbarButton_SaveAs,
            this.toolStripSeparator10,
            this.ToolbarButton_Cut,
            this.ToolbarButton_Copy,
            this.ToolbarButton_Paste,
            this.toolStripSeparator7,
            this.ToolbarButton_Undo,
            this.ToolbarButton_Redo,
            this.toolStripSeparator8});
            this.StandardToolBar.Name = "StandardToolBar";
            this.StandardToolBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            // 
            // ToolbarButton_Open
            // 
            this.ToolbarButton_Open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarButton_Open.Image = global::PAT.Main.Properties.Resources.open_spec;
            resources.ApplyResources(this.ToolbarButton_Open, "ToolbarButton_Open");
            this.ToolbarButton_Open.Name = "ToolbarButton_Open";
            this.ToolbarButton_Open.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // ToolbarButton_Save
            // 
            this.ToolbarButton_Save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.ToolbarButton_Save, "ToolbarButton_Save");
            this.ToolbarButton_Save.Name = "ToolbarButton_Save";
            this.ToolbarButton_Save.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // ToolbarButton_SaveAs
            // 
            this.ToolbarButton_SaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarButton_SaveAs.Image = global::PAT.Main.Properties.Resources.save_as;
            resources.ApplyResources(this.ToolbarButton_SaveAs, "ToolbarButton_SaveAs");
            this.ToolbarButton_SaveAs.Name = "ToolbarButton_SaveAs";
            this.ToolbarButton_SaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            // 
            // ToolbarButton_Cut
            // 
            this.ToolbarButton_Cut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarButton_Cut.Image = global::PAT.Main.Properties.Resources.cut;
            resources.ApplyResources(this.ToolbarButton_Cut, "ToolbarButton_Cut");
            this.ToolbarButton_Cut.Name = "ToolbarButton_Cut";
            this.ToolbarButton_Cut.Click += new System.EventHandler(this.btnCut_Click);
            // 
            // ToolbarButton_Copy
            // 
            this.ToolbarButton_Copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarButton_Copy.Image = global::PAT.Main.Properties.Resources.copy;
            resources.ApplyResources(this.ToolbarButton_Copy, "ToolbarButton_Copy");
            this.ToolbarButton_Copy.Name = "ToolbarButton_Copy";
            this.ToolbarButton_Copy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // ToolbarButton_Paste
            // 
            this.ToolbarButton_Paste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarButton_Paste.Image = global::PAT.Main.Properties.Resources.paste;
            resources.ApplyResources(this.ToolbarButton_Paste, "ToolbarButton_Paste");
            this.ToolbarButton_Paste.Name = "ToolbarButton_Paste";
            this.ToolbarButton_Paste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // ToolbarButton_Undo
            // 
            this.ToolbarButton_Undo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarButton_Undo.Image = global::PAT.Main.Properties.Resources.undo;
            resources.ApplyResources(this.ToolbarButton_Undo, "ToolbarButton_Undo");
            this.ToolbarButton_Undo.Name = "ToolbarButton_Undo";
            this.ToolbarButton_Undo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // ToolbarButton_Redo
            // 
            this.ToolbarButton_Redo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolbarButton_Redo.Image = global::PAT.Main.Properties.Resources.redo;
            resources.ApplyResources(this.ToolbarButton_Redo, "ToolbarButton_Redo");
            this.ToolbarButton_Redo.Name = "ToolbarButton_Redo";
            this.ToolbarButton_Redo.Click += new System.EventHandler(this.btnRedo_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            // 
            // MenuStrip
            // 
            resources.ApplyResources(this.MenuStrip, "MenuStrip");
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuButton_File,
            this.MenuButton_Edit,
            this.MenuButton_View,
            this.MenuButton_Examples});
            this.MenuStrip.Name = "MenuStrip";
            // 
            // MenuButton_File
            // 
            this.MenuButton_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuButton_New,
            this.MenuButton_Open,
            this.toolStripSeparator1,
            this.MenuButton_Save,
            this.MenuButton_SaveAs,
            this.toolStripSeparator34,
            this.MenuButton_Exit});
            this.MenuButton_File.Name = "MenuButton_File";
            resources.ApplyResources(this.MenuButton_File, "MenuButton_File");
            // 
            // MenuButton_New
            // 
            resources.ApplyResources(this.MenuButton_New, "MenuButton_New");
            this.MenuButton_New.Name = "MenuButton_New";
            this.MenuButton_New.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // MenuButton_Open
            // 
            this.MenuButton_Open.Image = global::PAT.Main.Properties.Resources.open_spec;
            this.MenuButton_Open.Name = "MenuButton_Open";
            resources.ApplyResources(this.MenuButton_Open, "MenuButton_Open");
            this.MenuButton_Open.Click += new System.EventHandler(this.mnuOpen_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // MenuButton_Save
            // 
            resources.ApplyResources(this.MenuButton_Save, "MenuButton_Save");
            this.MenuButton_Save.Name = "MenuButton_Save";
            this.MenuButton_Save.Click += new System.EventHandler(this.mnuSave_Click);
            // 
            // MenuButton_SaveAs
            // 
            this.MenuButton_SaveAs.Image = global::PAT.Main.Properties.Resources.save_as;
            this.MenuButton_SaveAs.Name = "MenuButton_SaveAs";
            resources.ApplyResources(this.MenuButton_SaveAs, "MenuButton_SaveAs");
            this.MenuButton_SaveAs.Click += new System.EventHandler(this.mnuSaveAs_Click);
            // 
            // toolStripSeparator34
            // 
            this.toolStripSeparator34.Name = "toolStripSeparator34";
            resources.ApplyResources(this.toolStripSeparator34, "toolStripSeparator34");
            // 
            // MenuButton_Exit
            // 
            this.MenuButton_Exit.Name = "MenuButton_Exit";
            resources.ApplyResources(this.MenuButton_Exit, "MenuButton_Exit");
            this.MenuButton_Exit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // MenuButton_Edit
            // 
            this.MenuButton_Edit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuButton_Undo,
            this.MenuButton_Redo,
            this.toolStripSeparator3,
            this.MenuButton_Delete,
            this.MenuButton_Cut,
            this.MenuButton_Copy,
            this.MenuButton_Paste,
            this.toolStripSeparator4,
            this.MenuButton_SelectAll});
            this.MenuButton_Edit.Name = "MenuButton_Edit";
            resources.ApplyResources(this.MenuButton_Edit, "MenuButton_Edit");
            this.MenuButton_Edit.DropDownOpening += new System.EventHandler(this.mnuEdit_DropDownOpening);
            // 
            // MenuButton_Undo
            // 
            this.MenuButton_Undo.Image = global::PAT.Main.Properties.Resources.undo;
            this.MenuButton_Undo.Name = "MenuButton_Undo";
            resources.ApplyResources(this.MenuButton_Undo, "MenuButton_Undo");
            this.MenuButton_Undo.Click += new System.EventHandler(this.mnuUndo_Click);
            // 
            // MenuButton_Redo
            // 
            this.MenuButton_Redo.Image = global::PAT.Main.Properties.Resources.redo;
            this.MenuButton_Redo.Name = "MenuButton_Redo";
            resources.ApplyResources(this.MenuButton_Redo, "MenuButton_Redo");
            this.MenuButton_Redo.Click += new System.EventHandler(this.mnuRedo_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // MenuButton_Delete
            // 
            this.MenuButton_Delete.Image = global::PAT.Main.Properties.Resources.delete;
            this.MenuButton_Delete.Name = "MenuButton_Delete";
            resources.ApplyResources(this.MenuButton_Delete, "MenuButton_Delete");
            this.MenuButton_Delete.Click += new System.EventHandler(this.MenuButton_Delete_Click);
            // 
            // MenuButton_Cut
            // 
            this.MenuButton_Cut.Image = global::PAT.Main.Properties.Resources.cut;
            this.MenuButton_Cut.Name = "MenuButton_Cut";
            resources.ApplyResources(this.MenuButton_Cut, "MenuButton_Cut");
            this.MenuButton_Cut.Click += new System.EventHandler(this.mnuCut_Click);
            // 
            // MenuButton_Copy
            // 
            this.MenuButton_Copy.Image = global::PAT.Main.Properties.Resources.copy;
            this.MenuButton_Copy.Name = "MenuButton_Copy";
            resources.ApplyResources(this.MenuButton_Copy, "MenuButton_Copy");
            this.MenuButton_Copy.Click += new System.EventHandler(this.mnuCopy_Click);
            // 
            // MenuButton_Paste
            // 
            this.MenuButton_Paste.Image = global::PAT.Main.Properties.Resources.paste;
            this.MenuButton_Paste.Name = "MenuButton_Paste";
            resources.ApplyResources(this.MenuButton_Paste, "MenuButton_Paste");
            this.MenuButton_Paste.Click += new System.EventHandler(this.mnuPaste_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // MenuButton_SelectAll
            // 
            this.MenuButton_SelectAll.Image = global::PAT.Main.Properties.Resources.selection;
            this.MenuButton_SelectAll.Name = "MenuButton_SelectAll";
            resources.ApplyResources(this.MenuButton_SelectAll, "MenuButton_SelectAll");
            this.MenuButton_SelectAll.Click += new System.EventHandler(this.mnuSelectAll_Click);
            // 
            // MenuButton_View
            // 
            this.MenuButton_View.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuButton_OutputPanel,
            this.MenuButton_ErrorList});
            this.MenuButton_View.Name = "MenuButton_View";
            resources.ApplyResources(this.MenuButton_View, "MenuButton_View");
            // 
            // MenuButton_OutputPanel
            // 
            this.MenuButton_OutputPanel.CheckOnClick = true;
            resources.ApplyResources(this.MenuButton_OutputPanel, "MenuButton_OutputPanel");
            this.MenuButton_OutputPanel.Name = "MenuButton_OutputPanel";
            this.MenuButton_OutputPanel.CheckStateChanged += new System.EventHandler(this.showResultPanelToolStripMenuItem_CheckStateChanged);
            // 
            // MenuButton_ErrorList
            // 
            this.MenuButton_ErrorList.CheckOnClick = true;
            resources.ApplyResources(this.MenuButton_ErrorList, "MenuButton_ErrorList");
            this.MenuButton_ErrorList.Name = "MenuButton_ErrorList";
            this.MenuButton_ErrorList.CheckedChanged += new System.EventHandler(this.showErrorListToolStripMenuItem_CheckStateChanged);
            // 
            // MenuButton_Examples
            // 
            this.MenuButton_Examples.Name = "MenuButton_Examples";
            resources.ApplyResources(this.MenuButton_Examples, "MenuButton_Examples");
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // ToolbarButton_New
            // 
            this.ToolbarButton_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.ToolbarButton_New, "ToolbarButton_New");
            this.ToolbarButton_New.Name = "ToolbarButton_New";
            this.ToolbarButton_New.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.toolStripContainer1);
            this.MainMenuStrip = this.MenuStrip;
            this.Name = "FormMain";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.StatusBar.ResumeLayout(false);
            this.StatusBar.PerformLayout();
            this.FunctionalToolBar.ResumeLayout(false);
            this.FunctionalToolBar.PerformLayout();
            this.StandardToolBar.ResumeLayout(false);
            this.StandardToolBar.PerformLayout();
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.StatusStrip StatusBar;
        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStrip StandardToolBar;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_File;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_New;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Open;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Save;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_SaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Exit;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Edit;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Undo;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Redo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Cut;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Copy;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Paste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_SelectAll;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Open;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Save;
        private System.Windows.Forms.ToolStripButton ToolbarButton_SaveAs;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Cut;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Copy;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Paste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Undo;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Redo;
        private System.Windows.Forms.ToolStripStatusLabel stFileLabel;
        private System.Windows.Forms.ToolStripStatusLabel stKeyMod;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripStatusLabel stLineLabel;
        private System.Windows.Forms.ToolStripStatusLabel stColumLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_View;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_OutputPanel;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Examples;
        private System.Windows.Forms.ToolStrip FunctionalToolBar;
        private System.Windows.Forms.ToolStripButton ToolbarButton_CheckGrammar;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Simulation;
        private System.Windows.Forms.ToolStripButton ToolbarButton_Verification;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel_Status;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripLabel toolStripSeparator18;
        private System.Windows.Forms.ToolStripLabel ToolbarButton_Specification;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_ErrorList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator34;
        private System.Windows.Forms.ToolStripMenuItem MenuButton_Delete;
        private System.Windows.Forms.ToolStripButton ToolbarButton_New;

    }
}