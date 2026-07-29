using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OpenMyDear.Wpf.Helpers;

public static class SmoothScrollBehavior
{
    private const double FallbackScrollDistance = 72;
    private static readonly Duration ScrollDuration = TimeSpan.FromMilliseconds(140);

    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(SmoothScrollBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    private static readonly DependencyProperty AnimatedVerticalOffsetProperty = DependencyProperty.RegisterAttached(
        "AnimatedVerticalOffset",
        typeof(double),
        typeof(SmoothScrollBehavior),
        new PropertyMetadata(0d, OnAnimatedVerticalOffsetChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not UIElement element)
        {
            return;
        }

        if ((bool)e.OldValue)
        {
            element.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        if ((bool)e.NewValue)
        {
            element.PreviewMouseWheel += OnPreviewMouseWheel;
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not DependencyObject element || e.Delta == 0)
        {
            return;
        }

        var scrollViewer = FindScrollViewer(element);
        if (scrollViewer is null || scrollViewer.ScrollableHeight <= 0)
        {
            return;
        }

        var offsetChange = -(e.Delta / 120d) * GetScrollDistance(element);
        var targetOffset = Math.Clamp(
            scrollViewer.VerticalOffset + offsetChange,
            0,
            scrollViewer.ScrollableHeight);

        scrollViewer.BeginAnimation(AnimatedVerticalOffsetProperty, null);
        scrollViewer.SetValue(AnimatedVerticalOffsetProperty, scrollViewer.VerticalOffset);
        scrollViewer.BeginAnimation(
            AnimatedVerticalOffsetProperty,
            new DoubleAnimation(scrollViewer.VerticalOffset, targetOffset, ScrollDuration)
            {
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            },
            HandoffBehavior.SnapshotAndReplace);

        e.Handled = true;
    }

    private static void OnAnimatedVerticalOffsetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            var result = FindScrollViewer(child);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static double GetScrollDistance(DependencyObject element)
    {
        if (element is not ItemsControl itemsControl || itemsControl.Items.Count == 0)
        {
            return FallbackScrollDistance;
        }

        if (itemsControl.ItemContainerGenerator.ContainerFromIndex(0) is not FrameworkElement itemContainer
            || itemContainer.ActualHeight <= 0)
        {
            return FallbackScrollDistance;
        }

        var margin = itemContainer.Margin;
        return itemContainer.ActualHeight + margin.Top + margin.Bottom;
    }
}
