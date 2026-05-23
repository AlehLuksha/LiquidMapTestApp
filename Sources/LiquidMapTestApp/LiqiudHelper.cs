using System;
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

        // Cached compiled regex patterns to avoid recompilation on every highlight call
        private static Regex _keywordRegex;
        private static Regex _filterRegexRubyNaming;
        private static Regex _filterRegexCSharpNaming;
        private static Regex _customFilterRegexRubyNaming;
        private static Regex _customFilterRegexCSharpNaming;
        private static Regex _typeRegex;
        private static Regex _commentRegex;
        private static Regex _stringRegex;

        public static void Init(RichTextBox codeRichTextBox)
        {
            _keywordFont = new Font(codeRichTextBox.Font.Name, codeRichTextBox.Font.Size, FontStyle.Bold);
            _commentsFont = new Font(codeRichTextBox.Font.Name, codeRichTextBox.Font.Size, FontStyle.Italic);

            // Initialize cached regex patterns
            InitializeRegexPatterns();
        }

        private static void InitializeRegexPatterns()
        {
            if (_keywordRegex != null)
                return; // Already initialized

            // Keywords pattern - compile once
            string keywordsPattern = $@"\b({string.Join("|", LiquidSyntax.Keywords)})\b";
            _keywordRegex = new Regex(keywordsPattern, RegexOptions.Compiled);

            // Filters patterns - compile once for both naming conventions
            string filtersPatternRuby = $@"\b({string.Join("|", LiquidSyntax.Filters)})\b";
            _filterRegexRubyNaming = new Regex(filtersPatternRuby, RegexOptions.Compiled);

            string filtersPatternCSharp = $@"\b({string.Join("|", LiquidSyntax.Filters.Select(x => ConvertToCSharpName(x)))})\b";
            _filterRegexCSharpNaming = new Regex(filtersPatternCSharp, RegexOptions.Compiled);

            // Custom filters patterns - compile once for both naming conventions
            string customFiltersPatternRuby = $@"\b({string.Join("|", LiquidSyntax.CustomFilters)})\b";
            _customFilterRegexRubyNaming = new Regex(customFiltersPatternRuby, RegexOptions.Compiled);

            string customFiltersPatternCSharp = $@"\b({string.Join("|", LiquidSyntax.CustomFilters.Select(x => ConvertToCSharpName(x)))})\b";
            _customFilterRegexCSharpNaming = new Regex(customFiltersPatternCSharp, RegexOptions.Compiled);

            // Types pattern
            _typeRegex = new Regex(@"\b(Console)\b", RegexOptions.Compiled);

            // Comments pattern
            _commentRegex = new Regex(@"{% comment %}(.|[\r\n])*?{% endcomment %}", RegexOptions.Compiled | RegexOptions.Multiline);

            // Strings pattern
            _stringRegex = new Regex("(\".+?\"|'.+?')", RegexOptions.Compiled);
        }

        public static void HighlightLiquidSyntax(RichTextBox codeRichTextBox, bool isCSharpNamingConvention)
        {
            // Ensure regex patterns are initialized
            InitializeRegexPatterns();

            string text = codeRichTextBox.Text;

            // Use cached regex patterns instead of recompiling them
            MatchCollection keywordMatches = _keywordRegex.Matches(text);
            MatchCollection filterMatches = isCSharpNamingConvention
                ? _filterRegexCSharpNaming.Matches(text)
                : _filterRegexRubyNaming.Matches(text);

            MatchCollection customFilterMatches = isCSharpNamingConvention
                ? _customFilterRegexCSharpNaming.Matches(text)
                : _customFilterRegexRubyNaming.Matches(text);

            MatchCollection typeMatches = _typeRegex.Matches(text);
            MatchCollection commentMatches = _commentRegex.Matches(text);
            MatchCollection stringMatches = _stringRegex.Matches(text);

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
            var parts = s.Split(new char[] { '_' }).Select(x => UpperFirstLetter(x));
            return string.Join("", parts);
        }

        /// <summary>
        /// Converts the first character of a word to uppercase.
        /// Uses string.Concat with AsSpan to minimize allocations.
        /// </summary>
        private static string UpperFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            if (word.Length == 1)
                return word.ToUpperInvariant();

            // Optimal approach: use string.Concat with AsSpan to avoid Substring allocation
            return string.Concat(char.ToUpperInvariant(word[0]), word.AsSpan(1));
        }
    }
}
