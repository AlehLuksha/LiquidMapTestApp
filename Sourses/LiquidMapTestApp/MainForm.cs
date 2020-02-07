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
using Newtonsoft.Json;

namespace LiquidMapTestApp
{
    public partial class MainForm : Form
    {
        private const string FormHeader = "Liquid Map Tester";
        private static string FontName = "Consolas";
        private static int FontSize = 9;
        private Font EditorDefaultFont = new Font(FontName, FontSize);

        private string templateFileName = "";
        private String contentValue = "{}";
        private OutputFormat outputFormat = OutputFormat.PlainText;

        private String ContentString(string rootElement)
        {
                return "{" + rootElement + ":" + contentValue + "}";
        }

        public MainForm()
        {
            InitializeComponent();
            this.Text = FormHeader;

            textBoxData.Font = EditorDefaultFont;
            codeRichTextBox.Font = EditorDefaultFont;
            textBoxResult.Font = EditorDefaultFont;
        }

        private void DisplayText(TextBox textBox, string text)
        {
            textBox.Clear();
            textBox.Text = text;
        }

        private void DisplayData(string json)
        {
            // load to TextBox
            DisplayText(textBoxData, json);

            // load to TreeView
            dynamic data = JsonConvert.DeserializeObject(json);
            ObjectToTreeView.SetObjectAsJson(treeView1, data);
        }

        private void DisplayTemplate(string text)
        {
            codeRichTextBox.Clear();
            codeRichTextBox.Text = text;
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
                this.Text = $"{FormHeader} ({openFileDialog1.SafeFileName})";
                templateFileName = openFileDialog1.FileName;

                string fileContent = File.ReadAllText(openFileDialog1.FileName);

                // load to TextBox
                DisplayTemplate(fileContent);

                executeToolStripMenuItem.Enabled = fileContent.Length > 0;
                toolStripButtonExecute.Enabled = executeToolStripMenuItem.Enabled;
            }

        }

        private void sourseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //sourseToolStripMenuItem.Checked = !sourseToolStripMenuItem.Checked;
            //splitContainer1.Panel1.Visible = sourseToolStripMenuItem.Checked;
            splitContainer1.Panel1Collapsed = !sourseToolStripMenuItem.Checked;
        }

        private void buttonFormatJson_Click(object sender, EventArgs e)
        {
            var json = textBoxResult.Text;

            var resultString = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);

            DisplayResult(resultString);

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            executeToolStripMenuItem.Enabled = codeRichTextBox.TextLength > 0;
            toolStripButtonExecute.Enabled = executeToolStripMenuItem.Enabled;

            saveToolStripMenuItem.Enabled = codeRichTextBox.TextLength > 0;
            saveToolStripButton.Enabled = saveToolStripMenuItem.Enabled || templateFileName.Length > 0;

            // MANDATORY - focuses a label before highlighting (avoids blinking)
            labelTitle.Focus();

            LiqiudHelper.HighlightLiquidSyntax(codeRichTextBox, checkBoxUseAzureSyntax.Checked);
            // giving back the focus
            codeRichTextBox.Focus();
        }

        private void textBoxData_TextChanged(object sender, EventArgs e)
        {
            contentValue = textBoxData.Text;
        }

        private void executeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var templateText = codeRichTextBox.Text;

                var transformer = new Transformer(templateText, !checkBoxUseAzureSyntax.Checked);

                #region No working
                //object data = JsonConvert.DeserializeObject(ContentString);
                //Hash renderData0 = Hash.FromAnonymousObject((new { content = data }), false);
                //var result0 = template.Render(renderData0);

                ////var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(ContentString);
                //var json = JsonConvert.DeserializeObject<IDictionary<string, object>>(ContentString, new DictionaryConverter());

                //Hash renderData2 = Hash.FromAnonymousObject((new { content = json }), false);
                //var result2 = template.Render(renderData2);
                #endregion

                var dataText = ContentString(comboBox2.Text);

                var json = JsonConvert.DeserializeObject<IDictionary<string, object>>(dataText, new DictionaryConverter());
                Hash renderData3 = Hash.FromDictionary(json);
                var result3 = transformer.LiquidTemplate.Render(renderData3);

                DisplayResult(result3);

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
            if (!string.IsNullOrEmpty(templateFileName))
            {
                saveFileDialog1.FileName = templateFileName;
            }
            saveFileDialog1.OverwritePrompt = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(templateFileName, codeRichTextBox.Text);
            }
        }
    }
}
