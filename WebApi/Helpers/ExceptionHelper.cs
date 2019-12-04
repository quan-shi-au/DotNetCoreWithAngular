using System;
using System.Text;

namespace ent.manager.WebApi.Helpers
{
    public static class ExceptionHelper
    {

        public static string GetLogText(this Exception ex, string exceptionlocaiton)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("managerLOC: " +exceptionlocaiton);
            if (ex.Message != null) sb.AppendLine("Message: " + ex.Message);
            if (ex.StackTrace != null) sb.AppendLine("Stack Trace" + ex.StackTrace);
            return sb.ToString();
        }

    }
}
