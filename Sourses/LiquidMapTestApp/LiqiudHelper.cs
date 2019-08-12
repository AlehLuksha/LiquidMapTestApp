using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LiquidMapTestApp
{
    public class LiqiudHelper
    {
        public static void HighlightLiquidSyntax(RichTextBox codeRichTextBox, bool useAzureLiquidSyntax)
        {
            // getting keywords/functions
            string keywords = $@"\b({string.Join("|", LiquidSyntax.Keywords)})\b";
            MatchCollection keywordMatches = Regex.Matches(codeRichTextBox.Text, keywords);

            // getting filters
            string filters = $@"\b({string.Join("|", useAzureLiquidSyntax? AzureLiquidSyntax.Filters: LiquidSyntax.Filters)})\b";
            MatchCollection filterMatches = Regex.Matches(codeRichTextBox.Text, filters);

            // getting types/classes from the text 
            string types = @"\b(Console)\b";
            MatchCollection typeMatches = Regex.Matches(codeRichTextBox.Text, types);

            // getting comments (inline or multiline)
            string comments = @"(\/\/.+?$|\/\*.+?\*\/)";
            MatchCollection commentMatches = Regex.Matches(codeRichTextBox.Text, comments, RegexOptions.Multiline);

            // getting strings
            string strings = "\".+?\"";
            MatchCollection stringMatches = Regex.Matches(codeRichTextBox.Text, strings);

            // saving the original caret position + forecolor
            int originalIndex = codeRichTextBox.SelectionStart;
            int originalLength = codeRichTextBox.SelectionLength;
            Color originalColor = Color.Black;

            // removes any previous highlighting (so modified words won't remain highlighted)
            codeRichTextBox.SelectionStart = 0;
            codeRichTextBox.SelectionLength = codeRichTextBox.Text.Length;
            codeRichTextBox.SelectionColor = originalColor;

            var keywordFont = new Font( codeRichTextBox.Font.Name, codeRichTextBox.Font.Size, FontStyle.Bold);
            // scanning...
            foreach (Match m in keywordMatches)
            {
                codeRichTextBox.SelectionStart = m.Index;
                codeRichTextBox.SelectionLength = m.Length;
                codeRichTextBox.SelectionColor = Color.DarkBlue;
                codeRichTextBox.SelectionFont = keywordFont;
            }

            foreach (Match m in typeMatches)
            {
                codeRichTextBox.SelectionStart = m.Index;
                codeRichTextBox.SelectionLength = m.Length;
                codeRichTextBox.SelectionColor = Color.DarkCyan;
            }

            foreach (Match m in commentMatches)
            {
                codeRichTextBox.SelectionStart = m.Index;
                codeRichTextBox.SelectionLength = m.Length;
                codeRichTextBox.SelectionColor = Color.Green;
            }

            foreach (Match m in stringMatches)
            {
                codeRichTextBox.SelectionStart = m.Index;
                codeRichTextBox.SelectionLength = m.Length;
                codeRichTextBox.SelectionColor = Color.Brown;
            }

            foreach (Match m in filterMatches)
            {
                codeRichTextBox.SelectionStart = m.Index;
                codeRichTextBox.SelectionLength = m.Length;
                codeRichTextBox.SelectionColor = Color.Blue;
                codeRichTextBox.SelectionFont = keywordFont;
            }

            // restoring the original colors, for further writing
            codeRichTextBox.SelectionStart = originalIndex;
            codeRichTextBox.SelectionLength = originalLength;
            codeRichTextBox.SelectionColor = originalColor;

        }
    }
}
