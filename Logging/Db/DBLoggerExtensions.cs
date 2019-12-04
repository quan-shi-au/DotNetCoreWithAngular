using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ent.manager.Logging.Db;
using ent.manager.Logging.RollingFile;
using ent.manager.Logging.RollingFile.Internal;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions for adding the <see cref="FileLoggerProvider" /> to the <see cref="ILoggingBuilder" />
    /// </summary>
    public static class DBLoggerExtensions
    {
        public static ILoggingBuilder AddDBLog(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, DBLoggerProvider>();
            return builder;
        }

        public static ILoggerFactory AddDBLogger(this ILoggerFactory factory, string connectionStr, string logLevel)
        {
            factory.AddProvider(new DBLoggerProvider(connectionStr, logLevel, new BatchingLoggerOptions()));
            return factory;
        }

    }
}
