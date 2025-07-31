using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace synapse.Utils
{
    public static class CodeDetector
    {
        // Common programming language keywords across multiple languages
        private static readonly HashSet<string> CommonKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // C-style languages (C, C++, C#, Java, JavaScript, etc.)
            "public", "private", "protected", "static", "void", "int", "string", "bool", "boolean",
            "class", "interface", "struct", "enum", "namespace", "using", "import", "package",
            "if", "else", "switch", "case", "for", "foreach", "while", "do", "break", "continue",
            "return", "throw", "try", "catch", "finally", "new", "this", "base", "super",
            "const", "let", "var", "function", "async", "await", "null", "undefined",
            
            // Python
            "def", "elif", "except", "lambda", "pass", "raise", "yield", "from", "as",
            "True", "False", "None", "and", "or", "not", "in", "is",
            
            // Other common keywords
            "select", "from", "where", "insert", "update", "delete", "create", "alter", "drop",
            "begin", "end", "then", "when", "declare", "set", "get"
        };

        // Patterns that strongly indicate code
        private static readonly Regex CodePatterns = new Regex(
            @"(\{|\}|\[|\]|\(|\)|;|=>|->|::|\.\.\.|\+\+|--|==|!=|<=|>=|&&|\|\||<<|>>|" +
            @"function\s*\(|def\s+\w+|class\s+\w+|public\s+class|private\s+void|" +
            @"import\s+[\w\.]+|from\s+\w+\s+import|using\s+\w+|#include|" +
            @"\/\/.*|\/\*.*\*\/|#.*$|--.*$)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // Patterns for common variable declarations
        private static readonly Regex VariableDeclarationPattern = new Regex(
            @"(int|string|bool|boolean|var|let|const|float|double|char|long|short|byte)\s+\w+\s*=",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Check for consistent indentation (2 or 4 spaces, or tabs)
        private static readonly Regex IndentationPattern = new Regex(
            @"^(  +|\t+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public static bool IsCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 10)
                return false;

            // First check: if it's a URL, it's not code
            if (UrlDetector.IsUrl(text))
                return false;

            // Calculate various indicators
            int score = 0;
            
            // Check for code patterns (brackets, operators, etc.)
            if (CodePatterns.IsMatch(text))
                score += 3;

            // Check for variable declarations
            if (VariableDeclarationPattern.IsMatch(text))
                score += 2;

            // Check for keywords
            var words = text.Split(new[] { ' ', '\t', '\n', '\r', '(', ')', '{', '}', '[', ']', ';', ',', '.' }, 
                StringSplitOptions.RemoveEmptyEntries);
            var keywordCount = words.Count(word => CommonKeywords.Contains(word));
            if (keywordCount >= 2)
                score += 2;
            if (keywordCount >= 5)
                score += 1;

            // Check for consistent indentation (sign of structured code)
            var indentMatches = IndentationPattern.Matches(text);
            if (indentMatches.Count >= 2)
                score += 1;

            // Check for semicolons at end of lines (common in many languages)
            if (Regex.IsMatch(text, @";\s*$", RegexOptions.Multiline))
                score += 1;

            // Check for function/method calls
            if (Regex.IsMatch(text, @"\w+\s*\([^)]*\)"))
                score += 1;

            // Check bracket balance (basic check)
            var openBrackets = text.Count(c => c == '{' || c == '(' || c == '[');
            var closeBrackets = text.Count(c => c == '}' || c == ')' || c == ']');
            if (openBrackets > 0 && Math.Abs(openBrackets - closeBrackets) <= 2)
                score += 1;

            // If multiple lines, check if most lines are short (typical of code)
            var lines = text.Split('\n');
            if (lines.Length > 3)
            {
                var avgLineLength = lines.Where(l => !string.IsNullOrWhiteSpace(l))
                                         .Select(l => l.Trim().Length)
                                         .DefaultIfEmpty(0)
                                         .Average();
                if (avgLineLength < 80)
                    score += 1;
            }

            // A score of 4 or higher indicates it's likely code
            return score >= 4;
        }

        public static string DetectLanguage(string code)
        {
            // Simple language detection based on specific patterns
            if (Regex.IsMatch(code, @"#include\s*<.*>|std::|cout\s*<<|cin\s*>>"))
                return "C++";
            if (Regex.IsMatch(code, @"using\s+System|namespace\s+\w+|public\s+class.*\{"))
                return "C#";
            if (Regex.IsMatch(code, @"import\s+java\.|public\s+static\s+void\s+main"))
                return "Java";
            if (Regex.IsMatch(code, @"def\s+\w+.*:|if\s+__name__\s*==|import\s+\w+|from\s+\w+\s+import"))
                return "Python";
            if (Regex.IsMatch(code, @"function\s*\(|const\s+\w+\s*=|let\s+\w+\s*=|=>\s*\{|console\.log"))
                return "JavaScript";
            if (Regex.IsMatch(code, @"SELECT\s+.*FROM|INSERT\s+INTO|UPDATE\s+.*SET|CREATE\s+TABLE", RegexOptions.IgnoreCase))
                return "SQL";
            if (Regex.IsMatch(code, @"<\?php|echo\s+.*\;|\$\w+\s*="))
                return "PHP";
            if (Regex.IsMatch(code, @"func\s+\w+|var\s+\w+\s+\w+|package\s+\w+|import\s+"""))
                return "Go";
            if (Regex.IsMatch(code, @"fn\s+\w+|let\s+mut\s+|impl\s+\w+|use\s+\w+::|#\[derive"))
                return "Rust";
            
            return "Code"; // Generic code if language cannot be determined
        }
    }
}