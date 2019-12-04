using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ent.manager.WebApi.Helpers;

public class DataRefreshService : HostedService
{
    private readonly ReportProvider _reportProvider;
    private IConfigurationRoot _configuration;
    private ILogger<DataRefreshService> _logger;

    public DataRefreshService(ReportProvider reportProvider,ILogger<DataRefreshService> logger)
    {
        _reportProvider = reportProvider;
        _configuration = ent.manager.WebApi.Helpers.CommonHelper.GetConfigurationObject();
        _logger = logger;

    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _reportProvider.CallReportProcessor(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetLogText("DataRefreshService_ExecuteAsync"));

            }
            await Task.Delay(TimeSpan.FromMinutes(int.Parse(_configuration["ReportProcess:PeriodMinuteSpan"])), cancellationToken);
        }


    }
}