using System.Collections.Generic;

namespace DaJet.Scripting
{
    public static class ScriptHelper
    {
        private static HashSet<string> _keywords = new()
        {
            "SELECT", "FROM", "WHERE", "AND", "OR", "AS", "GO", "OUTPUT", "INTO", "SET",
            "INNER", "LEFT", "RIGHT", "OUTER", "CROSS", "FULL", "JOIN", "ON", "DECLARE",
            "UNION", "ALL", "IF", "ELSE", "CASE", "WHEN", "THEN", "BEGIN", "END",
            "ORDER", "BY", "ASC", "DESC", "GROUP", "BETWEEN", "DISTINCT", "IN", "IS",
            "TRANSACTION", "TRAN", "COMMIT", "ROLLBACK", "CONVERT", "HAVING", "LIKE",
            "CREATE", "ALTER", "INSERT", "UPDATE", "DELETE", "TRUNCATE", "TABLE", "DROP",
            "TOP", "LIMIT", "MERGE", "WITH", "NOT", "OVER", "SUM", "MAX", "MIN", "COUNT"
        };
        public static bool IsKeyword(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return false;
            }

            return _keywords.Contains(identifier.ToUpperInvariant());
        }
        public static bool IsNullLiteral(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return false;
            }

            string test = identifier.ToLowerInvariant();

            return (test == "null");
        }
        public static bool IsBooleanLiteral(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return false;
            }

            string test = identifier.ToLowerInvariant();

            return (test == "true" || test == "false");
        }
        public static bool IsAlpha(char character)
        {
            return character == '_'
                || character == '.' // multipart identifier
                || (character >= 'A' && character <= 'Z')
                || (character >= 'a' && character <= 'z')
                || (character >= 'А' && character <= 'Я')
                || (character >= 'а' && character <= 'я');
        }
        public static bool IsNumeric(char character)
        {
            return (character >= '0' && character <= '9');
        }
        public static bool IsAlphaNumeric(char character)
        {
            return IsAlpha(character) || IsNumeric(character);
        }
    }
}