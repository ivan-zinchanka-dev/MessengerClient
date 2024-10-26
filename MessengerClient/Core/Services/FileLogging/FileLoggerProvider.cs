﻿using Microsoft.Extensions.Logging;

namespace MessengerClient.Core.Services.FileLogging;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _fullFileName;

    public FileLoggerProvider(string fullFileName)
    {
        _fullFileName = fullFileName;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_fullFileName, categoryName);
    }
    
    public void Dispose() { }
}