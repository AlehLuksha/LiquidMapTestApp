using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LiquidMapTestApp.MySyntaxHighlighter
{
    public class SyntaxHighlighter
    {
        private Font _defaultFont;
        private Font _boldFont;
        private Font _italicFont;
        private Font _boldItalicFont;

        private RichTextBox _richTextBox;

        private string _keywordsRegex;
        private string _filtersRegex;
        private string _customFiltersRegex;
        private string _typesRegex;
        private string _commentsRegex;
        private string _stringsRegEx;
        private string _templatesRegEx;

        /// <summary>
        /// Determines whether the program is busy creating rtf for the previous
        /// modification of the text-box. It is necessary to avoid blinks when the 
        /// user is typing fast.
        /// </summary>
        /// 
        private bool _isDuringHighlight;

        public bool IsDuringHighlight { get { return _isDuringHighlight; } }

        public bool DisableHighlighting { get; set; }

        public SyntaxHighlighter(RichTextBox richTextBox)
        {
            if (richTextBox == null)
                throw new ArgumentNullException("richTextBox");

            _richTextBox = richTextBox;
            _defaultFont = new Font(richTextBox.Font.Name, richTextBox.Font.Size);
            _boldFont = new Font(richTextBox.Font.Name, richTextBox.Font.Size, FontStyle.Bold);
            _italicFont = new Font(richTextBox.Font.Name, richTextBox.Font.Size, FontStyle.Italic);
            _boldItalicFont = new Font(richTextBox.Font.Name, richTextBox.Font.Size, FontStyle.Bold | FontStyle.Italic);

            DisableHighlighting = false;

            //_richTextBox.TextChanged += RichTextBox_TextChanged;
        }

        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!DisableHighlighting)
            {
                if (_isDuringHighlight)
                    return;

                HighlightSyntax();
            }
        }

        public void InitSyntax(bool isCSharpNamingConvention, bool isFirstLetterUpper)
        {
            _keywordsRegex = $@"\b({string.Join("|", LiquidSyntax.Keywords)})\b";

            _filtersRegex = $@"\b({string.Join("|", isCSharpNamingConvention
                ? LiquidSyntax.Filters.ToList().Select(x => LiquidSyntax.ConvertToCSharpName(x,isFirstLetterUpper))
                : LiquidSyntax.Filters)})\b";

            _customFiltersRegex = $@"\b({string.Join("|", isCSharpNamingConvention
                ? LiquidSyntax.CustomFilters.ToList().Select(x => LiquidSyntax.ConvertToCSharpName(x, isFirstLetterUpper))
                : LiquidSyntax.CustomFilters)})\b";

            _typesRegex = @"\b(Console)\b";

            //_commentsRegex = @"{% comment %}(.|[\r\n])*?{% endcomment %}";
            _commentsRegex = @"{%-?\s*comment\s*-?%}[\s\S]*?{%-?\s*endcomment\s*-?%}";

            _stringsRegEx = "(\".+?\"|'.+?')";

            _templatesRegEx = @"{{.+?}}";

        }

        public void HighlightSyntax()
        {
            // saving the original caret position + forecolor
            int originalIndex = _richTextBox.SelectionStart;
            int originalLength = _richTextBox.SelectionLength;
            Color originalColor = Color.Black;

            try
            {
                _isDuringHighlight = true;


                // removes any previous highlighting (so modified words won't remain highlighted)
                _richTextBox.SelectionStart = 0;
                _richTextBox.SelectionLength = _richTextBox.Text.Length;
                _richTextBox.SelectionColor = originalColor;
                _richTextBox.SelectionFont = _defaultFont;

                if (DisableHighlighting)
                {
                    return;
                }

                // getting keywords/functions
                MatchCollection keywordMatches = Regex.Matches(_richTextBox.Text, _keywordsRegex);

                // getting filters
                MatchCollection filterMatches = Regex.Matches(_richTextBox.Text, _filtersRegex);

                // getting custom filters
                MatchCollection customFilterMatches = Regex.Matches(_richTextBox.Text, _customFiltersRegex);

                // getting types/classes from the text 
                MatchCollection typeMatches = Regex.Matches(_richTextBox.Text, _typesRegex);

                // getting comments (multiline)
                MatchCollection commentMatches = Regex.Matches(_richTextBox.Text, _commentsRegex, RegexOptions.Multiline);

                // getting strings
                MatchCollection stringMatches = Regex.Matches(_richTextBox.Text, _stringsRegEx);

                // getting templates
                MatchCollection templatesMatches = Regex.Matches(_richTextBox.Text, _templatesRegEx);

                // scanning...
                foreach (Match m in keywordMatches?.OrderBy(x=>x.Index))
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.DarkBlue;
                    _richTextBox.SelectionFont = _boldFont;
                }

                foreach (Match m in typeMatches?.OrderBy(x => x.Index))
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.DarkCyan;
                }

                foreach (Match m in commentMatches?.OrderBy(x => x.Index))
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Green;
                    _richTextBox.SelectionFont = _italicFont;
                }

                foreach (Match m in stringMatches?.OrderBy(x => x.Index))
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Brown;
                }

                foreach (Match m in filterMatches?.OrderBy(x => x.Index))
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Blue;
                    _richTextBox.SelectionFont = _boldFont;
                }

                foreach (Match m in customFilterMatches?.OrderBy(x => x.Index))
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Red;
                    _richTextBox.SelectionFont = _boldFont;
                }

                foreach (Match m in templatesMatches?.OrderBy(x => x.Index))
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.DarkGray;
                    _richTextBox.SelectionFont = _boldItalicFont;
                }

            }
            finally
            {
                // restoring the original colors, for further writing
                _richTextBox.SelectionStart = originalIndex;
                _richTextBox.SelectionLength = originalLength;
                _richTextBox.SelectionColor = originalColor;

                _isDuringHighlight = false;
            }
        }
    }
}
