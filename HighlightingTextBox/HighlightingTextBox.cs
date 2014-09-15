namespace HighlightingTextBox
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class HighlightingTextBox : Canvas
    {
        private TextBox internalTextBox;
        private Canvas highlightCanvas;
        private bool highlightsUpdating;

        #region DependencyProperties

        public static readonly DependencyProperty HighlightColorProperty =
            DependencyProperty.Register("HighlightColor", typeof(Brush), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty ShouldHighlightProperty =
            DependencyProperty.Register("ShouldHighlight", typeof(Func<string, bool>), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(new Func<string, bool>(s => false), FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));

        public static readonly DependencyProperty CaretIndexProperty =
            DependencyProperty.Register("CaretIndex", typeof(int), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaretIndexPropertyChanged));

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((HighlightingTextBox)d).internalTextBox.Foreground = (Brush)e.NewValue));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((HighlightingTextBox)d).internalTextBox.FontSize = (double)e.NewValue));

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((HighlightingTextBox)d).internalTextBox.Padding = (Thickness)e.NewValue));

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(default(FontFamily), FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((HighlightingTextBox)d).internalTextBox.FontFamily = (FontFamily)e.NewValue));

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(HighlightingTextBox),
                new FrameworkPropertyMetadata(new Thickness(1), FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((HighlightingTextBox)d).internalTextBox.BorderThickness = (Thickness)e.NewValue));

        #endregion

        public HighlightingTextBox()
        {
            InitializeInternalTextBox();
            Focusable = true;
        }

        #region Properties

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public Brush HighlightColor
        {
            get { return (Brush)GetValue(HighlightColorProperty); }
            set { SetValue(HighlightColorProperty, value); }
        }

        public Func<string, bool> ShouldHighlight
        {
            get { return (Func<string, bool>)GetValue(ShouldHighlightProperty); }
            set { SetValue(ShouldHighlightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public int CaretIndex
        {
            get { return (int)GetValue(CaretIndexProperty); }
            set { SetValue(CaretIndexProperty, value); }
        }

        protected override void OnInitialized(EventArgs e)
        {
            internalTextBox.TextChanged += OnInternalTextChanged;
            internalTextBox.SelectionChanged += OnInternalSelectionChanged;
            internalTextBox.AddHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)OnScrollChanged);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Keyboard.Focus(internalTextBox);
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var highlightedTextBox = (HighlightingTextBox)d;
            highlightedTextBox.internalTextBox.Text = highlightedTextBox.Text;
        }

        private static void OnCaretIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (((HighlightingTextBox)d).internalTextBox.CaretIndex != (int)e.NewValue)
            {
                ((HighlightingTextBox)d).internalTextBox.CaretIndex = (int)e.NewValue;
            }
        }

        #endregion

        private void InitializeInternalTextBox()
        {
            internalTextBox = new TextBox { Background = Brushes.Transparent };

            var widthBinding = new Binding("ActualWidth") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(HighlightingTextBox), 1) };
            BindingOperations.SetBinding(internalTextBox, WidthProperty, widthBinding);
            var heightBinding = new Binding("ActualHeight") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(HighlightingTextBox), 1) };
            BindingOperations.SetBinding(internalTextBox, HeightProperty, heightBinding);

            Children.Add(internalTextBox);

            SetTop(internalTextBox, 0);
            SetLeft(internalTextBox, 0);
            SetZIndex(internalTextBox, 255);
        }

        private void OnInternalTextChanged(object sender, TextChangedEventArgs e)
        {
            var newText = ((TextBox)sender).Text;
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (Text != newText)
            {
                Text = newText;
            }
            UpdateHighlights();
        }

        private void OnInternalSelectionChanged(object sender, RoutedEventArgs e)
        {
            CaretIndex = internalTextBox.CaretIndex;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateHighlights();
        }

        private void UpdateHighlights()
        {
            if (highlightsUpdating || !internalTextBox.IsLoaded)
            {
                return;
            }
            highlightsUpdating = true;
            ResetHighlightCanvas();
            HighlightMatchedSubstrings();
            highlightsUpdating = false;
        }

        private void ResetHighlightCanvas()
        {
            if (highlightCanvas != null && Children.Contains(highlightCanvas))
            {
                Children.Remove(highlightCanvas);
            }
            highlightCanvas = GetNewHighlightCanvas();
            Children.Add(highlightCanvas);
        }

        private Canvas GetNewHighlightCanvas()
        {
            var x = internalTextBox.Padding.Left;
            var y = internalTextBox.Padding.Top;
            var width = internalTextBox.Width - internalTextBox.Padding.Left - internalTextBox.Padding.Right;
            var height = internalTextBox.Height - internalTextBox.Padding.Top - internalTextBox.Padding.Bottom;
            var highlightCanvasBounds = new Rect(x, y, width, height);
            return new Canvas { Clip = new RectangleGeometry(highlightCanvasBounds) };
        }

        private void HighlightMatchedSubstrings()
        {
            var substrings = Text != null ? Text.Split(',') : new string[0];
            var stringIndex = 0;
            foreach (var substring in substrings)
            {
                var trimmedSubstring = substring.Trim();
                if (ShouldHighlight(trimmedSubstring))
                {
                    var leadingWhiteSpaceCount = (substring.Length - substring.TrimStart().Length);
                    AddHighlight(stringIndex + leadingWhiteSpaceCount, trimmedSubstring.Length);
                }
                stringIndex += substring.Length + 1;
            }
        }

        private void AddHighlight(int startIndex, int length)
        {
            var positionRect = PositionForHighlight(startIndex, length);
            var highlight = new Rectangle { Width = positionRect.Width, Height = positionRect.Height, Fill = HighlightColor };
            highlightCanvas.Children.Add(highlight);
            SetZIndex(highlight, 128);
            SetTop(highlight, positionRect.Top);
            SetLeft(highlight, positionRect.Left);
        }

        private Rect PositionForHighlight(int startIndex, int length)
        {
            var positionRect = internalTextBox.GetRectFromCharacterIndex(startIndex);
            positionRect.Union(internalTextBox.GetRectFromCharacterIndex(startIndex + length));
            return positionRect;
        }
    }
}
