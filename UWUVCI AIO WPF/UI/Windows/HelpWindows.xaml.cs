using System;
using System.IO;
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

        private static readonly string LocalReadMePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReadMe.txt");

        private DispatcherTimer _searchTimer;

        public HelpWindow()
        {
            InitializeComponent();
            Loaded += HelpWindow_Loaded;
            PreviewKeyDown += HelpWindow_PreviewKeyDown;

            // Dynamic page wrapping based on window size
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
            await LoadReadMeAsync();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            DisplayText("Refreshing latest ReadMe...");
            await LoadReadMeAsync(forceOnline: true);
        }

        private async Task LoadReadMeAsync(bool forceOnline = false)
        {
            try
            {
                string content = null;
                if (forceOnline || !File.Exists(LocalReadMePath))
                {
                    try
                    {
                        content = await DownloadReadMeAsync();
                        File.WriteAllText(LocalReadMePath, content);
                    }
                    catch
                    {
                        // Ignore, fallback below
                    }
                }

                if (string.IsNullOrWhiteSpace(content) && File.Exists(LocalReadMePath))
                    content = File.ReadAllText(LocalReadMePath);

                if (string.IsNullOrWhiteSpace(content))
                    DisplayText("⚠️ Unable to load ReadMe — no internet or local file found.");
                else
                    DisplayText(content, parseLinks: true);
            }
            catch (Exception ex)
            {
                DisplayText($"⚠️ Error loading ReadMe:\n{ex.Message}");
            }
        }

        private async Task<string> DownloadReadMeAsync()
        {
            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                return await client.DownloadStringTaskAsync(RemoteReadMeUrl);
            }
        }

        // ✅ Display text with optional clickable links
        private void DisplayText(string text, bool parseLinks = false)
        {
            ReadMeViewer.Document.Blocks.Clear();
            var para = new Paragraph();
            para.Margin = new Thickness(0);

            if (!parseLinks)
            {
                para.Inlines.Add(new Run(text));
                ReadMeViewer.Document.Blocks.Add(para);
                return;
            }

            // Turn URLs into clickable links
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            Regex urlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                int lastIndex = 0;
                foreach (Match m in urlRegex.Matches(line))
                {
                    if (m.Index > lastIndex)
                        para.Inlines.Add(new Run(line.Substring(lastIndex, m.Index - lastIndex)));

                    var link = new Hyperlink(new Run(m.Value))
                    {
                        NavigateUri = new Uri(m.Value),
                        Foreground = Brushes.DodgerBlue
                    };
                    link.RequestNavigate += (s, e) => System.Diagnostics.Process.Start(e.Uri.ToString());
                    para.Inlines.Add(link);

                    lastIndex = m.Index + m.Length;
                }

                if (lastIndex < line.Length)
                    para.Inlines.Add(new Run(line.Substring(lastIndex)));

                para.Inlines.Add(new LineBreak());
            }

            ReadMeViewer.Document.Blocks.Add(para);
        }

        // ✅ Smarter search with debounce + async highlighting
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

            // Run asynchronously to avoid freezing the UI
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
            {
                HighlightSearch(SearchBox.Text);
            }
        }
    }
}
