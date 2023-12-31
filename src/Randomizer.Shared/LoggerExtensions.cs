using System;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace Randomizer.Shared;

public static class LoggerExtensions
{
    /// <summary>
    /// Times the execution of the specified action and logs a message with the
    /// elapsed time as the last argument.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="message">
    /// Format string of the log message. The last argument will be the elapsed
    /// time in milliseconds, preceded by any arguments in <paramref
    /// name="args"/>.
    /// </param>
    /// <param name="args">
    /// An object array that contains zero of more objects to format.
    /// </param>
    /// <returns>
    /// A object that, when disposed, will trigger the log message to write.
    /// </returns>
    public static IDisposable Time(this ILogger logger, LogLevel logLevel, string? message, params object?[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        return new Disposable(() =>
        {
            stopwatch.Stop();
#pragma warning disable CA2254 // Template should be a static expression
            logger.Log(logLevel, message, CombineArgs(args, stopwatch.ElapsedMilliseconds));
#pragma warning restore CA2254 // Template should be a static expression
        });
    }

    /// <summary>
    /// Times the execution of the specified action and logs a debug message
    /// with the elapsed time as the last argument.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">
    /// Format string of the log message. The last argument will be the elapsed
    /// time in milliseconds, preceded by any arguments in <paramref
    /// name="args"/>.
    /// </param>
    /// <param name="args">
    /// An object array that contains zero of more objects to format.
    /// </param>
    /// <returns>
    /// A object that, when disposed, will trigger the log message to write.
    /// </returns>
    public static IDisposable TimeDebug(this ILogger logger, string? message, params object?[] args)
        => logger.Time(LogLevel.Debug, message, args);

    /// <summary>
    /// Times the execution of the specified action and logs an informational
    /// message with the elapsed time as the last argument.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">
    /// Format string of the log message. The last argument will be the elapsed
    /// time in milliseconds, preceded by any arguments in <paramref
    /// name="args"/>.
    /// </param>
    /// <param name="args">
    /// An object array that contains zero of more objects to format.
    /// </param>
    /// <returns>
    /// A object that, when disposed, will trigger the log message to write.
    /// </returns>
    public static IDisposable TimeInfo(this ILogger logger, string? message, params object?[] args)
        => logger.Time(LogLevel.Information, message, args);

    private static object?[] CombineArgs(object?[] args, object? arg)
    {
        var newArgs = new object?[args.Length + 1];
        if (args.Length > 0)
            Array.Copy(args, newArgs, args.Length);
        newArgs[^1] = arg;
        return newArgs;
    }
}
