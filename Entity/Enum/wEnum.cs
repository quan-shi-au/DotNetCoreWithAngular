using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class wEnum : BaseEntity
    {
        public wEnum()
        {
        }

        public enum ReportProcessorRunStatus
        {
            failed = 0,
            success = 1,
            processing = 2
        }

        public enum LicencingEnvironmentEnum
        {
            Development = 1,
            Production = 2   
        }

        public enum ReportTypeEnum
        {
            SubscriptionReport = 1
        }

       

    }

    public static class EnumExtensions
    {
        public static String convertToString(this Enum eff)
        {
            return Enum.GetName(eff.GetType(), eff);
        }

        public static EnumType converToEnum<EnumType>(this String enumValue)
        {
            return (EnumType)Enum.Parse(typeof(EnumType), enumValue);
        }
    }
}
