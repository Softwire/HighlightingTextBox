namespace HighlightingTextBox.Tests.TestHelpers
{
    using FluentAssertions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;

    public static class AssertionsExtensions
    {
        public static HighlightAssertions Should(this Rectangle highlight)
        {
            return new HighlightAssertions(highlight);
        }
    }

    public class HighlightAssertions : TypedObjectAssertions<Rectangle>
    {
        private readonly Rectangle highlight;

        public HighlightAssertions(Rectangle value)
            : base(value)
        {
            highlight = value;
        }

        protected override string Context
        {
            get { return typeof(Rectangle).Name; }
        }

        /// Asserts that a rectangle is positioned in a Canvas with the same dimensions as positionRect
        public AndConstraint<HighlightAssertions> BeAtPosition(Rect positionRect)
        {
            highlight.Width.Should().Be(positionRect.Width);
            highlight.Height.Should().Be(positionRect.Height);
            Canvas.GetLeft(highlight).Should().Be(positionRect.Left);
            Canvas.GetTop(highlight).Should().Be(positionRect.Top);
            return new AndConstraint<HighlightAssertions>(this);
        }
    }
}
