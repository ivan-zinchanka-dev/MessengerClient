using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MessengerClient.Views;

namespace MessengerClient.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{
    public ChatWindow Window { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;


    public ChatViewModel()
    {
        Window = new ChatWindow();
        Window.DataContext = this;
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}