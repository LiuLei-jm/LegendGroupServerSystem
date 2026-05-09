using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LegendGroupServerSystem.WPf.Messages;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace LegendGroupServerSystem.WPf.ViewModels.Pages;

public partial class LogViewModel : ObservableObject, IRecipient<AppLogMessage>, IDisposable
{
    private const int MaxLogsCount = 1000;
    private readonly Dispatcher _dispatcher;
    private readonly IMessenger _messenger;
    public ObservableCollection<string> Logs { get; } = [];

    public LogViewModel(Dispatcher dispatcher, IMessenger messenger)
    {
        _dispatcher = dispatcher;
        Logs.CollectionChanged += OnLogsCollectionChanged;
        _messenger = messenger;
        _messenger.Register(this);
    }

    private void OnLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ClearLogsCommand.NotifyCanExecuteChanged();
    }

    public void Receive(AppLogMessage message)
    {
        var formatted = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message.Message}";
        _dispatcher.BeginInvoke(new Action(() =>
        {
            Logs.Add(formatted);
            if (Logs.Count > MaxLogsCount) Logs.RemoveAt(0);
        }), DispatcherPriority.Background);
    }

    private bool CanClear() => Logs.Count > 0;
    [RelayCommand(CanExecute = nameof(CanClear))]
    private void ClearLogs()
    {
        Logs.Clear();
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messenger?.Unregister<AppLogMessage>(this);
            Logs?.CollectionChanged -= OnLogsCollectionChanged;
        }
    }
}
