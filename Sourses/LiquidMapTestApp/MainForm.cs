using DotLiquid;
using Fluid;
using LiquidMapTestApp.Helpers;
using LiquidProcessor.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LiquidMapTestApp
{
    public partial class MainForm : Form
    {
        private const string FormHeader = "Liquid Template Tester";
        private static string FontName = "Consolas";
        private static int FontSize = 9;
        private readonly Font EditorDefaultFont = new Font(FontName, FontSize);

        private string dataFileName;
        private string templateFileName;

        private string TemplateFileName
        {
            get => this.templateFileName;
            set
            {
                this.templateFileName = value;
                this.Text = $"{FormHeader} ({this.templateFileName})";
            }
        }
        private string contentValue = "{}";
        private OutputFormat outputFormat = OutputFormat.PlainText;


        private string ContentString(string rootElement)
        {
            return "{" + rootElement + ":" + contentValue + "}";
        }

        private bool TemplateChanged { get; set; }

        private ITransformationService<Template> _dotLiquidTransformationService;
        private ITransformationService<IFluidTemplate> _fluidTransformationService;
        private MySyntaxHighlighter.SyntaxHighlighter _syntaxHighlighter;
        private bool disableHighlighting = false;

        public MainForm(
            ITransformationService<Template> dotLiquidTransformationService,
            ITransformationService<IFluidTemplate> fluidTransformationService
            )
        {
            InitializeComponent();

            this.DragEnter += new DragEventHandler(mainForm_DragEnter);
            this.Text = FormHeader;

            #region Data
            dataTreeView.AllowDrop = false;
            dataTreeView.ItemDrag += new ItemDragEventHandler(treeView1_ItemDrag);
            #endregion

            #region Template
            textBoxTemplate.AllowDrop = true;
            textBoxTemplate.EnableAutoDragDrop = true;
            textBoxTemplate.DragEnter += new DragEventHandler(textBox1_DragEnter);
            textBoxTemplate.DragDrop += new DragEventHandler(textBox1_DragDrop);
            #endregion

            #region Result
            #endregion

            _dotLiquidTransformationService = dotLiquidTransformationService;
            _fluidTransformationService = fluidTransformationService;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxData.Font = EditorDefaultFont;
            textBoxTemplate.Font = EditorDefaultFont;
            textBoxResult.Font = EditorDefaultFont;

            _syntaxHighlighter = new MySyntaxHighlighter.SyntaxHighlighter(textBoxTemplate);
            InitLiquidSyntaxHighlighter();

            checkBoxCSharpNaming.Checked = true;
            comboBoxRootElement.SelectedIndex = -1;

            cmbSourceType.SelectedIndex = (int)SourceFormat.Json;
            cmbResultType.SelectedIndex = (int)OutputFormat.Json;
            cmbEngineType.SelectedIndex = (int)EngineType.Fluent;

            tabControlResult.TabPages.Remove(tabPageResultJson);
            tabControlResult.TabPages.Remove(tabPageResultHTML);
        }

        private void mainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

            // Copy the dragged node when the right mouse button is used.
            else if (e.Button == MouseButtons.Right)
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            int i;
            string s;

            var textBox = textBoxResult;

            // Get start position to drop the text.  
            i = textBox.SelectionStart;
            s = textBox.Text.Substring(i);
            textBox.Text = textBox.Text.Substring(0, i);

            // Drop the text on to the RichTextBox.  
            textBox.Text = textBox.Text +
               e.Data.GetData(DataFormats.Text).ToString();
            textBox.Text = textBox.Text + s;

            e.Effect = DragDropEffects.None;
        }

        private void InitLiquidSyntaxHighlighter()
        {
            bool isDotLiquidEngine = cmbEngineType.SelectedIndex == (int)EngineType.DotLiquid;

            bool isCSharpNamingConvention = isDotLiquidEngine && checkBoxCSharpNaming.Checked;
            bool isFirstLetterUpper = isDotLiquidEngine;

            //var syntaxHighlighter = new SyntaxHighlighter(textBoxTemplate);
            //ApplyLiquidPatterns(syntaxHighlighter, isCSharpNamingConvention, isFirstLetterUpper);

            if (_syntaxHighlighter != null)
                _syntaxHighlighter.InitSyntax(isCSharpNamingConvention, isFirstLetterUpper);
        }

        private void DisplayText(RichTextBox textBox, string text)
        {
            textBox.Clear();
            textBox.Text = text;
        }

        private void DisplayData(string contentValue)
        {
            // load to TextBox
            DisplayText(textBoxData, contentValue);

            try
            {
                // load to TreeView

                var content = contentValue;

                if (cmbSourceType.SelectedIndex == (int)SourceFormat.Xml)
                {
                    content = JsonHelper.ConvertXMlToSJson(contentValue, removeSpecialCharacters: false);
                }

                RefreshDataTree(content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DisplayText(textBoxResult, "Input data errors: \n\n" + e.Message);
            }
        }

        private void RefreshDataTree(string content)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(content);
                ObjectToTreeView.SetObjectAsJson(dataTreeView, data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Refresh Json tree error: {ex.Message}", "Error", MessageBoxButtons.OK);
            }
        }

        private void DisplayTemplate(string text)
        {
            textBoxTemplate.Clear();
            textBoxTemplate.Text = text;
        }

        private void DisplayResult(string text)
        {
            if (chbxRemoveEmptyString.Checked)
            {
                var resultString = Regex.Replace(text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                text = resultString;
            }

            if (outputFormat == OutputFormat.Json && checkBoxAutoFormatJsonResult.Checked)
            {
                // format to JSON
                dynamic outputData = JsonConvert.DeserializeObject(text
                    , new JsonSerializerSettings() { FloatParseHandling = FloatParseHandling.Decimal }
                    );
                text = JsonConvert.SerializeObject(outputData, Newtonsoft.Json.Formatting.Indented);
            }

            DisplayText(textBoxResult, text);

            if (outputFormat == OutputFormat.Json)
            {
                dynamic outputData = JsonConvert.DeserializeObject(text);
                ObjectToTreeView.SetObjectAsJson(treeView2, outputData);
            }
            else if (outputFormat == OutputFormat.Html)
            {
                webBrowser1.DocumentText = text;
            }
        }


        private void openDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open Data File";
            openFileDialog1.Filter = "JSON files|*.json|All files|*.*";

            if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dataFileName = openFileDialog1.FileName;
                var fileContent = File.ReadAllText(openFileDialog1.FileName);

                contentValue = fileContent;

                DisplayData(contentValue);
            }
        }

        private void openTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog2.Title = "Open Map File";
            openFileDialog2.Filter = "Liquid map|*.liquid|JSON files|*.json|All files|*.*";

            if (!string.IsNullOrEmpty(openFileDialog2.FileName))
            {
                openFileDialog2.InitialDirectory = System.IO.Path.GetDirectoryName(openFileDialog2.FileName);
            }

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                TemplateFileName = openFileDialog2.FileName;

                RefreshTemplate();

            }

            refreshToolStripButton.Enabled = !string.IsNullOrEmpty(TemplateFileName);

        }

        private void RefreshTemplate()
        {
            string fileContent = File.ReadAllText(TemplateFileName, Encoding.UTF8);

            // load to TextBox
            DisplayTemplate(fileContent);

            executeToolStripMenuItem.Enabled = fileContent.Length > 0;
            toolStripButtonExecute.Enabled = executeToolStripMenuItem.Enabled;

            saveToolStripMenuItem.Enabled = false;
            saveToolStripButton.Enabled = saveToolStripMenuItem.Enabled;
        }

        private void viewSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = !viewSourceToolStripMenuItem.Checked;
        }

        private void buttonFormatJson_Click(object sender, EventArgs e)
        {
            var json = textBoxResult.Text;

            var resultString = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Newtonsoft.Json.Formatting.Indented);

            DisplayResult(resultString);

        }

        private void textBoxTemplate_TextChanged(object sender, EventArgs e)
        {
            executeToolStripMenuItem.Enabled = textBoxTemplate.TextLength > 0;
            toolStripButtonExecute.Enabled = executeToolStripMenuItem.Enabled;

            saveToolStripMenuItem.Enabled = textBoxTemplate.TextLength > 0;
            saveToolStripButton.Enabled = saveToolStripMenuItem.Enabled;

            // MANDATORY - focuses a label before highlighting (avoids blinking)
            labelTitle.Focus();

            if (_syntaxHighlighter != null)
            {
                _syntaxHighlighter.DisableHighlighting = disableHighlighting;

                if (_syntaxHighlighter.IsDuringHighlight)
                    return;

                _syntaxHighlighter.HighlightSyntax();
            }
            // giving back the focus
            textBoxTemplate.Focus();
        }

        private void textBoxData_TextChanged(object sender, EventArgs e)
        {
            contentValue = textBoxData.Text;
        }


        private void executeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var templateText = textBoxTemplate.Text;

                var rootElement = comboBoxRootElement.Text;

                bool useRubyNamingConvention = !checkBoxCSharpNaming.Checked;

                var content = contentValue;

                if (cmbSourceType.SelectedIndex == (int)SourceFormat.Xml)
                {
                    content = JsonHelper.ConvertXMlToSJson(contentValue, removeSpecialCharacters: false);
                }

                ILiquidTransformationService _transformationService = cmbEngineType.SelectedIndex == (int)EngineType.DotLiquid
                    ? _dotLiquidTransformationService as ILiquidTransformationService
                    : _fluidTransformationService as ILiquidTransformationService;

                var t1 = DateTime.Now;

                var result3 = _transformationService.Transform(templateText, content, rootElement, useRubyNamingConvention, out var errorMessage);

                var t2 = DateTime.Now;

                var delta = (t2 - t1).TotalMilliseconds;
                var estimatedSeconds = Math.Round(delta / 1000 * 10000, 5);
                var estimatedMinutes = Math.Round(estimatedSeconds / 60, 2);
                toolStripStatusLabel1.Text = $"Estimated time for 10000 iterations: {estimatedSeconds} secs, {estimatedMinutes} mins ";

                //if (toolStripButtonShowErrors.Checked && template.Errors.Any())
                //{
                //    var message = "";
                //    foreach (var error in template.Errors)
                //    {
                //        message += $"Message: {error.Message}\nInnerException: {error.InnerException?.Message}\n\n";
                //    }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    DisplayText(textBoxResult, "Transformer errors: \n\n" + errorMessage);
                }
                else
                {
                    DisplayResult(result3);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                DisplayText(textBoxResult, "ERROR: " + exception.Message);
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            outputFormat = (OutputFormat)cmbResultType.SelectedIndex;

            checkBoxAutoFormatJsonResult.Visible = outputFormat == OutputFormat.Json;
            buttonFormatJsonResult.Visible = outputFormat == OutputFormat.Json;
            if (outputFormat == OutputFormat.Json)
            {
                if (!tabControlResult.TabPages.Contains(tabPageResultJson))
                    tabControlResult.TabPages.Add(tabPageResultJson);
            }
            else
            {
                if (tabControlResult.TabPages.Contains(tabPageResultJson))
                    tabControlResult.TabPages.Remove(tabPageResultJson);
            }

            if (outputFormat == OutputFormat.Html)
            {
                if (!tabControlResult.TabPages.Contains(tabPageResultHTML))
                    tabControlResult.TabPages.Add(tabPageResultHTML);
            }
            else
            {
                if (tabControlResult.TabPages.Contains(tabPageResultHTML))
                    tabControlResult.TabPages.Remove(tabPageResultHTML);
            }
            webBrowser1.DocumentText = "";
        }

        private void checkBoxCSharpNaming_CheckedChanged(object sender, EventArgs e)
        {
            // MANDATORY - focuses a label before highlighting (avoids blinking)
            labelTitle.Focus();

            if (_syntaxHighlighter != null)
            {
                _syntaxHighlighter.DisableHighlighting = disableHighlighting;
                InitLiquidSyntaxHighlighter();
                _syntaxHighlighter.HighlightSyntax();
            }

            // giving back the focus
            textBoxTemplate.Focus();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TemplateFileName))
            {
                saveFileDialog1.FileName = TemplateFileName;
            }
            saveFileDialog1.OverwritePrompt = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxTemplate.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                TemplateFileName = saveFileDialog1.FileName;
                tabPageTemplateText.Text = "Template";
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            var text = textBoxDataSearch.Text;
            SearchText(textBoxData, text);
            SearchText(textBoxTemplate, text);
            SearchText(textBoxResult, text);
        }

        private void SearchText(RichTextBox textBox, string text)
        {
            ClearSelection(textBox);
            FindAndSelect(textBox, text);
        }

        private void FindAndSelect(RichTextBox textBox, string searchString)
        {
            string[] words = searchString.Split(',');
            foreach (string word in words)
            {
                int startindex = 0;
                while (startindex < textBox.TextLength)
                {
                    int wordstartIndex = textBox.Find(word, startindex, RichTextBoxFinds.None);
                    if (wordstartIndex != -1)
                    {
                        textBox.SelectionStart = wordstartIndex;
                        textBox.SelectionLength = word.Length;
                        textBox.SelectionBackColor = Color.Yellow;
                    }
                    else
                        break;
                    startindex += wordstartIndex + word.Length;
                }
            }

        }

        private void ClearSelection(RichTextBox textBox)
        {
            textBox.SelectionStart = 0;
            textBox.SelectAll();
            textBox.SelectionBackColor = Color.White;
        }

        private void buttonSaveResult_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxResult.Text))
                return;

            saveFileDialog2.Title = "Save result as...";
            saveFileDialog2.Filter = "JSON files|*.json|CSV files|*.csv|All files|*.*";

            saveFileDialog2.OverwritePrompt = true;

            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog2.FilterIndex == 2) // CSV format
                {

                    var csvString = CsvHelper.FromJson(textBoxResult.Text);
                    if (string.IsNullOrEmpty(csvString))
                    {
                        MessageBox.Show("Converting to CSV failed", "Error", MessageBoxButtons.OK);
                        return;
                    }
                    File.WriteAllText(saveFileDialog2.FileName, csvString);
                }
                else
                {
                    File.WriteAllText(saveFileDialog2.FileName, textBoxResult.Text);
                }
            }
        }

        private void textBoxResult_TextChanged(object sender, EventArgs e)
        {
            buttonSaveResult.Enabled = !string.IsNullOrEmpty(textBoxResult.Text);
        }

        private void viewTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer2.Panel1Collapsed = !viewTemplateToolStripMenuItem.Checked;
        }

        private void viewOutputDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer2.Panel2Collapsed = !viewOutputDataToolStripMenuItem.Checked;
        }

        private void textBoxTemplate_KeyPress(object sender, KeyPressEventArgs e)
        {
            TemplateChanged = true;
            tabPageTemplateText.Text = "Template *";
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new AboutBox();
            form.ShowDialog();
        }

        private void loadTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var templateText = textBoxTemplate.Text;

            //var rootElement = comboBox2.Text;

            //var results = new List<string>();
            //Stopwatch stopwatch0 = Stopwatch.StartNew();
            //Stopwatch stopwatch = Stopwatch.StartNew();

            //var template = _transformationService.ParseTemplate(null, templateText, !checkBoxCSharpNaming.Checked, out var errorMessage);

            //int count = 10000;
            ////toolStripProgressBar1.Maximum = (int)(count / 100);
            ////toolStripProgressBar1.Visible = true;

            ////toolStripProgressBar1.Value = 0;
            //int[] items = new int[count];

            //#region case1
            //int i = 0;
            //foreach (var item in items)
            //{
            //    var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
            //    i++;
            //    if (i % 100 == 0)
            //    {
            //        //toolStripProgressBar1.Value++;
            //    }

            //}
            //stopwatch.Stop();
            //results.Add($"forech: {stopwatch.Elapsed.TotalSeconds} secs");
            //#endregion

            //#region case2
            //stopwatch.Restart();
            ////toolStripProgressBar1.Value = 0;
            //items.AsParallel().ForAll(item =>
            //{
            //    var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
            //}
            //);

            //stopwatch.Stop();
            //results.Add($"AsParallel().ForAll: {stopwatch.Elapsed.TotalSeconds} secs");
            //#endregion

            //#region case3
            //stopwatch.Restart();
            //Parallel.ForEach(items, item =>
            //{
            //    var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
            //}
            //);

            //stopwatch.Stop();
            //results.Add($"Parallel.ForEach: {stopwatch.Elapsed.TotalSeconds} secs");

            //stopwatch.Restart();
            //Parallel.ForEach(items, item =>
            //{
            //    var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
            //}
            //);

            //stopwatch.Stop();
            //results.Add($"Parallel.ForEach: {stopwatch.Elapsed.TotalSeconds} secs");
            //#endregion

            //#region case4
            //stopwatch.Restart();
            //var taskList = new List<Task>();
            //foreach (var item in items)
            //{
            //    var itemTodo = item;
            //    taskList.Add(Task.Run(() => _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage)));
            //}
            //Task.WaitAll(taskList.ToArray());

            //stopwatch.Stop();
            //results.Add($"Task.WaitAll: {stopwatch.Elapsed.TotalSeconds} secs");
            //#endregion

            //stopwatch0.Stop();
            //results.Add($"total: {stopwatch0.Elapsed.TotalSeconds} secs");

            ////toolStripProgressBar1.Visible = false;
            //toolStripStatusLabel1.Text = $"Actual time for {count} iterations: " + 
            //    String.Join(";", results);
        }

        private void refreshToolStripButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TemplateFileName))
            {
                RefreshTemplate();
            }
        }

        private void dataTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item.ToString(), DragDropEffects.Copy);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            string selectedNodePath = GetDataTreeViewSelectedNodePath();

            Clipboard.SetText(selectedNodePath);
        }

        private string GetDataTreeViewSelectedNodePath(int level = 0)
        {
            string selectedNodePath = GetTreeViewNodeFullPath(dataTreeView.SelectedNode);

            if (!string.IsNullOrEmpty(comboBoxRootElement.Text))
            {
                selectedNodePath = selectedNodePath.Replace("ROOT", comboBoxRootElement.Text);
            }

            return selectedNodePath;
        }

        private string GetTreeViewNodeFullPath(TreeNode treeNode)
        {
            string selectedNodePath = treeNode.FullPath;

            selectedNodePath = selectedNodePath
                .Replace("\\", ".")
                .Replace(" <Object>", "")
                .Replace(" <Array>", "")
                .Split([':']).First()?.Trim();

            return selectedNodePath;
        }

        private string GetDataTreeViewSelectedNodeValue()
        {
            return dataTreeView.SelectedNode.Text
                .Split([':']).LastOrDefault();
        }

        private void dataTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                dataTreeView.SelectedNode = e.Node;
                contextMenuStrip1.Show(Cursor.Position);
            }

        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            int i;
            string s;

            var textBox = textBoxTemplate;

            try
            {
                // MANDATORY - focuses a label before highlighting (avoids blinking)
                labelTitle.Focus();

                // Get start position to drop the text.  
                i = textBox.SelectionStart;
                s = textBox.Text.Substring(i);

                // Drop the text on to the RichTextBox.  
                var text = textBox.Text.Substring(0, i);
                text = text +
                   Clipboard.GetText() + s;
                textBox.Text = text;
            }
            finally
            {
                // giving back the focus
                textBox.Focus();
            }
        }

        private void toolStripMenuItem5_CheckedChanged(object sender, EventArgs e)
        {
            disableHighlighting = !toolStripMenuItem5.Checked;

            // MANDATORY - focuses a label before highlighting (avoids blinking)
            labelTitle.Focus();

            if (_syntaxHighlighter != null)
            {
                _syntaxHighlighter.DisableHighlighting = disableHighlighting;
                _syntaxHighlighter.HighlightSyntax();
            }
            else
            {
                var text = textBoxTemplate.Text;
                textBoxTemplate.ResetText();
                textBoxTemplate.Text = text;
            }

            // giving back the focus
            textBoxTemplate.Focus();


        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            string selectedNodePath = GetDataTreeViewSelectedNodePath();

            Clipboard.SetText("{{" + selectedNodePath + "}}");

        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            string selectedValue = GetDataTreeViewSelectedNodeValue();

            Clipboard.SetText(selectedValue);

        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            string value = @"{% if %}
{% else %}
{% endif %}";

            Clipboard.SetText(value);
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            string value = @"[
{% for item in items %}
{
}{% if forloop.last == false %},{% endif %}
{% endfor %}
]";

            Clipboard.SetText(value);

        }

        private void refreshTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshDataTree(contentValue);
        }
    }
}
