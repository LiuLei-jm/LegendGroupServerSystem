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
    private static readonly DependencyProperty ScrollChangedHandlerProperty =
        DependencyProperty.RegisterAttached("ScrollChangedHandler", typeof(ScrollChangedEventHandler), typeof(AutoScrollBehavior));
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
                ItemsControl_Unloaded(itemsControl, new RoutedEventArgs());
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
        var scrollViewer = FindScrollViewer(itemsControl);
        if (scrollViewer != null)
        {
            var handler = (ScrollChangedEventHandler)itemsControl.GetValue(ScrollChangedHandlerProperty);
            if (handler != null)
            {
                scrollViewer.ScrollChanged -= handler;
                scrollViewer.ClearValue(ScrollChangedHandlerProperty);
            }
        }
    }

    private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl) return;
        var scrollViewer = FindScrollViewer(itemsControl);
        if (scrollViewer == null) return;
        ScrollChangedEventHandler scrollChangedHandler = (s, args) =>
        {
            var internalAutoScrollState = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1;
            scrollViewer.SetValue(IsAutoScrollingProperty, internalAutoScrollState);
        };
        scrollViewer.SetValue(ScrollChangedHandlerProperty, scrollChangedHandler);
        scrollViewer.ScrollChanged += scrollChangedHandler;
        if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
        {
            NotifyCollectionChangedEventHandler collectionChangedHandler = (s, args) =>
            {
                var currentAutoScrollState = (bool)scrollViewer.GetValue(IsAutoScrollingProperty);
                if (!currentAutoScrollState) return;
                itemsControl.Dispatcher.BeginInvoke(
                   new Action(() => scrollViewer.ScrollToEnd()),
                   System.Windows.Threading.DispatcherPriority.Background
                    );
            };
            collection.CollectionChanged += collectionChangedHandler;
            itemsControl.SetValue(CollectionChangedHandlerProperty, collectionChangedHandler);
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
    public static readonly DependencyProperty IsAutoScrollingProperty
        = DependencyProperty.RegisterAttached("IsAutoScrolling", typeof(bool), typeof(AutoScrollBehavior));
}
