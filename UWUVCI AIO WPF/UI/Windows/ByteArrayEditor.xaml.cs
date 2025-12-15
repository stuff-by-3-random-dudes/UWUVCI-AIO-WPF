using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class ByteArrayEditor : Window
    {
        public string? Result { get; private set; }

        public ByteArrayEditor(string? initialAN = null)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(initialAN) && initialAN.Trim().StartsWith("a", StringComparison.OrdinalIgnoreCase))
            {
                HexInput.Text = ExtractHexFromAN(initialAN);
            }
            HexInput.TextChanged += (s, e) => Preview.Text = FormatPreview(HexInput.Text);
            Preview.Text = FormatPreview(HexInput.Text);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var hex = CleanHex(HexInput.Text);
            if (hex.Length == 0)
            {
                Result = string.Empty;
                DialogResult = true; return;
            }
            if (hex.Length % 2 != 0 || !Regex.IsMatch(hex, "^[0-9a-fA-F]+$"))
            {
                MessageBox.Show(this, "Please enter a valid even-length hexadecimal string.", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Result = ToAN(hex);
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void PasteFromAN_Click(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text)) return;
            HexInput.Text = ExtractHexFromAN(text);
        }

        private static string CleanHex(string s) => Regex.Replace(s ?? string.Empty, "[^0-9a-fA-F]", string.Empty);

        private static string ExtractHexFromAN(string s)
        {
            var t = s.Replace("\r", string.Empty);
            var lines = t.Split('\n');
            if (lines.Length == 0) return string.Empty;
            var header = lines[0].Trim();
            // header like a6:
            var idx = header.IndexOf(':');
            var hex = new StringBuilder();
            if (idx >= 0)
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    foreach (var token in lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (token.Length == 2)
                            hex.Append(token);
                    }
                }
                return hex.ToString();
            }
            return CleanHex(s);
        }

        private static string ToAN(string hex)
        {
            int bytes = hex.Length / 2;
            if (bytes == 1) return "0x" + hex.ToUpper();
            var sb = new StringBuilder();
            sb.Append('a').Append(bytes.ToString()).Append(':');
            for (int i = 0; i < bytes; i++)
            {
                if (i % 16 == 0) sb.Append("\r\n"); else sb.Append(' ');
                sb.Append(hex[i * 2]).Append(hex[i * 2 + 1]);
            }
            return sb.ToString().ToUpper();
        }

        private static string FormatPreview(string input)
        {
            var hex = CleanHex(input ?? string.Empty);
            if (hex.Length == 0) return string.Empty;
            try { return ToAN(hex); } catch { return string.Empty; }
        }
    }
}

