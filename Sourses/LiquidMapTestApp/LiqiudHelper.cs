using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LiquidMapTestApp
{
    public class LiqiudHelper
    {
        private static Font _keywordFont;
        private static Font _commentsFont;

        public static void Init(RichTextBox codeRichTextBox)
        {
            _keywordFont = new Font(codeRichTextBox.Font.Name, codeRichTextBox.Font.Size, FontStyle.Bold);
            _commentsFont = new Font(codeRichTextBox.Font.Name, codeRichTextBox.Font.Size, FontStyle.Italic);
        }

        public static void HighlightLiquidSyntax(RichTextBox codeRichTextBox, bool isCSharpNamingConvention)
        {
            // getting keywords/functions
            string keywords = $@"\b({string.Join("|", LiquidSyntax.Keywords)})\b";
            MatchCollection keywordMatches = Regex.Matches(codeRichTextBox.Text, keywords);

            // getting filters
            string filters = $@"\b({string.Join("|", isCSharpNamingConvention 
                ? LiquidSyntax.Filters.ToList().Select(x => ConvertToCSharpName(x)) 
                : LiquidSyntax.Filters)})\b";
            MatchCollection filterMatches = Regex.Matches(codeRichTextBox.Text, filters);

            // getting custom filters
            string customFilters = $@"\b({string.Join("|", isCSharpNamingConvention
                ? LiquidSyntax.CustomFilters.ToList().Select(x => ConvertToCSharpName(x))
                : LiquidSyntax.CustomFilters)})\b";
            MatchCollection customFilterMatches = Regex.Matches(codeRichTextBox.Text, customFilters);


            // getting types/classes from the text 
            string types = @"\b(Console)\b";
            MatchCollection typeMatches = Regex.Matches(codeRichTextBox.Text, types);

            // getting comments (multiline)
            //string comments = @"(\/\/.+?$|\/\*.+?\*\/)";
            string comments = @"{% comment %}(.|[\r\n])*?{% endcomment %}";
            MatchCollection commentMatches = Regex.Matches(codeRichTextBox.Text, comments, RegexOptions.Multiline);

            // getting strings
            string strings = "(\".+?\"|'.+?')";
            MatchCollection stringMatches = Regex.Matches(codeRichTextBox.Text, strings);

            // saving the original caret position + forecolor
            int originalIndex = codeRichTextBox.SelectionStart;
            int originalLength = codeRichTextBox.SelectionLength;
            Color originalColor = Color.Black;

            // removes any previous highlighting (so modified words won't remain highlighted)
            codeRichTextBox.SelectionStart = 0;
            codeRichTextBox.SelectionLength = codeRichTextBox.Text.Length;
            codeRichTextBox.SelectionColor = originalColor;

            // scanning...
            foreach (Match m in keywordMatches)
            {
                codeRichTextBox.SelectionStart = m.Index;
                codeRichTextBox.SelectionLength = m.Length;
                codeRichTextBox.SelectionColor = Color.DarkBlue;
                codeRichTextBox.SelectionFont = _keywordFont;
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
                codeRichTextBox.SelectionFont = _commentsFont;
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
                codeRichTextBox.SelectionFont = _keywordFont;
            }

            foreach (Match m in customFilterMatches)
            {
                codeRichTextBox.SelectionStart = m.Index;
                codeRichTextBox.SelectionLength = m.Length;
                codeRichTextBox.SelectionColor = Color.Red;
                codeRichTextBox.SelectionFont = _keywordFont;
            }
            // restoring the original colors, for further writing
            codeRichTextBox.SelectionStart = originalIndex;
            codeRichTextBox.SelectionLength = originalLength;
            codeRichTextBox.SelectionColor = originalColor;

        }

        private static string ConvertToCSharpName(string s)
        {
            var parts = s.Split(new char[] {'_'}).Select(x => UpperFirstLetter(x));
            return string.Join("", parts);
        }

        private static string UpperFirstLetter(string word)
        {
            return char.ToUpperInvariant(word[0]) + word.Substring(1);
        }

    }
}
