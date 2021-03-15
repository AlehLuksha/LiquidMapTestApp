using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotLiquid;
using DotLiquid.NamingConventions;
using LiquidMapTestApp.Helpers;
using Newtonsoft.Json;

namespace LiquidMapTestApp
{
    public partial class MainForm : Form
    {
        private const string FormHeader = "Liquid Map Tester";
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

        public MainForm()
        {
            InitializeComponent();
            this.Text = FormHeader;

            textBoxData.Font = EditorDefaultFont;
            textBoxTemplate.Font = EditorDefaultFont;
            textBoxResult.Font = EditorDefaultFont;

            LiqiudHelper.Init(textBoxTemplate);
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
                text = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(text), Formatting.Indented);
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

                string fileContent = File.ReadAllText(openFileDialog1.FileName, Encoding.UTF8);

                // load to TextBox
                DisplayTemplate(fileContent);

                executeToolStripMenuItem.Enabled = fileContent.Length > 0;
                toolStripButtonExecute.Enabled = executeToolStripMenuItem.Enabled;

                saveToolStripMenuItem.Enabled = false;
                saveToolStripButton.Enabled = saveToolStripMenuItem.Enabled;

            }

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

            LiqiudHelper.HighlightLiquidSyntax(textBoxTemplate, checkBoxUseAzureSyntax.Checked);
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

                var transformer = new Transformer(templateText, !checkBoxUseAzureSyntax.Checked);

                var rootElement = comboBox2.Text;

                var result3 = transformer.RenderFromString(contentValue, rootElement);

                if (toolStripButtonShowErrors.Checked && transformer.LiquidTemplate.Errors.Any())
                {
                    var message = "";
                    foreach (var error in transformer.LiquidTemplate.Errors)
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
            checkBoxUseAzureSyntax.Checked = true;
            tabControlResult.TabPages.Remove(tabPageResultJson);
            tabControlResult.TabPages.Remove(tabPageResultHTML);
            comboBox1.SelectedIndex = (int)OutputFormat.Json;
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            outputFormat = (OutputFormat) comboBox1.SelectedIndex;

            checkBoxAutoFormatJsonResult.Visible = outputFormat == OutputFormat.Json;
            buttonFormatJsonResult.Visible = outputFormat == OutputFormat.Json;
            if (outputFormat == OutputFormat.Json)
            {
                tabControlResult.TabPages.Add(tabPageResultJson);
            }
            else
            {
                tabControlResult.TabPages.Remove(tabPageResultJson);
            }

            if (outputFormat == OutputFormat.Html)
            {
                tabControlResult.TabPages.Add(tabPageResultHTML);
            }
            else
            {
                tabControlResult.TabPages.Remove(tabPageResultHTML);
            }
            webBrowser1.DocumentText = "";
        }

        private void checkBoxUseAzureSyntax_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseAzureSyntax.Checked)
            {
                comboBox2.SelectedIndex = 0;
            }
            richTextBox1_TextChanged(checkBoxUseAzureSyntax, new EventArgs());
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
                tabPage5.Text = "Template";
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
            tabPage5.Text = "Template *";
        }

    }
}
