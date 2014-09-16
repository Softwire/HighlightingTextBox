HighlightingTextBox
===================

A custom textbox for WPF which will highlight text in a comma-separated list depending on a supplied function.

Usage
==========


The dependency properties `HighlightColor` and `ShouldHighlight` should be specified. 

For example, this will highlight in yellow the first and second items in this list when it is typed into the textbox: `HIT, FOOHITBAR, MISS`

FooView.xaml:

```xml
<Window x:Class="TestWpf.Views.TestView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:highlightingTextBox="clr-namespace:HighlightingTextBox;assembly=HighlightingTextBox"
        Title="TestView" Height="300" Width="300">
    <Grid>
        <highlightingTextBox:HighlightingTextBox x:Name="Test"
                                    ShouldHighlight="{Binding Matches}"
                                    HighlightColor="Yellow" />
    </Grid>
</Window>
```

FooViewModel.cs:

```c#
        public Func<string, bool> Matches
        {
            get { return x => x.Contains("HIT"); }
        }
```

