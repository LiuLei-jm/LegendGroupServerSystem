using LegendGroupServerSystem.WPf.ViewModels;
using System.Windows;
using Forms = System.Windows.Forms;

namespace legendGroupServerSystem.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Forms.NotifyIcon _notifyIcon;

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = new System.Drawing.Icon("Resources/favicon.ico"),
            Visible = true,
            Text = "传奇功能网关",
        };
        _notifyIcon.DoubleClick += (s, e) =>
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        };
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _notifyIcon.Dispose();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            Hide();
    }
}

