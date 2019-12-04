using ent.manager.Entity.Model;

namespace ent.manager.Reporting
{
    
     public interface IReportProcessor 
    {
        System.Threading.Tasks.Task<int> GenerateSubscriptionReportsAsync(Subscription sub, int reportProcessorRunId);
         void GenerateSubscriptionReportsTestAsync(Subscription sub);
    }
}
