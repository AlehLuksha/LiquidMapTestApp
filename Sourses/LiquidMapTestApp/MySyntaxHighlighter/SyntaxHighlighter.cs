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

        private static Regex _keywordsRegex;
        private static Regex _filtersRegexRubyNaming;
        private static Regex _filtersRegexCSharpUpperNaming;
        private static Regex _filtersRegexCSharpLowerNaming;
        private static Regex _customFiltersRegexRubyNaming;
        private static Regex _customFiltersRegexCSharpUpperNaming;
        private static Regex _customFiltersRegexCSharpLowerNaming;
        private static Regex _typesRegex;
        private static Regex _commentsRegex;
        private static Regex _stringsRegEx;
        private static Regex _templatesRegEx;

        private Regex _activeFiltersRegex;
        private Regex _activeCustomFiltersRegex;

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
            InitializeRegexPatterns();

            _activeFiltersRegex = isCSharpNamingConvention
                ? (isFirstLetterUpper ? _filtersRegexCSharpUpperNaming : _filtersRegexCSharpLowerNaming)
                : _filtersRegexRubyNaming;

            _activeCustomFiltersRegex = isCSharpNamingConvention
                ? (isFirstLetterUpper ? _customFiltersRegexCSharpUpperNaming : _customFiltersRegexCSharpLowerNaming)
                : _customFiltersRegexRubyNaming;
        }

        public void HighlightSyntax()
        {
            if (_activeFiltersRegex == null || _activeCustomFiltersRegex == null)
            {
                InitSyntax(false, true);
            }

            string text = _richTextBox.Text;

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
                MatchCollection keywordMatches = _keywordsRegex.Matches(text);

                // getting filters
                MatchCollection filterMatches = _activeFiltersRegex.Matches(text);

                // getting custom filters
                MatchCollection customFilterMatches = _activeCustomFiltersRegex.Matches(text);

                // getting types/classes from the text 
                MatchCollection typeMatches = _typesRegex.Matches(text);

                // getting comments (multiline)
                MatchCollection commentMatches = _commentsRegex.Matches(text);

                // getting strings
                MatchCollection stringMatches = _stringsRegEx.Matches(text);

                // getting templates
                MatchCollection templatesMatches = _templatesRegEx.Matches(text);

                // scanning...
                foreach (Match m in keywordMatches)
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.DarkBlue;
                    _richTextBox.SelectionFont = _boldFont;
                }

                foreach (Match m in typeMatches)
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.DarkCyan;
                }

                foreach (Match m in commentMatches)
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Green;
                    _richTextBox.SelectionFont = _italicFont;
                }

                foreach (Match m in stringMatches)
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Brown;
                }

                foreach (Match m in filterMatches)
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Blue;
                    _richTextBox.SelectionFont = _boldFont;
                }

                foreach (Match m in customFilterMatches)
                {
                    _richTextBox.SelectionStart = m.Index;
                    _richTextBox.SelectionLength = m.Length;
                    _richTextBox.SelectionColor = Color.Red;
                    _richTextBox.SelectionFont = _boldFont;
                }

                foreach (Match m in templatesMatches)
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

        private static void InitializeRegexPatterns()
        {
            if (_keywordsRegex != null)
                return;

            _keywordsRegex = new Regex($@"\b({string.Join("|", LiquidSyntax.Keywords)})\b", RegexOptions.Compiled);

            _filtersRegexRubyNaming = new Regex($@"\b({string.Join("|", LiquidSyntax.Filters)})\b", RegexOptions.Compiled);
            _filtersRegexCSharpUpperNaming = new Regex(
                $@"\b({string.Join("|", LiquidSyntax.Filters.Select(x => LiquidSyntax.ConvertToCSharpName(x, true)))})\b",
                RegexOptions.Compiled);
            _filtersRegexCSharpLowerNaming = new Regex(
                $@"\b({string.Join("|", LiquidSyntax.Filters.Select(x => LiquidSyntax.ConvertToCSharpName(x, false)))})\b",
                RegexOptions.Compiled);

            _customFiltersRegexRubyNaming = new Regex($@"\b({string.Join("|", LiquidSyntax.CustomFilters)})\b", RegexOptions.Compiled);
            _customFiltersRegexCSharpUpperNaming = new Regex(
                $@"\b({string.Join("|", LiquidSyntax.CustomFilters.Select(x => LiquidSyntax.ConvertToCSharpName(x, true)))})\b",
                RegexOptions.Compiled);
            _customFiltersRegexCSharpLowerNaming = new Regex(
                $@"\b({string.Join("|", LiquidSyntax.CustomFilters.Select(x => LiquidSyntax.ConvertToCSharpName(x, false)))})\b",
                RegexOptions.Compiled);

            _typesRegex = new Regex(@"\b(Console)\b", RegexOptions.Compiled);
            _commentsRegex = new Regex(@"{%-?\s*comment\s*-?%}[\s\S]*?{%-?\s*endcomment\s*-?%}", RegexOptions.Compiled);
            _stringsRegEx = new Regex("(\".+?\"|'.+?')", RegexOptions.Compiled);
            _templatesRegEx = new Regex(@"{{.+?}}", RegexOptions.Compiled);
        }
    }
}
