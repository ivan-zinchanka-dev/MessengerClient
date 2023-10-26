using System;

namespace MessengerClient.Core.Models;

public class Message
{
    public User Sender { get; init; }
    public User Receiver { get; init; }
    public string Text { get; init; }
    public DateTime PostDateTime { get; init; }

    public override string ToString()
    {
        return Text;
    }
}