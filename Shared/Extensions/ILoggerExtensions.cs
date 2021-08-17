using DAM2.Shared.Insights;
using Microsoft.Extensions.Logging;
using System;
using static DAM2.Shared.Insights.InsightsLogger;

namespace DAM2.Shared.Extensions
{
    public static class ILoggerExtensions
    {
        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            LogLevel logLevel,
            string methodName)
        {
            LogMethodExecution(logger, operation, logLevel, ex: null, methodName);
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            LogLevel logLevel,
            Exception ex,
            string methodName)
        {
            if (ex == null)
            {
                logger.Log(
                    logLevel,
                    "{Operation} method {FullMethodName}",
                    operation,
                    GetFullMethodName(typeof(T), methodName));
            }
            else
            {
                logger.Log(
                    logLevel,
                    ex,
                    "{Operation} method {FullMethodName}",
                    operation,
                    GetFullMethodName(typeof(T), methodName));
            }
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            LogLevel logLevel,
            string methodName,
            string requestId)
        {
            LogMethodExecution(logger, operation, logLevel, methodName, requestId, null, null);
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            LogLevel logLevel,
            string methodName,
            string requestId,
            string extraMessageTemplate,
            params object[] messageArgs)
        {
            LogMethodExecution(
                logger,
                operation,
                logLevel,
                ex: null,
                methodName,
                requestId,
                extraMessageTemplate,
                messageArgs);
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            LogLevel logLevel,
            Exception ex,
            string methodName,
            string requestId)
        {
            LogMethodExecution(logger, operation, logLevel, ex, methodName, requestId, null, null);
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            LogLevel logLevel,
            Exception ex,
            string methodName,
            string requestId,
            string extraMessageTemplate,
            params object[] messageArgs)
        {
            if (messageArgs?.Length > 0 || !string.IsNullOrEmpty(extraMessageTemplate))
            {
                var args = MergeArgs(
                    messageArgs,
                    operation,
                    GetFullMethodName(typeof(T), methodName),
                    requestId);
                logger.Log(
                    logLevel,
                    ex,
                    "{Operation} method {FullMethodName}, RequestId: {RequestId}. " + extraMessageTemplate,
                    args);
            }
            else
            {
                logger.Log(
                    logLevel,
                    ex,
                    "{Operation} method {FullMethodName}, RequestId: {RequestId}. ",
                    operation,
                    GetFullMethodName(typeof(T), methodName),
                    requestId);
            }
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            InsightsEvent @event,
            string methodName)
        {
            LogMethodExecution(logger, operation, @event, null, methodName, null, null);
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            InsightsEvent @event,
            string methodName,
            string extraMessageTemplate,
            params object[] messageArgs)
        {
            LogMethodExecution(logger, operation, @event, null, methodName, extraMessageTemplate, messageArgs);
        }

        public static void LogMethodExecution<T>(
            this ILogger<T> logger,
            ExecutedOperation operation,
            InsightsEvent @event,
            Exception ex,
            string methodName,
            string extraMessageTemplate,
            params object[] messageArgs)
        {
            if (messageArgs?.Length > 0 || !string.IsNullOrEmpty(extraMessageTemplate))
            {
                var args = MergeArgs(
                    messageArgs,
                    operation,
                    GetFullMethodName(typeof(T),
                    methodName),
                    @event.cid);

                LogImportant(
                    logger,
                    @event,
                    ex,
                    "{Operation} method {FullMethodName}, RequestId: {RequestId}. " + extraMessageTemplate,
                    args);
            }
            else
            {
                LogImportant(
                    logger,
                    @event,
                    ex,
                    "{Operation} method {FullMethodName}, RequestId: {RequestId}. ",
                    operation,
                    GetFullMethodName(typeof(T), methodName),
                    @event.cid);
            }
        }

        public static void LogImportant(
            this ILogger logger,
            InsightsEvent @event)
        {
            logger.LogImportant(@event, null, null, null);
        }

        public static void LogImportant(
            this ILogger logger,
            InsightsEvent @event,
            string message,
            params object[] args)
        {
            logger.LogImportant(@event, null, message, args);
        }

        public static void LogImportant(
            this ILogger logger,
            InsightsEvent @event,
            Exception ex,
            string message, 
            params object[] args)
        {
            if (@event != null)
            {
                InsightsLogger.Post(@event);
            }
            
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (args == null)
            {
                args = Array.Empty<object>();
            }

            var logLevel = GetLogLevel(@event?.level);
            if (ex == null)
            {
                logger.Log(logLevel, message, args);
            }
            else
            {
                logger.Log(logLevel, ex, message, args);
            }
        }

        #region Private
        private static string GetFullMethodName(Type type, string methodName)
        {
            return type.Name + "." + methodName;
        }

        private static object[] MergeArgs(object[] currentArgs, params object[] args)
        {
            currentArgs ??= Array.Empty<object>();
            if (args == null || args.Length == 0)
            {
                return currentArgs;
            }

            var newArgs = new object[currentArgs.Length + args.Length];

            for (var i = 0; i < args.Length; i++)
            {
                newArgs[i] = args[i];
            }

            for (var i = 0; i < currentArgs.Length; i++)
            {
                newArgs[i + args.Length] = currentArgs[i];
            }

            return newArgs;
        }

        private static LogLevel GetLogLevel(string level)
        {
            var logLevel = LogLevel.Debug;
            if (level == null)
            {
                return logLevel;
            }
            else if (level.Equals(LogLevel.Trace.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Trace;
            }
            else if (level.Equals(LogLevel.Information.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Information;
            }
            else if (level.Equals(LogLevel.Warning.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Warning;
            }
            else if (level.Equals(LogLevel.Error.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Error;
            }
            else if (level.Equals(LogLevel.Critical.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Critical;
            }

            return logLevel;
        }
        #endregion Private
    }
}
