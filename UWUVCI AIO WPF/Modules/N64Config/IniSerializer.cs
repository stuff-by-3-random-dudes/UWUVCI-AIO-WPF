using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UWUVCI_AIO_WPF.Modules.N64Config
{
    public static class IniSerializer
    {
        public static IniDocument Parse(string text)
        {
            var doc = new IniDocument();
            var lines = text.Replace("\r", string.Empty).Split('\n');
            IniSection? current = null;
            for (int i = 0; i < lines.Length; i++)
            {
                var raw = lines[i];
                var trimmed = raw.Trim();
                // skip empty and full-line comments (';' or '#', or '//')
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#") || trimmed.StartsWith("//")) continue;
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    var name = trimmed.Substring(1, trimmed.Length - 2);
                    current = doc.GetOrAddSection(name);
                    continue;
                }
                if (current == null) continue;
                var eq = trimmed.IndexOf('=');
                if (eq <= 0) continue;
                var key = trimmed.Substring(0, eq).Trim();
                var value = trimmed.Substring(eq + 1).Trim();
                value = StripInlineComment(value);
                if (IsByteArrayHeader(value))
                {
                    var sb = new StringBuilder();
                    sb.Append(value);
                    int j = i + 1;
                    for (; j < lines.Length; j++)
                    {
                        var t = lines[j].Trim();
                        if (string.IsNullOrWhiteSpace(t)) { break; }
                        if (t.StartsWith(";") || t.StartsWith("#") || t.StartsWith("//")) continue; // allow comments within blocks
                        // allow trailing inline comments on continuation
                        var withoutComment = StripInlineComment(t);
                        if (IsContinuationHexLine(withoutComment)) sb.Append("\n").Append(withoutComment);
                        else break;
                    }
                    current.Set(key, sb.ToString());
                    i = j - 1; // continue parsing from the first non-hex line
                }
                else
                {
                    current.Set(key, value);
                }
            }
            return doc;
        }

        private static string StripInlineComment(string value)
        {
            // remove trailing ; or # comment that is not inside quotes
            bool inQuotes = false;
            for (int k = 0; k < value.Length; k++)
            {
                var c = value[k];
                if (c == '"') inQuotes = !inQuotes;
                else if ((c == ';' || c == '#') && !inQuotes)
                {
                    return value.Substring(0, k).TrimEnd();
                }
            }
            return value;
        }

        private static bool IsByteArrayHeader(string value)
        {
            // a<number>:
            if (value.Length < 3) return false;
            if (value[0] != 'a' && value[0] != 'A') return false;
            int i = 1;
            while (i < value.Length && char.IsDigit(value[i])) i++;
            return i < value.Length && value[i] == ':';
        }

        private static bool IsContinuationHexLine(string line)
        {
            // one or more hex pairs separated by spaces (allow commas and optional 0x prefix)
            foreach (var tokenRaw in line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var token = tokenRaw.TrimEnd(',');
                if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    var hex = token.Substring(2);
                    if (hex.Length != 2) return false;
                    if (!IsHex(hex[0]) || !IsHex(hex[1])) return false;
                }
                else
                {
                    if (token.Length != 2) return false;
                    if (!IsHex(token[0]) || !IsHex(token[1])) return false;
                }
            }
            return line.Trim().Length > 0;
        }

        private static bool IsHex(char c)
            => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

        public static string Serialize(IniDocument doc)
        {
            var sb = new StringBuilder();
            // stable order: sections and keys sorted case-insensitively
            foreach (var sec in doc.Sections.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
            {
                sb.Append('[').Append(sec.Name).Append("]\r\n");
                foreach (var p in sec.Properties.OrderBy(pp => pp.Key, StringComparer.OrdinalIgnoreCase))
                {
                    if (IsByteArrayHeader(p.Value))
                    {
                        sb.Append(p.Key).Append(" = ");
                        WriteByteArrayBlock(sb, p.Value);
                        sb.Append("\r\n");
                    }
                    else
                    {
                        sb.Append(p.Key).Append(" = ").Append(p.Value).Append("\r\n");
                    }
                }
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        private static void WriteByteArrayBlock(StringBuilder sb, string value)
        {
            // value is like: aN:\n<hex pairs>
            var parts = value.Split(new[] { '\\', 'n' });
            // The above split is not correct; safer approach:
            var lines = value.Replace("\r", string.Empty).Split('\n');
            if (lines.Length == 0) { sb.Append(value); return; }
            sb.Append(lines[0]);
            if (lines.Length == 1) return;
            for (int i = 1; i < lines.Length; i++)
            {
                sb.Append("\r\n").Append(lines[i]);
            }
        }
    }
}
