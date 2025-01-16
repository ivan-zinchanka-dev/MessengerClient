using System.Diagnostics;
using System.Text;
using System.Windows;

namespace MessengerClient;

public class BindingErrorTraceListener : TraceListener
{
    private readonly StringBuilder _messageBuilder = new StringBuilder();

    public override void Write(string message)
    {
        _messageBuilder.Append(message);
    }

    public override void WriteLine(string message)
    {
        Write(message);

        MessageBox.Show(_messageBuilder.ToString(), "Binding error!", MessageBoxButton.OK, MessageBoxImage.Warning);
        _messageBuilder.Clear();
    }
}