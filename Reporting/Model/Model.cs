using System.Collections.Generic;

namespace ent.manager.Reporting.Model
{
   

    public struct ReportGroupItemModel
    {

        public string name { get; set; }
        public int count { get; set; }
    }

    public class DeviceOsReportDataModel
    {
        public DeviceOsReportDataModel()
        {
            result = new List<ReportGroupItemModel>();
        }
        public List<ReportGroupItemModel> result { get; set; }

    }

    public class DeviceManufacturerReportDataModel
    {
        public DeviceManufacturerReportDataModel()
        {
            result = new List<ReportGroupItemModel>();
        }
        public List<ReportGroupItemModel> result { get; set; }

    }

    public class DeviceTypeReportDataModel
    {
        public DeviceTypeReportDataModel()
        {
            result = new List<ReportGroupItemModel>();
        }
        public List<ReportGroupItemModel> result { get; set; }

    }

}
