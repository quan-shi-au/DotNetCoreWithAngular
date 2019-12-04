using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ent.manager.Data.Context;
using ent.manager.Entity.Model;
using ent.manager.Logging.Db;

namespace ent.manager.Logging.RollingFile.Internal
{
    public class DbLogger: ILogger
    {
        readonly string _category;
        readonly LogLevel _logLevel;
        readonly DBLoggerProvider _dbLoggerProvider;

        public DbLogger(DBLoggerProvider dbLoggerProvider, string categoryName, LogLevel logLevel)
        {
            _dbLoggerProvider = dbLoggerProvider;
            _category = categoryName;
            _logLevel = logLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if ((int)logLevel < (int)_logLevel)
                return false;

            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            _dbLoggerProvider.AddMessage(new Entity.Model.Log
            {
                Category = _category,
                Level = logLevel.ToString(),
                CreationTime = DateTime.UtcNow,
                Message = formatter(state, exception)
            });

        }
    }
}
