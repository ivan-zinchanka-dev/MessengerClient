namespace MessengerClient.Core.Infrastructure;

public enum QueryHeader : byte
{
    None = 0,
    Login = 1,
    PostMessage = 2,
    UpdateChat = 3,
    Quit = 10,
}