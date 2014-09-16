namespace HighlightingTextBox.Tests
{
    using FluentAssertions;
    using global::HighlightingTextBox.Tests.TestHelpers;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Shapes;

    [TestFixture]
    [RequiresSTA]
    public class HighlightingTextBoxTests : Window
    {
        private HighlightingTextBox box;
        private TextBox internalTextBox;

        private IEnumerable<Rectangle> Highlights
        {
            get { return HighlightCanvas != null ? HighlightCanvas.Children.OfType<Rectangle>() : Enumerable.Empty<Rectangle>(); }
        }

        private Canvas HighlightCanvas
        {
            get { return box.Children.OfType<Canvas>().SingleOrDefault(); }
        }

        [SetUp]
        public void SetUp()
        {
            var window = new TestWindow();
            box = new HighlightingTextBox { Width = 300, Height = 30, ShouldHighlight = TestHighlightCondition };
            internalTextBox = box.Children.OfType<TextBox>().Single();

            window.AddHighlightingTextBox(box);
            window.Show();
        }

        [Test]
        public void Highlights_are_only_visible_within_textbox()
        {
            // Given
            box.Padding = new Thickness { Left = 10, Top = 3, Right = 11, Bottom = 4 };
            box.Width = 300;
            box.Height = 30;

            // When
            box.Text = "HIT";

            // Then
            var highlightCanvasBounds = HighlightCanvas.Clip.Bounds;
            highlightCanvasBounds.X.Should().Be(10);
            highlightCanvasBounds.Y.Should().Be(3);
            highlightCanvasBounds.Width.Should().Be(300 - 10 - 11);
            highlightCanvasBounds.Height.Should().Be(30 - 3 - 4);
        }

        [Test]
        public void A_highlight_is_added_for_a_match_even_with_whitespace()
        {
            // When
            box.Text = "    HIT  \r\n\t  ,MISS,MISS";

            // Then
            Highlights.Should().HaveCount(1);
        }

        [Test]
        public void No_highlights_added_for_no_matches()
        {
            // When
            box.Text = "MISS, MISS, MISS";

            // Then
            Highlights.Should().BeEmpty();
        }

        [Test]
        public void Highlights_are_added_for_multiple_matches()
        {
            // When
            box.Text = "MISS, HIT, MISS, HIT, HIT, MISS";

            // Then
            Highlights.Should().HaveCount(3);
        }

        [Test]
        public void Highlights_are_updated_when_text_is_changed()
        {
            // Given
            box.Text = "MISS, HIT";
            Highlights.Should().HaveCount(1);

            //When
            box.Text = "MISS, MISS ";

            // Then
            Highlights.Should().HaveCount(0);
        }

        [Test]
        public void Internal_text_box_is_focussed_when_highlight_box_is_focussed()
        {
            // When
            Keyboard.Focus(box);

            // Then
            Keyboard.FocusedElement.Should().BeOfType<TextBox>();
        }

        [Test]
        public void Highlight_text_box_text_is_set_when_internal_text_is_changed()
        {
            // Given
            const string newText = "This is some text";

            // When
            internalTextBox.Text = newText;

            //Then
            box.Text.Should().Be(newText);
        }

        [TestCase("MISS, HIT, MISS", 6)]
        [TestCase("MISS, MISS, HIT", 12)]
        public void Single_highlight_rectangle_is_positioned_correctly(string text, int index)
        {
            // When
            box.Text = text;

            // Then
            var positionRect = PositionRect(index, 3);
            Highlights.First().Should().BeAtPosition(positionRect);
        }

        [TestCase("HIT, HIT, MISS", 0, 5)]
        [TestCase("HIT, MISS, HIT", 0, 11)]
        [TestCase("HIT, HIT, HIT", 0, 5, 10)]
        public void Highlight_rectangles_are_positioned_correctly(string text, params int[] indices)
        {
            // When
            box.Text = text;

            //Then
            var highlightsList = Highlights.ToList();
            for (var i = 0; i < indices.Length; i++)
            {
                var positionRect = PositionRect(indices[i], 3);
                highlightsList[i].Should().BeAtPosition(positionRect);
            }
        }

        [Test]
        public void Highlights_update_when_textbox_is_scrolled()
        {
            // Given
            box.Text = "MISS, HIT, MISS";
            box.Width = 20;
            var originalPosition = Canvas.GetLeft(Highlights.First());

            // When
            internalTextBox.ScrollToHorizontalOffset(10);
            internalTextBox.UpdateLayout();

            // Then
            var positionAfterScroll = Canvas.GetLeft(Highlights.First());
            positionAfterScroll.Should().Be(originalPosition - 10);
        }

        [Test]
        public void Setting_caret_position_to_current_value_while_text_is_selected_keeps_the_selection()
        {
            // Given
            box.Text = "florence";
            internalTextBox.Select(3, 2);

            // When
            box.CaretIndex = 3;

            // Then
            internalTextBox.SelectionStart.Should().Be(3);
            internalTextBox.SelectionLength.Should().Be(2);
        }

        [Test]
        public void Setting_caret_position_to_different_value_updates_caret_index()
        {
            // Given
            box.Text = "florence";
            internalTextBox.Select(3, 2);

            // When
            box.CaretIndex = 4;

            // Then
            internalTextBox.SelectionStart.Should().Be(4);
            internalTextBox.SelectionLength.Should().Be(0);
        }

        private Rect PositionRect(int startIndex, int length)
        {
            var positionRect = internalTextBox.GetRectFromCharacterIndex(startIndex);
            positionRect.Union(internalTextBox.GetRectFromCharacterIndex(startIndex + length));
            return positionRect;
        }

        private bool TestHighlightCondition(string trimmedSubstring)
        {
            return trimmedSubstring == "HIT";
        }

        private class TestWindow : Window
        {
            public void AddHighlightingTextBox(HighlightingTextBox highlightingTextBox)
            {
                var panel = new StackPanel();
                panel.Children.Add(highlightingTextBox);
                AddChild(panel);
            }
        }
    }
}
