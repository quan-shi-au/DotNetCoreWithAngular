using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ent.manager.Reporting;
using ent.manager.Services.Reporting.ReportProcessorRun;
using ent.manager.Services.Subscription;
using ent.manager.WebApi.Helpers;
using static ent.manager.Entity.Model.wEnum;

public class ReportProvider
{

    private IReportProcessorRunService _reportProcessorRunService;
    private ILogger<ReportProvider> _logger { get; set; }
    private ISubscriptionService _subscriptionService;
    private IReportProcessor _reportProcessor;
    private IConfigurationRoot _configuration;
    private bool _isApiTriggered;

    public ReportProvider(IReportProcessorRunService reportProcessorRunService,
        ILogger<ReportProvider> logger, 
        ISubscriptionService subscriptionService,
        IReportProcessor reportProcessor,bool IsApiTriggered = false)
    {
        _reportProcessorRunService = reportProcessorRunService;
        _subscriptionService = subscriptionService;
        _reportProcessor = reportProcessor;
        _logger = logger;
        _configuration = CommonHelper.GetConfigurationObject();
        _isApiTriggered = IsApiTriggered;
    }
    public async Task CallReportProcessor(CancellationToken cancellationToken)
    {

        //Log Current Run
        var logProcessorRunId = -1;
        logProcessorRunId = LogProcessorRun();

        try
        {

            if (logProcessorRunId != -1)
                {
                    //fetch & Fill
                    var reportsCount = await FillReportsAsync(logProcessorRunId);

                    // update log
                    if (!UpdateProcessorRun(logProcessorRunId, reportsCount, ReportProcessorRunStatus.success))
                    {
                        _logger.LogError("Reporting_Host_Failed to Updated Report Processor Run");
                    }
                }

        }
        catch (Exception ex)
        {
            
          _logger.LogError(ex.GetLogText("Reporting_Host_CallReportProcessor_Exception"));

            // update log
            if (!UpdateProcessorRun(logProcessorRunId, 0, ReportProcessorRunStatus.failed))
            {
                _logger.LogError("Reporting_Host_Failed to Updated Report Processor Run");
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Run Id: " + logProcessorRunId);
            sb.AppendLine("Report Processor Failed, Exception:");
            sb.AppendLine(ex.Message);

            EmailHelper.SendReportFailed(from: _configuration["entSenderEmail"],
                toCSL: _configuration["entReportFailedRecipientEmail"],
                subject: "Report Processor - Crashed",
                body: sb.ToString());
        }
    }


    private int LogProcessorRun()
    {
        var latestrun = _reportProcessorRunService.GetLatestRun();

        var reportProcessorRun = new ent.manager.Entity.Model.Reporting.ReportProcessorRun
                ()
        {
            StartRunTime = DateTime.UtcNow,
            ReportsCount = 0,
            Status = Convert.ToInt32(ReportProcessorRunStatus.processing)
        };

        if (latestrun == null)
        {
            _reportProcessorRunService.Add(reportProcessorRun);

            //log
            _logger.LogWarning("Reporting_Host_First Report Processor Run @ " + DateTime.UtcNow.ToString());

            return reportProcessorRun.Id;
        }
        else
        {
           
            if (latestrun.Status == Convert.ToInt32(ReportProcessorRunStatus.failed))
            {
                _reportProcessorRunService.Add(reportProcessorRun);

                _logger.LogWarning("Reporting_Host_After Failed_Report Processor Run @ " + DateTime.UtcNow.ToString());

                return reportProcessorRun.Id;
            }
            else if (latestrun.Status == Convert.ToInt32(ReportProcessorRunStatus.success))
            {
                if (_isApiTriggered)
                {
                    _reportProcessorRunService.Add(reportProcessorRun);

                    _logger.LogWarning("Reporting_Host - CALLED FROM API @ " + DateTime.UtcNow.ToString());

                    return reportProcessorRun.Id;
                }
                else
                {
                    var lastRunTime = latestrun.EndRunTime;

                    var timeDiff = DateTime.UtcNow.Subtract(lastRunTime.Value).TotalMinutes;

                    var configTimeDiff = int.Parse(_configuration["ReportProcess:PeriodMinuteSpan"]) * int.Parse(_configuration["ReportProcess:PeriodCountUntilFetch"]);

                    if (timeDiff >= configTimeDiff)
                    {
                        _reportProcessorRunService.Add(reportProcessorRun);

                        _logger.LogWarning("Reporting_Host_After Success_Report Processor Run @ " + DateTime.UtcNow.ToString());

                        return reportProcessorRun.Id;
                    }
                    else
                    {
                        _logger.LogWarning("Reporting_Host_After Success_NO Report Processor Run @ " + DateTime.UtcNow.ToString());

                        return -1;
                    }
                }

              

            }
            else //if (latestrun.Status == Convert.ToInt32(ReportProcessorRunStatus.processing))
            {

                var lastStartRunTime = latestrun.StartRunTime;

                var timeDiff = DateTime.UtcNow.Subtract(lastStartRunTime).TotalMinutes;

                // if the run is within its hour and this report process was tried to be executed, ignore and return -1, 
                // in order not to send this email redundandtly due to concurrent access to this process by load balanced instances

                if (timeDiff < 60) return -1;

                _logger.LogWarning("Reporting_Host_After Success_CurrentProcessing_NO Report Processor Run @ " + DateTime.UtcNow.ToString() );

                StringBuilder sb = new StringBuilder();
            
                sb.AppendLine("Report Processor Didnt Start, the current report processor run is still processing.");
                sb.AppendLine( "ReportProcessorRun.Id: " + latestrun.Id + ",  ReportProcessorRun.StartRunTime: " + latestrun.StartRunTime);

                EmailHelper.SendReportFailed(from: _configuration["entSenderEmail"],
                    toCSL: _configuration["entReportFailedRecipientEmail"],
                    subject: "manager Report Processor - Last Run didnt finish",
                    body: sb.ToString());

                return -1;

            }
        }
    }


    private async Task<int> FillReportsAsync(int logProcessorRunId)
    {

        var subscriptions = _subscriptionService.GetActive();

        var total = 0;
        var successsubReportsCount = 0;
        var failedSubReportsCount = 0;
        List<int> subIds = new List<int>();

        foreach (var item in subscriptions)
        {
            try
            {
                var reportcount = 0;

                // Report 1 - subscriptions report, whatever other reports we configure should be filled here, report types are in ReportType table
                reportcount = await _reportProcessor.GenerateSubscriptionReportsAsync(item, logProcessorRunId);

                total += reportcount;
                successsubReportsCount++;
            }
            catch  
            {
                _logger.LogError("Reporting_Host_PerSubscription_FillReportsAsync_ Processor Run @ " + DateTime.UtcNow.ToString() + " for subscription id: " + item.Id);
                subIds.Add(item.Id);
                failedSubReportsCount++;


            }
          
        }

        try
        {
            if(failedSubReportsCount > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Total Successful : " + successsubReportsCount);
                sb.AppendLine("Total Failed : " + failedSubReportsCount);
                sb.AppendLine("");
                sb.AppendLine("Failed Subscription Ids:");
                foreach (var item in subIds)
                {
                    sb.AppendLine("Id: " + item.ToString());
                }

                EmailHelper.SendReportFailed(from: _configuration["entSenderEmail"],
                    toCSL: _configuration["entReportFailedRecipientEmail"],
                    subject: "manager Report Processor - Failed to Generate For Subscriptions",
                    body: sb.ToString());
            }
        }
        catch 
        {}

        return total;
    }

    private bool UpdateProcessorRun(int Id, int reportsCounts, ReportProcessorRunStatus reportProcessorRunStatus)
    {
        try
        {
            var reportProcessorRunService = _reportProcessorRunService.GetById(Id);

            if (reportProcessorRunService != null)
            {

                reportProcessorRunService.EndRunTime = DateTime.UtcNow;
                reportProcessorRunService.ReportsCount = reportsCounts;
                reportProcessorRunService.Status =Convert.ToInt32(reportProcessorRunStatus);
                _reportProcessorRunService.Update(reportProcessorRunService);
            }
            return true;
        }
        catch  
        {

            
        }

        return false;
       
    }

  


}