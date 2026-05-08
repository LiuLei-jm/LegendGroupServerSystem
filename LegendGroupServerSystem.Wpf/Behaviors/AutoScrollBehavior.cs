using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LegendGroupServerSystem.WPf.Behaviors;

public static class AutoScrollBehavior
{
    public static bool GetEnable(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableProperty);
    }

    public static void SetEnable(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableProperty, value);
    }

    // Using a DependencyProperty as the backing store for Enable.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, propertyChangedCallback: OnEnableChanged));
    private static readonly DependencyProperty CollectionChangedHandlerProperty =
        DependencyProperty.RegisterAttached("CollectionChangedHandler", typeof(NotifyCollectionChangedEventHandler), typeof(AutoScrollBehavior));

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsControl itemsControl)
        {
            if ((bool)e.NewValue)
            {
                itemsControl.Loaded += ItemsControl_Loaded;
                itemsControl.Unloaded += ItemsControl_Unloaded;
            }
            else
            {
                itemsControl.Loaded -= ItemsControl_Loaded;
                itemsControl.Unloaded -= ItemsControl_Unloaded;
            }
        }
    }

    private static void ItemsControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl) return;
        if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
        {
            var handler = (NotifyCollectionChangedEventHandler)itemsControl.GetValue(CollectionChangedHandlerProperty);
            if (handler != null)
            {
                collection.CollectionChanged -= handler;
                itemsControl.ClearValue(CollectionChangedHandlerProperty);
            }
        }
    }

    private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl) return;
        var scrollViewer = FindScrollViewer(itemsControl);
        if (scrollViewer == null) return;
        bool autoScroll = true;
        scrollViewer.ScrollChanged += (s, args) =>
        {
            autoScroll = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1;
        };
        if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
        {
            NotifyCollectionChangedEventHandler handler = (s, args) =>
            {
                if (!autoScroll) return;
                itemsControl.Dispatcher.BeginInvoke(
                   new Action(() => scrollViewer.ScrollToEnd()),
                   System.Windows.Threading.DispatcherPriority.Background
                    );
            };
            collection.CollectionChanged += handler;
            itemsControl.SetValue(CollectionChangedHandlerProperty, handler);
        }
    }

    private static ScrollViewer FindScrollViewer(DependencyObject d)
    {
        if (d is ScrollViewer sv) return sv;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);
            var result = FindScrollViewer(child);
            if (result != null) return result;
        }
        return null!;
    }
}
