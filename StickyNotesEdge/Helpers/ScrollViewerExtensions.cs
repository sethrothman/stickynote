using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

public static class ScrollViewerExtensions
{
    private static readonly DependencyProperty DummyProperty =
        DependencyProperty.RegisterAttached(
            "DummyProperty",
            typeof(double),
            typeof(ScrollViewerExtensions),
            new PropertyMetadata(0.0, OnDummyPropertyChanged));

    private static void OnDummyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
        }
    }

    public static void AnimateScroll(this ScrollViewer scrollViewer, double toOffset, double durationSeconds = 0.2)
    {
        var animation = new DoubleAnimation
        {
            From = scrollViewer.HorizontalOffset,
            To = toOffset,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);

        Storyboard.SetTarget(animation, scrollViewer);
        Storyboard.SetTargetProperty(animation, new PropertyPath(DummyProperty));

        storyboard.Begin(scrollViewer);
    }
}
