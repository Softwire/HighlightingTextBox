namespace HighlightingTextBox.Tests.TestHelpers
{
    using FluentAssertions.Primitives;

    public class TypedObjectAssertions<T> : ObjectAssertions
    {
        public TypedObjectAssertions(T value) : base(value)
        {
            Subject = value;
        }

        public new T Subject { get; protected set; }
    }
}
