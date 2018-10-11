using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using Fireball.Docking;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;
using PAT.ADL;
using PAT.Common;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Ultility;
using PAT.Main.Properties;


namespace PAT.Main
{
    public partial class FormMain : Form
    {
        public DockContainer DockContainer = null;
        public static SyntaxMode Language = null;
        private string OpenFilter;

        public OutputDockingWindow Output_Window = null;
        public ErrorListWindow ErrorListWindow = null;

        public FormMain()
        {
            InitializeComponent();

            DockContainer = new DockContainer();
            DockContainer.MainForm = this;
            DockContainer.Dock = DockStyle.Fill;

            this.toolStripContainer1.ContentPanel.Controls.Add(this.DockContainer);

            DockContainer.ShowDocumentIcon = true;

            DockContainer.DocumentStyle = DocumentStyles.DockingWindow;

            DockContainer.ActiveDocumentChanged += new EventHandler(_DockContainer_ActiveDocumentChanged);

            DockContainer.ActiveContentChanged += new EventHandler(_DockContainer_ActiveContentChanged);

            DockContainer.ContextMenu = new ContextMenu();

            Assembly exe = this.GetType().Assembly;

            ResourceService.RegisterNeutralStrings(new ResourceManager("PAT.Main.Properties.StringResources", exe));
            ResourceService.RegisterNeutralImages(new ResourceManager("PAT.Main.Properties.BitmapResources", exe));

            WorkbenchSingleton.DockContainer = DockContainer;
            WorkbenchSingleton.InitializeWorkbench();

            CoreStartup startup = new CoreStartup(Common.Ultility.Ultility.APPLICATION_NAME);
            startup.ConfigDirectory = Common.Ultility.Ultility.APPLICATION_PATH; 
            startup.DataDirectory = Common.Ultility.Ultility.APPLICATION_PATH; 

            startup.StartCoreServices();
            startup.RunInitialization();

            ((ToolStripProfessionalRenderer)StandardToolBar.Renderer).ColorTable.UseSystemColors = true;

            Output_Window = new OutputDockingWindow();
            Output_Window.Icon = Common.Ultility.Ultility.GetIcon("Output");
            Output_Window.Disposed += new EventHandler(output_Window_Disposed);

            ErrorListWindow = new ErrorListWindow();
            ErrorListWindow.Icon = Common.Ultility.Ultility.GetIcon("ErrorList");
            ErrorListWindow.Disposed += new EventHandler(ErrorListWindow_Disposed);
            ErrorListWindow.ListView.DoubleClick += new EventHandler(ErrorListView_DoubleClick);


            this.toolStripContainer1.TopToolStripPanel.Controls.Clear();
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.FunctionalToolBar);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.StandardToolBar);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.MenuStrip);

            StandardToolBar.Location = new Point(3, 24);
            FunctionalToolBar.Location = new Point(3, 49);
            FunctionalToolBar.Size = new Size(409, 25);
            MenuStrip.Dock = DockStyle.Top;
            MenuStrip.Location = new Point(0, 0);

            CheckForItemState();

            CurrentModule = new ModuleFacade();
            Language = HighlightingManager.Manager.AddHighlightingStrategy(Path.Combine(Application.StartupPath, "Syntax.xshd"));
            OpenFilter = Language.Name + " (*" + Language.ExtensionString + ")|*" + Language.ExtensionString;

            CurrentModule.ShowModel += new ShowModelHandler(ShowModel);
            CurrentModule.ExampleMenualToolbarInitialize(this.MenuButton_Examples);
            CurrentModule.ReadConfiguration();

            //Common.Ultility.Ultility.ModuleFolderNames.Add(moduleFolderName);
            Common.Ultility.Ultility.ModuleNames = new List<string>();
            Common.Ultility.Ultility.ModuleDictionary = new Dictionary<string, ModuleFacadeBase>();

            Common.Ultility.Ultility.ModuleNames.Add(Language.Name);
            Common.Ultility.Ultility.ModuleDictionary.Add(Language.Name, CurrentModule);
            Common.Ultility.Ultility.Images.ImageList.Images.Add(Language.Name, CurrentModule.ModuleIcon);
        
            this.FunctionalToolBar.SuspendLayout();
            CurrentModule.ToolbarInitialize(this.FunctionalToolBar, Toolbar_Button_Click);
            this.FunctionalToolBar.ResumeLayout();
        }


        private void Toolbar_Button_Click(object sender, EventArgs e)
        {
            try
            {
                if (ParseSpecification(true) != null)
                {
                    CurrentModule.PerformButtonClick(this.CurrentEditorTabItem.TabText.TrimEnd('*'), (sender as ToolStripButton).Text);
                }

            }
            catch (Exception ex)
            {
                
            }
        }

        private void ShowModel(string inputModel, string module)
        {
            this.DisableAllControls();
            try
            {
                EditorTabItem tabItem = AddDocument(module);
                tabItem.Text = inputModel;

                tabItem.TabText = tabItem.TabText.TrimEnd('*');
            }
            catch (Exception ex)
            {
                
            }
            this.EnableAllControls();
        }


        #region Editor Methods


        private void _DockContainer_ActiveContentChanged(object sender, EventArgs e)
        {
            this.CheckForItemState();

        }


        private void _DockContainer_ActiveDocumentChanged(object sender, EventArgs e)
        {
            this.CheckForItemState();

            if (this.CurrentEditorTabItem != null)
            {
                SetAllFileNameLabel(CurrentEditorTabItem.FileName);
            }
        }



        private void mnuOpen_Click(object sender, EventArgs e)
        {
            this.Open();
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            this.SaveAs();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }


        public void Save()
        {
            try
            {
                if (!this.CurrentEditorTabItem.TabText.EndsWith("*"))
                {
                    this.StatusLabel_Status.Text = "Model Saved";
                    return;
                }

                if (string.IsNullOrEmpty(this.CurrentEditorTabItem.FileName) || !this.CurrentEditorTabItem.HaveFileName)
                {
                    this.SaveAs();
                }
                else
                {
                    string filePath = this.CurrentEditorTabItem.FileName;
                    bool isReadOnly = ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
                    if (isReadOnly)
                    {
                        if (MessageBox.Show(Resources.This_file_is_read_only__Do_you_want_to_overwrite_it_, Resources.Saving_error, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                        {
                            File.SetAttributes(filePath, File.GetAttributes(filePath) & ~(FileAttributes.ReadOnly));
                        }
                        else
                        {
                            return;
                        }
                    }
                    this.CurrentEditorTabItem.Save(null);


                    if (CurrentEditorTabItem != null && CurrentEditorTabItem.TabText.EndsWith("*"))
                    {
                        CurrentEditorTabItem.TabText = CurrentEditorTabItem.TabText.TrimEnd('*');
                    }
                    ToolbarButton_Save.Enabled = false;
                    MenuButton_Save.Enabled = ToolbarButton_Save.Enabled;

                }
                SetAllFileNameLabel(this.CurrentEditorTabItem.FileName);

                this.StatusLabel_Status.Text = "Model Saved";

            }
            catch (FileNotFoundException ex)
            {
                this.SaveAs();
            }
            catch (System.UnauthorizedAccessException ex)
            {
                this.SaveAs();
            }
            catch (Exception ex)
            {

            }
        }

        public void Open()
        {
            try
            {
                OpenFileDialog opf = new OpenFileDialog();

                opf.Filter = OpenFilter;

                if (opf.ShowDialog() == DialogResult.OK)
                {
                    this.OpenFile(opf.FileName, true);
                }
            }
            catch (Exception ex)
            {

            }

        }


        public EditorTabItem CurrentEditorTabItem
        {
            get
            {
                return this.DockContainer.ActiveDocument as EditorTabItem;
            }
        }


        public EditorTabItem OpenFile(string filename, bool ShowMessageBox)
        {
            int N = DockContainer.Documents.Length;
            for (int i = 0; i < N; i++)
            {
                EditorTabItem item = DockContainer.Documents[i] as EditorTabItem;
                if (item != null)
                {
                    if (item.FileName == filename)
                    {
                        item.Activate();
                        CurrentActiveTab = item;
                        SetAllFileNameLabel(filename);
                        return item;
                    }
                }
            }

            if (File.Exists(filename))
            {
                try
                {
                    SyntaxMode ModuleSyntax = null;

                    foreach (string extension in Language.Extensions)
                    {
                        if (filename.ToLower().EndsWith(extension))
                        {
                            ModuleSyntax = Language;
                            break;
                        }
                    }



                    if (ModuleSyntax == null)
                    {
                        if (ShowMessageBox)
                        {
                            MessageBox.Show(Resources.Error_happened_in_opening_ + filename + ".\r\n" + Resources.File_format_is_not_supported_by_PAT_, Common.Ultility.Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        return null;
                    }

                    try
                    {
                        OpenFilter = "";

                        if (ModuleSyntax.Name == Language.Name)
                        {
                            OpenFilter = Language.Name + " (*" + Language.ExtensionString + ")|*" +
                                         Language.ExtensionString + "|" + OpenFilter;
                        }
                        else
                        {
                            OpenFilter += Language.Name + " (*" + Language.ExtensionString + ")|*" +
                                          Language.ExtensionString + "|";
                        }

                        OpenFilter += "All File (*.*)|*.*";
                    }
                    catch (Exception)
                    {

                    }


                    EditorTabItem tabItem = this.AddDocument(ModuleSyntax.Name);
                    tabItem.Open(filename);


                    SetAllFileNameLabel(filename);

                    if (tabItem.TabText.EndsWith("*"))
                    {
                        tabItem.TabText = tabItem.TabText.TrimEnd('*');
                    }

                    this.StatusLabel_Status.Text = "Ready";

                    return tabItem;

                }
                catch (Exception ex)
                {
                    if (ShowMessageBox)
                    {
                        MessageBox.Show(Resources.Open_Error_ + ex.Message + "\r\n" + Resources.Please_make_sure_that_the_format_is_correct_, Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                if (ShowMessageBox)
                {
                    MessageBox.Show(Resources.Open_Error__the_selected_file_is_not_found_, Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return null;
        }

        public static EditorTabItem CurrentActiveTab;

        public EditorTabItem AddDocument(string moduleName)
        {

            EditorTabItem tabItem = new EditorTabItem(moduleName);

            tabItem.Tag = "";

            tabItem.Show(DockContainer, DockState.Document);

            tabItem.CodeEditor.LineViewerStyle = LineViewerStyle.None;

            tabItem.FormClosing += new FormClosingEventHandler(tabItem_FormClosing);

            tabItem.CodeEditor.TextChanged += new EventHandler(editorControl_TextChanged);
            tabItem.TabActivited += new EditorTabItem.TabActivitedHandler(tabItem_Activated);
            tabItem.IsDirtyChanged += new EventHandler(editorControl_TextChanged);

            tabItem.Activate();

            CurrentActiveTab = tabItem;

            return tabItem;
        }


        private void tabItem_Activated(EditorTabItem tab)
        {
            CurrentActiveTab = tab;
        }



        private void tabItem_FormClosing(object sender, FormClosingEventArgs e)
        {
            EditorTabItem currentDoc = sender as EditorTabItem;
            if (currentDoc != null)
            {
                if (currentDoc.TabText.EndsWith("*"))
                {
                    DialogResult rt = MessageBox.Show(string.Format(Resources.Document__0__unsaved__Do_you_want_to_save_it_before_close_, currentDoc.TabText.TrimEnd('*')), Ultility.APPLICATION_NAME, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (rt == DialogResult.Yes)
                    {
                        this.Save();
                    }
                    else if (rt == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            if (DockContainer.Documents.Length > 1)
            {
                //this.MenuButton_NavigateBack.PerformClick();
            }
            else if (DockContainer.Documents.Length == 1)
            {
                while (this.FunctionalToolBar.Items.Count > 6)
                {
                    this.FunctionalToolBar.Items.RemoveAt(this.FunctionalToolBar.Items.Count - 1);
                }
                CurrentActiveTab = null;

            }

            CheckForItemState();

        }


        public void SaveAs()
        {
            try
            {
                SaveFileDialog svd = new SaveFileDialog();
                if (CurrentEditorTabItem != null)
                {
                    svd.Filter = CurrentEditorTabItem.FileExtension; 

                    if (svd.ShowDialog() == DialogResult.OK)
                    {
                        CurrentEditorTabItem.Save(svd.FileName);
                        SetAllFileNameLabel(svd.FileName);
                        CurrentEditorTabItem.SetSyntaxLanguageFromFile(svd.FileName);
                        CurrentEditorTabItem.TabText = Path.GetFileName(svd.FileName);

                        ToolbarButton_Save.Enabled = false;
                        MenuButton_Save.Enabled = ToolbarButton_Save.Enabled;

                        this.StatusLabel_Status.Text = "Model Saved";
                    }
                }

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {


            }
        }

        private void mnuUndo_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Output_Window.TextBox.Focused)
                {
                    this.Output_Window.TextBox.Undo();
                }
                else
                {
                    this.CurrentEditorTabItem.Undo();
                }
            }
            catch (Exception ex)
            {


            }
        }

        private void mnuRedo_Click(object sender, EventArgs e)
        {

            this.CurrentEditorTabItem.Redo();

        }

        private void mnuCut_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Cut();
        }

        private void MenuButton_Delete_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Delete();
        }

        private void mnuCopy_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Copy();
        }

        private void mnuPaste_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Paste();
        }

        private void mnuSelectAll_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.SelectAll();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }


        private void btnOpen_Click(object sender, EventArgs e)
        {
            this.Open();
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            this.SaveAs();
        }

        private void CheckForItemState()
        {
            if (this.CurrentEditorTabItem == null)
            {
                MenuButton_Undo.Enabled = false;
                MenuButton_Redo.Enabled = false;

                MenuButton_Delete.Enabled = false;
                MenuButton_Paste.Enabled = false;
                MenuButton_Cut.Enabled = false;
                MenuButton_Copy.Enabled = false;

                MenuButton_SelectAll.Enabled = false;

                ToolbarButton_Cut.Enabled = false;
                ToolbarButton_Copy.Enabled = false;
                ToolbarButton_Paste.Enabled = false;

                ToolbarButton_Redo.Enabled = false;
                ToolbarButton_Undo.Enabled = false;
                ToolbarButton_Save.Enabled = false;
                ToolbarButton_SaveAs.Enabled = false;

                MenuButton_Save.Enabled = false;
                MenuButton_SaveAs.Enabled = false;


                FunctionalToolBar.Enabled = false;

                return;
            }

            MenuButton_Undo.Enabled = this.CurrentEditorTabItem.CanUndo;
            MenuButton_Redo.Enabled = this.CurrentEditorTabItem.CanRedo;

            ToolbarButton_Save.Enabled = this.CurrentEditorTabItem.IsDirty;
            ToolbarButton_SaveAs.Enabled = true;


            MenuButton_Save.Enabled = ToolbarButton_Save.Enabled;
            MenuButton_SaveAs.Enabled = true;

            MenuButton_Delete.Enabled = true; // this.CurrentEditorTabItem.CanDelete;
            MenuButton_Paste.Enabled = this.CurrentEditorTabItem.CanPaste;
            MenuButton_Cut.Enabled = this.CurrentEditorTabItem.CanCut;
            MenuButton_Copy.Enabled = this.CurrentEditorTabItem.CanCopy;
            ;

            MenuButton_SelectAll.Enabled = this.CurrentEditorTabItem.CanSelectAll;

            ToolbarButton_Cut.Enabled = MenuButton_Cut.Enabled;
            ToolbarButton_Copy.Enabled = MenuButton_Copy.Enabled;
            ToolbarButton_Paste.Enabled = MenuButton_Paste.Enabled;

            ToolbarButton_Redo.Enabled = MenuButton_Redo.Enabled;
            ToolbarButton_Undo.Enabled = MenuButton_Undo.Enabled;


            FunctionalToolBar.Enabled = true;

        }

        private void SetAllFileNameLabel(string name)
        {
            stFileLabel.Text = Path.GetFileName(name);
            stFileLabel.ToolTipText = Resources.Double_click_to_open_the_folder_of_ + name;
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e)
        {
            this.CheckForItemState();
        }

        private void btnCut_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Cut();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {

            this.CurrentEditorTabItem.Copy();
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Paste();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Undo();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            this.CurrentEditorTabItem.Redo();
        }


        private void editorControl_TextChanged(object sender, EventArgs e)
        {
            this.CheckForItemState();
        }



        //store the current loaded module, if the next module is same as the current one, then don't need to reload it.
        private ModuleFacadeBase CurrentModule;

        /// <summary>
        /// The current editor showed on FireEdit
        /// </summary>
        public SharpDevelopTextAreaControl CurrentEditor
        {
            get
            {

                if (this.DockContainer.ActiveDocument == null || !(DockContainer.ActiveDocument is EditorTabItem))
                    return null;

                return ((EditorTabItem)this.DockContainer.ActiveDocument).CodeEditor;

            }
        }




        #endregion

        #region Enabled Disable Button
        private void DisableAllControls()
        {
            this.Cursor = Cursors.WaitCursor;
            this.FunctionalToolBar.Enabled = false;
            MenuStrip.Enabled = false;

        }

        private void EnableAllControls()
        {
            this.FunctionalToolBar.Enabled = true;
            MenuStrip.Enabled = true;
            this.Cursor = Cursors.Default;
        }
        #endregion


        private void Button_SpecParse_Click(object sender, EventArgs e)
        {
            ParseSpecification(true);
        }

        private SpecificationBase ParseSpecification(bool showVerbolMsg)
        {
            if (CurrentEditorTabItem == null || this.CurrentEditorTabItem.Text.Trim() == "")
            {
                if (showVerbolMsg)
                {
                    MessageBox.Show(Resources.Please_input_a_model_first_, Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return null;
            }

            DisableAllControls();
            SpecificationBase spec = null;
            try
            {
                //clear the error list
                if (!ErrorListWindow.IsDisposed)
                {
                    ErrorListWindow.Clear();
                }

                CurrentModule = new ModuleFacade();
                spec = CurrentModule.ParseSpecification(this.CurrentEditorTabItem.Text, "",
                                                        CurrentEditorTabItem.FileName);

                if (spec != null)
                {
                    CurrentEditorTabItem.Specification = spec;

                    if (spec.Errors.Count > 0)
                    {
                        string key = "";
                        foreach (KeyValuePair<string, ParsingException> pair in spec.Errors)
                        {
                            key = pair.Key;
                            break;
                        }
                        ParsingException parsingException = spec.Errors[key];
                        spec.Errors.Remove(key);
                        throw parsingException;
                    }

                    if (showVerbolMsg)
                    {
                        this.StatusLabel_Status.Text = Resources.Grammar_Checked;

                        MenuButton_OutputPanel.Checked = true;
                        Output_Window.TextBox.Text = spec.GetSpecification() + "\r\n" + Output_Window.TextBox.Text;
                        Output_Window.Show(DockContainer);

                        if (spec.Warnings.Count > 0)
                        {
                            this.MenuButton_ErrorList.Checked = true;
                            ErrorListWindow.AddWarnings(spec.Warnings);

                            //ErrorListWindow.Show(DockContainer);
                            ShowErrorMessage();
                        }
                    }


                    EnableAllControls();

                    return spec;
                }
                else
                {
                    EnableAllControls();

                    return null;
                }

            }
            catch (ParsingException ex)
            {

                EnableAllControls();

                if (showVerbolMsg)
                {
                    if (spec != null)
                    {
                        ErrorListWindow.AddWarnings(spec.Warnings);
                        ErrorListWindow.AddErrors(spec.Errors);
                        //MenuButton_ErrorList.Checked = true;                        
                    }


                    CurrentEditorTabItem.HandleParsingException(ex);
                    ErrorListWindow.InsertError(ex);
                    MenuButton_ErrorList.Checked = true;

                    if (ex.Line > 0)
                    {
                        MessageBox.Show(Resources.Parsing_error_at_line_ + ex.Line + Resources._column_ + ex.CharPositionInLine + ": " + ex.Text + "\nFile:" + ex.DisplayFileName + "\n" + ex.Message, Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        MenuButton_OutputPanel.Checked = true;
                        this.Output_Window.TextBox.Text = Resources.Parsing_error_at_line_ + ex.Line + Resources._column_ + ex.CharPositionInLine + ": " + ex.Text + "\nFile:" + ex.DisplayFileName + "\n" + ex.Message + "\r\n\r\n" + this.Output_Window.TextBox.Text; //"\n" + ex.StackTrace +     
                    }
                    else
                    {
                        MessageBox.Show(Resources.Parsing_error__ + ex.Message, Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        MenuButton_OutputPanel.Checked = true;
                        this.Output_Window.TextBox.Text = Resources.Parsing_error__ + ex.Message + "\r\n\r\n" + this.Output_Window.TextBox.Text; //"\n" + ex.StackTrace +     
                    }
                    ShowErrorMessage();
                }

            }
            catch (Exception ex)
            {
                EnableAllControls();
                if (showVerbolMsg)
                {
                    MessageBox.Show(Resources.Parsing_error__ + ex.Message, Ultility.APPLICATION_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MenuButton_OutputPanel.Checked = true;
                    this.Output_Window.TextBox.Text = Resources.Parsing_error__ + ex.Message + "\n" + ex.StackTrace + "\r\n\r\n" + this.Output_Window.TextBox.Text;
                }
            }

            return null;
        }

        private void ShowErrorMessage()
        {
            ErrorListWindow.Show(DockContainer);
        }


        private void Button_ModelChecking_Click(object sender, EventArgs e)
        {
            if (ParseSpecification(true) != null)
            {
                CurrentModule.ShowModelCheckingWindow(this.CurrentEditorTabItem.TabText.TrimEnd('*'));
            }
        }


        private void Button_Simulation_Click(object sender, EventArgs e)
        {
            //if the parsing is successful
            if (ParseSpecification(true) != null)
            {
                CurrentModule.ShowSimulationWindow(this.CurrentEditorTabItem.TabText.TrimEnd('*'));
            }
        }




        private void showResultPanelToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (MenuButton_OutputPanel.Checked)
                {
                    if (Output_Window.IsDisposed)
                    {
                        Output_Window = new OutputDockingWindow();
                        Output_Window.Icon = Ultility.GetIcon("Output");
                        Output_Window.Disposed += new EventHandler(output_Window_Disposed);
                    }
                    Output_Window.Show(DockContainer);
                }
                else
                {
                    Output_Window.Hide();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void showErrorListToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (MenuButton_ErrorList.Checked)
                {
                    if (ErrorListWindow.IsDisposed)
                    {
                        ErrorListWindow = new ErrorListWindow();
                        ErrorListWindow.Icon = Ultility.GetIcon("ErrorList");
                        ErrorListWindow.Disposed += new EventHandler(ErrorListWindow_Disposed);
                        ErrorListWindow.ListView.DoubleClick += new EventHandler(ErrorListView_DoubleClick);
                    }
                    ErrorListWindow.Show(DockContainer);
                }
                else
                {
                    try
                    {
                        ErrorListWindow.Hide();

                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
            }
        }




        private void ErrorListView_DoubleClick(object sender, EventArgs e)
        {
            if (ErrorListWindow.ListView.SelectedItems.Count > 0)
            {
                if (ErrorListWindow.ListView.SelectedItems[0].Tag != null && ErrorListWindow.ListView.SelectedItems[0].Tag is ParsingException)
                {
                    ParsingException ex = ErrorListWindow.ListView.SelectedItems[0].Tag as ParsingException;

                    OpenException(ex);
                }
            }
        }

        private void OpenException(ParsingException ex)
        {
            try
            {
                if (string.IsNullOrEmpty(ex.FileName))
                {
                    if (CurrentEditorTabItem != null)
                    {
                        this.CurrentEditorTabItem.HandleParsingException(ex);
                    }
                }
                else
                {
                    EditorTabItem item1 = OpenFile(ex.FileName, false);
                    if (item1 != null)
                    {
                        item1.HandleParsingException(ex);

                    }
                }
            }
            catch (Exception)
            {

            }
        }


        private void output_Window_Disposed(object sender, EventArgs e)
        {
            MenuButton_OutputPanel.Checked = false;

            if ((DockContainer.ActiveDocument as Control) != null)
            {
                (DockContainer.ActiveDocument as Control).Hide();
                (DockContainer.ActiveDocument as Control).Show();
            }
        }

        private void ErrorListWindow_Disposed(object sender, EventArgs e)
        {
            MenuButton_ErrorList.Checked = false;

            if ((DockContainer.ActiveDocument as Control) != null)
            {
                (DockContainer.ActiveDocument as Control).Hide();
                (DockContainer.ActiveDocument as Control).Show();
            }
        }



        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorTabItem tabItem = AddDocument(Language.Name);

            tabItem.TabText = tabItem.TabText.TrimEnd('*');
        }

    }
}