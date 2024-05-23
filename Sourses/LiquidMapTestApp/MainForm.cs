using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiquidMapTestApp.Helpers;
using DotLiquidProcessor;
using LiquidProcessor.Core.Interfaces;
using Newtonsoft.Json;
using DotLiquid;

namespace LiquidMapTestApp
{
    public partial class MainForm : Form
    {
        private const string FormHeader = "Liquid Template Tester";
        private static string FontName = "Consolas";
        private static int FontSize = 9;
        private readonly Font EditorDefaultFont = new Font(FontName, FontSize);

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
        private String contentValue = "{}";
        private OutputFormat outputFormat = OutputFormat.PlainText;


        private String ContentString(string rootElement)
        {
                return "{" + rootElement + ":" + contentValue + "}";
        }

        private bool TemplateChanged { get; set; }

        private ITransformationService<Template> _transformationService;

        public MainForm()
        {
            InitializeComponent();
            this.Text = FormHeader;

            textBoxData.Font = EditorDefaultFont;
            textBoxTemplate.Font = EditorDefaultFont;
            textBoxResult.Font = EditorDefaultFont;

            LiqiudHelper.Init(textBoxTemplate);

            _transformationService = new DotLiquidProcessor.TransformationService();
            //_transformationService = new FluidProcessor.TransformationService();
        }

        private void DisplayText(RichTextBox textBox, string text)
        {
            textBox.Clear();
            textBox.Text = text;
        }

        private void DisplayData(string json)
        {
            // load to TextBox
            DisplayText(textBoxData, json);

            try
            {
                // load to TreeView
                dynamic data = JsonConvert.DeserializeObject(json);
                ObjectToTreeView.SetObjectAsJson(treeView1, data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DisplayText(textBoxResult, "Input data errors: \n\n" + e.Message);
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
                    , new JsonSerializerSettings(){FloatParseHandling = FloatParseHandling.Decimal}
                    );
                text = JsonConvert.SerializeObject(outputData, Formatting.Indented);
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
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var fileContent = File.ReadAllText(openFileDialog1.FileName);

                contentValue = fileContent;

                DisplayData(contentValue);
            }
        }

        private void openTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open Map File";
            openFileDialog1.Filter = "Liquid map|*.liquid|JSON files|*.json|All files|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TemplateFileName = openFileDialog1.FileName;

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

            var resultString = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);

            DisplayResult(resultString);

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            executeToolStripMenuItem.Enabled = textBoxTemplate.TextLength > 0;
            toolStripButtonExecute.Enabled = executeToolStripMenuItem.Enabled;

            saveToolStripMenuItem.Enabled = textBoxTemplate.TextLength > 0;
            saveToolStripButton.Enabled = saveToolStripMenuItem.Enabled;

            // MANDATORY - focuses a label before highlighting (avoids blinking)
            labelTitle.Focus();

            LiqiudHelper.HighlightLiquidSyntax(textBoxTemplate, checkBoxCSharpNaming.Checked);
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

                var rootElement = comboBox2.Text;

                var t1 = DateTime.Now;

                var template = _transformationService.ParseTemplate(null, templateText, !checkBoxCSharpNaming.Checked, out var errorMessage);
                var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);

                var t2 = DateTime.Now;

                var delta = (t2 - t1).TotalMilliseconds;
                var estimatedSeconds = Math.Round(delta / 1000 * 10000, 5);
                var estimatedMinutes = Math.Round(estimatedSeconds / 60, 2);
                toolStripStatusLabel1.Text = $"Estimated time for 10000 iterations: {estimatedSeconds} secs, {estimatedMinutes} mins ";

                if (toolStripButtonShowErrors.Checked && template.Errors.Any())
                {
                    var message = "";
                    foreach (var error in template.Errors)
                    {
                        message += $"Message: {error.Message}\nInnerException: {error.InnerException?.Message}\n\n";
                    }

                    DisplayText(textBoxResult, "Transformer errors: \n\n" + message);
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

        private void Form1_Load(object sender, EventArgs e)
        {
            checkBoxCSharpNaming.Checked = true;
            tabControlResult.TabPages.Remove(tabPageResultJson);
            tabControlResult.TabPages.Remove(tabPageResultHTML);
            cmbResultType.SelectedIndex = (int)OutputFormat.Json;
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            outputFormat = (OutputFormat) cmbResultType.SelectedIndex;

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
            if (checkBoxCSharpNaming.Checked)
            {
                comboBox2.SelectedIndex = 0;
            }
            richTextBox1_TextChanged(checkBoxCSharpNaming, new EventArgs());
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
            var templateText = textBoxTemplate.Text;

            var rootElement = comboBox2.Text;

            var results = new List<string>();
            Stopwatch stopwatch0 = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();

            var template = _transformationService.ParseTemplate(null, templateText, !checkBoxCSharpNaming.Checked, out var errorMessage);

            int count = 10000;
            //toolStripProgressBar1.Maximum = (int)(count / 100);
            //toolStripProgressBar1.Visible = true;

            //toolStripProgressBar1.Value = 0;
            int[] items = new int[count];

            #region case1
            int i = 0;
            foreach (var item in items)
            {
                var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
                i++;
                if (i % 100 == 0)
                {
                    //toolStripProgressBar1.Value++;
                }

            }
            stopwatch.Stop();
            results.Add($"forech: {stopwatch.Elapsed.TotalSeconds} secs");
            #endregion

            #region case2
            stopwatch.Restart();
            //toolStripProgressBar1.Value = 0;
            items.AsParallel().ForAll(item =>
            {
                var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
            }
            );

            stopwatch.Stop();
            results.Add($"AsParallel().ForAll: {stopwatch.Elapsed.TotalSeconds} secs");
            #endregion

            #region case3
            stopwatch.Restart();
            Parallel.ForEach(items, item =>
            {
                var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
            }
            );

            stopwatch.Stop();
            results.Add($"Parallel.ForEach: {stopwatch.Elapsed.TotalSeconds} secs");

            stopwatch.Restart();
            Parallel.ForEach(items, item =>
            {
                var result3 = _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage);
            }
            );

            stopwatch.Stop();
            results.Add($"Parallel.ForEach: {stopwatch.Elapsed.TotalSeconds} secs");
            #endregion

            #region case4
            stopwatch.Restart();
            var taskList = new List<Task>();
            foreach (var item in items)
            {
                var itemTodo = item;
                taskList.Add(Task.Run(() => _transformationService.TransformJsonToText(null, template, contentValue, rootElement, out errorMessage)));
            }
            Task.WaitAll(taskList.ToArray());

            stopwatch.Stop();
            results.Add($"Task.WaitAll: {stopwatch.Elapsed.TotalSeconds} secs");
            #endregion

            stopwatch0.Stop();
            results.Add($"total: {stopwatch0.Elapsed.TotalSeconds} secs");

            //toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Text = $"Actual time for {count} iterations: " + 
                String.Join(";", results);
        }

        private void refreshToolStripButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TemplateFileName))
            {
                RefreshTemplate();
            }
        }
    }
}
