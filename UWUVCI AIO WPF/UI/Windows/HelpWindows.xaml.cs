using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace UWUVCI_AIO_WPF.UI.Windows
{
    public partial class HelpWindow : Window
    {
        private static readonly string RemoteReadMeUrl =
            "https://raw.githubusercontent.com/ZestyTS/UWUVCI-AIO-WPF/refs/heads/master/UWUVCI%20AIO%20WPF/uwuvci_installer_creator/app/Readme.txt";

        private static readonly string RemotePatchNotesUrl =
            "https://raw.githubusercontent.com/ZestyTS/UWUVCI-AIO-WPF/refs/heads/master/UWUVCI%20AIO%20WPF/uwuvci_installer_creator/app/PatchNotes.txt";

        private static readonly string LocalReadMePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReadMe.txt");

        private static readonly string LocalPatchNotesPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PatchNotes.txt");

        private DispatcherTimer _searchTimer;
        private readonly string _mode; // "readme" or "patchnotes"

        public HelpWindow(string mode = "readme")
        {
            InitializeComponent();

            _mode = mode.ToLowerInvariant();
            WindowTitleText.Text = _mode == "patchnotes"
                ? "📝 UWUVCI Patch Notes Viewer"
                : "📘 UWUVCI ReadMe Viewer";

            Loaded += HelpWindow_Loaded;
            PreviewKeyDown += HelpWindow_PreviewKeyDown;

            // dynamic wrap adjustment
            SizeChanged += (s, e) =>
            {
                var scrollBarWidth = SystemParameters.VerticalScrollBarWidth;
                ReadMeViewer.Document.PageWidth = e.NewSize.Width - scrollBarWidth - 140;
            };

            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                HighlightSearch(SearchBox.Text);
            };
        }

        private async void HelpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mode == "patchnotes")
            {
                Title = "UWUVCI Patch Notes Viewer";
                DisplayText("Loading Patch Notes...");
                await LoadTextFileAsync(LocalPatchNotesPath, RemotePatchNotesUrl);
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 255)); // subtle blue-white
            }
            else
            {
                Title = "UWUVCI ReadMe Viewer";
                DisplayText("Loading ReadMe...");
                await LoadTextFileAsync(LocalReadMePath, RemoteReadMeUrl);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == "patchnotes")
                DisplayText("Refreshing latest Patch Notes...");
            else
                DisplayText("Refreshing latest ReadMe...");

            await LoadTextFileAsync(
                _mode == "patchnotes" ? LocalPatchNotesPath : LocalReadMePath,
                _mode == "patchnotes" ? RemotePatchNotesUrl : RemoteReadMeUrl,
                forceOnline: true
            );
        }

        private async Task LoadTextFileAsync(string localPath, string remoteUrl, bool forceOnline = false)
        {
            try
            {
                string content = null;

                if (forceOnline || !File.Exists(localPath))
                {
                    try
                    {
                        content = await DownloadTextAsync(remoteUrl);
                        File.WriteAllText(localPath, content);
                    }
                    catch
                    {
                        // Ignore and fallback to local
                    }
                }

                if (string.IsNullOrWhiteSpace(content) && File.Exists(localPath))
                    content = File.ReadAllText(localPath);

                if (string.IsNullOrWhiteSpace(content))
                    DisplayText("Unable to load file — no internet or local copy found.");
                else
                    DisplayText(content, parseLinks: true);
            }
            catch (Exception ex)
            {
                DisplayText($"Error loading file:\n{ex.Message}");
            }
        }

        private async Task<string> DownloadTextAsync(string url)
        {
            using var client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            return await client.DownloadStringTaskAsync(url);
        }

        // -------- Display Logic --------
        private void DisplayText(string text, bool parseLinks = false)
        {
            ReadMeViewer.Document.Blocks.Clear();
            ReadMeViewer.Document.LineHeight = 22; // consistent line spacing

            var urlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            bool firstVersionShown = false;

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // ==========================================
                // 1. Section Dividers (==== / ----)
                // ==========================================
                if (Regex.IsMatch(line, @"^[=\-]{5,}$"))
                {
                    var sep = new Paragraph(new Run(" "))
                    {
                        Margin = new Thickness(0, 6, 0, 6),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(21, 101, 192)), // blue accent
                        BorderThickness = new Thickness(0, 0, 0, 2)
                    };
                    ReadMeViewer.Document.Blocks.Add(sep);
                    continue;
                }

                // ==========================================
                // 2. Version Header (Patch Notes)
                // ==========================================
                if (line.StartsWith("Version ", StringComparison.OrdinalIgnoreCase))
                {
                    bool isFirstVersion = !firstVersionShown;
                    firstVersionShown = true;

                    // Extract release date if available
                    DateTime? releaseDate = null;
                    var match = Regex.Match(line, @"\b(\w+)\s+(\d{1,2}),\s*(\d{4})\b");
                    if (match.Success && DateTime.TryParse($"{match.Groups[1].Value} {match.Groups[2].Value}, {match.Groups[3].Value}", out var parsed))
                        releaseDate = parsed;

                    var run = new Run(line)
                    {
                        FontWeight = FontWeights.Bold,
                        FontSize = 17,
                        Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)) // blue header
                    };

                    var p = new Paragraph { Margin = new Thickness(0, 10, 0, 3) };
                    p.Inlines.Add(run);

                    // Add "⭐ NEW" tag for most recent version
                    if (isFirstVersion && (!releaseDate.HasValue || (DateTime.Now - releaseDate.Value).TotalDays <= 7))
                    {
                        var tag = new Run("  ⭐ NEW")
                        {
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)), // orange
                            FontWeight = FontWeights.Bold,
                            FontSize = 15
                        };
                        p.Inlines.Add(tag);
                    }

                    ReadMeViewer.Document.Blocks.Add(p);
                    continue;
                }

                // ==========================================
                // 3. Major Section Titles (e.g. PROJECT OVERVIEW / FAQ)
                // ==========================================
                if (Regex.IsMatch(line, @"^[A-Z\s]{4,}$") && line.Length < 60)
                {
                    var title = new Run(line.Trim())
                    {
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
                    };
                    var p = new Paragraph(title) { Margin = new Thickness(0, 10, 0, 4) };
                    ReadMeViewer.Document.Blocks.Add(p);
                    continue;
                }

                // ==========================================
                // 4. Section Headers (**Improvements**, **Fixes**, etc.)
                // ==========================================
                if (line.StartsWith("**") && line.EndsWith("**"))
                {
                    string inner = line.Trim('*');
                    var run = new Run(inner)
                    {
                        FontWeight = FontWeights.Bold,
                        FontSize = 15,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 87, 34)) // orange accent
                    };
                    var p = new Paragraph(run) { Margin = new Thickness(0, 4, 0, 2) };
                    ReadMeViewer.Document.Blocks.Add(p);
                    continue;
                }

                // ==========================================
                // 5. FAQ Questions (Q#)
                // ==========================================
                if (Regex.IsMatch(line, @"^Q\d+\)"))
                {
                    var match = Regex.Match(line, @"^(Q\d+\))\s*(.*)");
                    if (match.Success)
                    {
                        var p = new Paragraph { Margin = new Thickness(0, 8, 0, 2) };
                        p.Inlines.Add(new Run(match.Groups[1].Value + " ")
                        {
                            Foreground = new SolidColorBrush(Color.FromRgb(0, 188, 212)), // cyan Q#
                            FontWeight = FontWeights.Bold
                        });
                        p.Inlines.Add(new Run(match.Groups[2].Value)
                        {
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Black
                        });
                        ReadMeViewer.Document.Blocks.Add(p);
                        continue;
                    }
                }

                // ==========================================
                // 6. FAQ Answers (A))
                // ==========================================
                if (Regex.IsMatch(line, @"^A\)"))
                {
                    var p = new Paragraph(new Run(line))
                    {
                        Margin = new Thickness(20, 0, 0, 6)
                    };
                    p.Inlines.FirstInline.Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33));
                    ReadMeViewer.Document.Blocks.Add(p);
                    continue;
                }

                // ==========================================
                // 7. Keyword Highlight (Error / Fix / Note)
                // ==========================================
                if (Regex.IsMatch(line, @"\b(error|fix|warning|note|important|issue)\b", RegexOptions.IgnoreCase))
                {
                    var run = new Run(line)
                    {
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)), // orange
                        FontWeight = FontWeights.SemiBold
                    };
                    var p = new Paragraph(run) { Margin = new Thickness(0, 2, 0, 2) };
                    ReadMeViewer.Document.Blocks.Add(p);
                    continue;
                }

                // ==========================================
                // 8. Bullet Points (- ...)
                // ==========================================
                if (line.StartsWith("- ") || line.StartsWith("• "))
                {
                    var bulletText = Regex.Replace(line, @"^[-•]\s*", "• ");
                    var bullet = new Run(bulletText)
                    {
                        FontSize = 14,
                        Foreground = Brushes.Black
                    };
                    var p = new Paragraph(bullet) { Margin = new Thickness(24, 0, 0, 0) };
                    ReadMeViewer.Document.Blocks.Add(p);
                    continue;
                }

                // ==========================================
                // 9. Hyperlink Detection
                // ==========================================
                if (urlRegex.IsMatch(line))
                {
                    var p = new Paragraph { Margin = new Thickness(0) };
                    int lastIndex = 0;

                    foreach (Match m in urlRegex.Matches(line))
                    {
                        if (m.Index > lastIndex)
                            p.Inlines.Add(new Run(line.Substring(lastIndex, m.Index - lastIndex)));

                        var link = new Hyperlink(new Run(m.Value))
                        {
                            NavigateUri = new Uri(m.Value),
                            Foreground = Brushes.DodgerBlue
                        };
                        link.RequestNavigate += (s, e) =>
                            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });

                        p.Inlines.Add(link);
                        lastIndex = m.Index + m.Length;
                    }

                    if (lastIndex < line.Length)
                        p.Inlines.Add(new Run(line.Substring(lastIndex)));

                    ReadMeViewer.Document.Blocks.Add(p);
                    continue;
                }

                // ==========================================
                // 10. Default Text
                // ==========================================
                var defaultParagraph = new Paragraph(new Run(line))
                {
                    Margin = new Thickness(0, 0, 0, 2)
                };
                ReadMeViewer.Document.Blocks.Add(defaultParagraph);
            }
        }


        // -------- Search Logic --------
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void HighlightSearch(string query)
        {
            var doc = ReadMeViewer.Document;
            TextRange full = new TextRange(doc.ContentStart, doc.ContentEnd);
            full.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);

            if (string.IsNullOrWhiteSpace(query))
            {
                ResultCountText.Text = "";
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                int count = 0;
                StringComparison comp = CaseSensitiveBox.IsChecked == true
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

                TextPointer pos = doc.ContentStart;
                while (pos != null && pos.CompareTo(doc.ContentEnd) < 0)
                {
                    string text = pos.GetTextInRun(LogicalDirection.Forward);
                    int index = text.IndexOf(query, comp);
                    if (index >= 0)
                    {
                        TextPointer startPos = pos.GetPositionAtOffset(index);
                        TextPointer endPos = pos.GetPositionAtOffset(index + query.Length);
                        if (startPos != null && endPos != null)
                        {
                            new TextRange(startPos, endPos)
                                .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
                            count++;
                        }
                        pos = pos.GetPositionAtOffset(index + query.Length);
                    }
                    else
                        pos = pos.GetNextContextPosition(LogicalDirection.Forward);
                }

                ResultCountText.Text = $"{count} match{(count == 1 ? "" : "es")} found";
            }), DispatcherPriority.Background);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            string allText = new TextRange(ReadMeViewer.Document.ContentStart, ReadMeViewer.Document.ContentEnd).Text;
            Clipboard.SetText(allText);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void HelpWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SearchBox.Focus();
                SearchBox.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                HighlightSearch(SearchBox.Text);
        }
    }
}
