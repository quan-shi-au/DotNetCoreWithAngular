using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ent.manager.Data.Context;
using ent.manager.Entity.Model;
using ent.manager.Logging.RollingFile.Internal;

namespace ent.manager.Logging.Db
{
    public class DBLoggerProvider : ILoggerProvider
    {
        readonly List<Log> _currentBatch = new List<Log>();
        readonly TimeSpan _interval;
        readonly int? _queueSize;
        readonly int? _batchSize;
        readonly managerDbContext _managerDbContext;

        BlockingCollection<Log> _messageQueue;
        Task _outputTask;
        CancellationTokenSource _cancellationTokenSource;

        readonly LogLevel _logLevel;

        public DBLoggerProvider(string connectionString, string logLevel, BatchingLoggerOptions loggerOptions)
        {
            if (!Enum.TryParse(logLevel, out _logLevel))
                _logLevel = LogLevel.Warning;

            var optionsBuilder = new DbContextOptionsBuilder<managerDbContext>();
            optionsBuilder.UseMySQL(connectionString);
            _managerDbContext = new managerDbContext(optionsBuilder.Options);

            if (loggerOptions.BatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.BatchSize), $"{nameof(loggerOptions.BatchSize)} must be a positive number.");

            if (loggerOptions.FlushPeriod <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(loggerOptions.FlushPeriod), $"{nameof(loggerOptions.FlushPeriod)} must be longer than zero.");

            _interval = loggerOptions.FlushPeriod;
            _batchSize = loggerOptions.BatchSize;
            _queueSize = loggerOptions.BackgroundQueueSize;

            Start();

        }

        Task WriteMessageAsync(IEnumerable<Log> messages, CancellationToken token)
        {
            _managerDbContext.Log.AddRangeAsync(messages);

            return _managerDbContext.SaveChangesAsync();

        }

        async Task ProcessLogQueue(object state)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var limit = _batchSize ?? int.MaxValue;

                while (limit > 0 && _messageQueue.TryTake(out var message))
                {
                    _currentBatch.Add(message);
                    limit--;
                }

                if (_currentBatch.Count > 0)
                {
                    try
                    {
                        await WriteMessageAsync(_currentBatch, _cancellationTokenSource.Token);
                    }
                    catch
                    {
                        // ignored
                    }

                    _currentBatch.Clear();
                }

                await IntervalAsync(_interval, _cancellationTokenSource.Token);
            }
        }


        Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            return Task.Delay(interval, cancellationToken);
        }

        internal void AddMessage(Log log)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(log, _cancellationTokenSource.Token);
                }
                catch
                {
                    // cancellation token called or CompleteAdding called
                }
            }
        }

        void Start()
        {
            _messageQueue = _queueSize == null ?
                new BlockingCollection<Log>(new ConcurrentQueue<Log>()) :
                new BlockingCollection<Log>(new ConcurrentQueue<Log>(), _queueSize.Value);

            _cancellationTokenSource = new CancellationTokenSource();

            _outputTask = Task.Factory.StartNew<Task>(
                ProcessLogQueue,
                null,
                TaskCreationOptions.LongRunning
                );
        }

        void Stop()
        {
            _cancellationTokenSource.Cancel();
            _messageQueue.CompleteAdding();

            try
            {
                _outputTask.Wait(_interval);
            }
            catch (TaskCanceledException)
            {

            }
            catch(AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            {

            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DbLogger(this, categoryName, _logLevel);
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource.Dispose();
            _managerDbContext.Dispose();
        }

    }
}