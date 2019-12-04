using System;
using System.Collections.Generic;
using ent.manager.Entity.Model;
using ent.manager.Services.LicenceEnvironment;
using ent.manager.Services.Reporting.Report;
using ent.manager.Services.Subscription;
using ent.manager.Reporting.Model;
using Newtonsoft.Json;
using ent.manager.Services.User;
using System.Linq;
using ent.manager.Entity.Model.Reporting;
using Microsoft.Extensions.Logging;
using static ent.manager.Entity.Model.wEnum;
using ent.manager.Licence.Model;

namespace ent.manager.Reporting
{
    public class ReportProcessor : IReportProcessor
    {
        private ISubscriptionService _subscriptionService;
        private ILicenceEnvironmentService _licenceEnvironmentService;
        private IReportService _reportService;
        private IUsageReportService _usageReportService;
        private IDeviceOSReportService _deviceOSReportService;
        private ISeatDetailsReportService _seatDetailsReportService;
        private IUserDataService _userDataService;
        private IDeviceManufacturerReportService _deviceManufacturerReportService;
        private IDeviceTypeReportService _deviceTypeReportService;
        private int reportsGeneratedForThisSubscription = 0;
        private ILogger<ReportProcessor> _logger;


        public ReportProcessor(ISubscriptionService subscriptionService,
            ILicenceEnvironmentService licenceEnvironmentService, 
            IReportService reportService, 
            IUsageReportService usageReportService,
            IDeviceOSReportService deviceOSReportService,
            ISeatDetailsReportService seatDetailsReportService,
            IUserDataService userDataService,
            IDeviceManufacturerReportService deviceManufacturerReportService,
            IDeviceTypeReportService deviceTypeReportService,
             ILogger<ReportProcessor> logger)
        {
            _subscriptionService = subscriptionService;
            _licenceEnvironmentService = licenceEnvironmentService;
            _reportService = reportService;
            _usageReportService = usageReportService;
            _deviceOSReportService = deviceOSReportService;
            _seatDetailsReportService = seatDetailsReportService;
            _userDataService = userDataService;
            _deviceManufacturerReportService = deviceManufacturerReportService;
            _deviceTypeReportService = deviceTypeReportService;
            _logger = logger ;
        }


        public async System.Threading.Tasks.Task<int> GenerateSubscriptionReportsAsync(Subscription sub, int reportProcessorRunId)
        {
            reportsGeneratedForThisSubscription = 0;

            if (sub != null)
            {

                //Insert report record

                var report = _reportService.Add(new Entity.Model.Reporting.Report()
                {
                    SubscriptionId = sub.Id,
                    Type = (int)ReportTypeEnum.SubscriptionReport,
                    CreationTime = DateTime.UtcNow,
                    ReportProcessorRunId = reportProcessorRunId
                });

                // Usage Sub Report
                var licenceDetailsObj = await FillUsageReportAsync(sub, report.Id);
                reportsGeneratedForThisSubscription++;

                //DeviceOsReport + DevManufacturerReport + DeviceTypeReport + SeatDetailsReport
                ExtractLicenceObjectAndFillReports(licenceDetailsObj, report.Id);

                //Update report record
                report.CompletionTime = DateTime.UtcNow;
                _reportService.Update(report);

            }

            return reportsGeneratedForThisSubscription;
        }

        
        private async System.Threading.Tasks.Task<Licence.Model.SafeCentralLicenceDetail> FillUsageReportAsync(Subscription subscription, int reportId)
        {


            var le = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment);

            
                var seatsUsed = 0;
                var seatsAvailable = 0;

                 if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Development)
                {
                    manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: le.CoreURL,
                    username: subscription.CoreAuthUsername,
                    password: subscription.CoreAuthPassword,
                    brandid: subscription.BrandId,
                    campaignid: subscription.Campaign,
                    logger: _logger);

                   var result = await lsxDevCore.GetLicenceDetailsAsync(subscription.LicenceKey);


                    seatsUsed = result.SeatsUsed;
                    seatsAvailable = result.SeatsAvailable;

                    _usageReportService.Add(new Entity.Model.Reporting.UsageReport() { Available = seatsAvailable, Used = seatsUsed, CreationTime = DateTime.UtcNow, ReportId = reportId });

                    return result;

                }
                else if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Production)
                {
                    manager.Licence.Proxy.Ls1Core Ls1Core = new manager.Licence.Proxy.Ls1Core(url: le.CoreURL,
                   username: subscription.CoreAuthUsername,
                   password: subscription.CoreAuthPassword,
                   brandid: subscription.BrandId,
                   campaignid: subscription.Campaign,
                   logger: _logger);

                   var result = await Ls1Core.GetLicenceDetailsAsync(subscription.LicenceKey);

                    seatsUsed = result.SeatsUsed;
                    seatsAvailable = result.SeatsAvailable;

                    _usageReportService.Add(new Entity.Model.Reporting.UsageReport() { Available = seatsAvailable, Used = seatsUsed, CreationTime = DateTime.UtcNow, ReportId = reportId });

                    return result;
                }

             

            //if code reaches here something went wrong
            return null;

        }

        private bool ExtractLicenceObjectAndFillReports(Licence.Model.SafeCentralLicenceDetail licenceObj, int reportId)
        {
             
                Dictionary<string, int> deviceOsDictionary = new Dictionary<string, int>();
                Dictionary<string, int> deviceManuFacturerDictionary = new Dictionary<string, int>();
                Dictionary<string, int> deviceTypeDictionary = new Dictionary<string, int>();

                DeviceOsReportDataModel deviceOsReportModel = new DeviceOsReportDataModel();
                DeviceManufacturerReportDataModel deviceManufacturerReportModel = new DeviceManufacturerReportDataModel();
                DeviceTypeReportDataModel deviceTypeReportModel = new DeviceTypeReportDataModel();

                var userDataEnumerable = _userDataService.GetAll();
                var isReportCounted = false;
                if (licenceObj != null)
                {
                    foreach (var item in licenceObj.ComputersCurrentlyInstalled)
                    {

                        SeatDetailsReport seatDetailsReportLine = new SeatDetailsReport();
                    
                        //gather device OS data for - DeviceOsReport
                        if (deviceOsDictionary.ContainsKey(item.OperatingSystem))
                        {
                            deviceOsDictionary[item.OperatingSystem] += 1;
                        }
                        else
                        {
                            deviceOsDictionary.Add(item.OperatingSystem, 1);
                        }

                        if(item.Seats.Count > 0 )
                        {
                        UserData userDataRecord = new UserData(); ;
                        LicenceProductComputerSeat activeseat = new LicenceProductComputerSeat(); ;

                        var activeseats = item.Seats.Where(x => x.IsBlocked == false).OrderByDescending(x=>x.ActivationDate);
                          
                            foreach (var licenceActiveSeat in activeseats)
                            {
                                 var _tmp = userDataEnumerable.Where(x => x.SeatKey == licenceActiveSeat.SeatKey).OrderByDescending(x => x.CreationTime).FirstOrDefault();

                                     if(_tmp != null)
                                    {
                                        userDataRecord = _tmp;
                                        activeseat = licenceActiveSeat;
                                    }

                            }

                            if(!string.IsNullOrEmpty(activeseat.SeatKey))
                            {

                         

                                // the following can be gathered from the licence details
                                seatDetailsReportLine.ActivationDate = activeseat.ActivationDate;
                                seatDetailsReportLine.LastUpdateDate = activeseat.LastUpdate;
                                seatDetailsReportLine.ReportId = reportId;
                                seatDetailsReportLine.CreationTime = DateTime.UtcNow;
                                seatDetailsReportLine.OsVersion = item.OperatingSystem;
                                seatDetailsReportLine.DeviceName = item.Name;
                                seatDetailsReportLine.SeatKey = activeseat.SeatKey;

                                if (!string.IsNullOrEmpty(userDataRecord.SeatKey)) //corresponding user data found
                                {
                                    //gather device mode data for - DevManufacturerReport
                                    if (deviceManuFacturerDictionary.ContainsKey(userDataRecord.DeviceModel))
                                    {
                                        deviceManuFacturerDictionary[userDataRecord.DeviceModel] += 1;
                                    }
                                    else
                                    {
                                        deviceManuFacturerDictionary.Add(userDataRecord.DeviceModel, 1);
                                    }

                                    //gather device type data for - DeviceTypeReport
                                    if (deviceTypeDictionary.ContainsKey(userDataRecord.DeviceType))
                                    {
                                        deviceTypeDictionary[userDataRecord.DeviceType] += 1;
                                    }
                                    else
                                    {
                                        deviceTypeDictionary.Add(userDataRecord.DeviceType, 1);
                                    }

                                    seatDetailsReportLine.DeviceType = userDataRecord.DeviceType;
                                    seatDetailsReportLine.FirstName = Convert.ToBase64String(userDataRecord.FirstName);
                                    seatDetailsReportLine.LastName = Convert.ToBase64String(userDataRecord.LastName);
                                    seatDetailsReportLine.DeviceModel = userDataRecord.DeviceModel;
                                    seatDetailsReportLine.OptionalData = userDataRecord.OptionalData == null? string.Empty : Convert.ToBase64String(userDataRecord.OptionalData);



                            }
                            else // corresponding user data not found
                                {

                                    seatDetailsReportLine.DeviceType = "";
                                    seatDetailsReportLine.FirstName = "";
                                    seatDetailsReportLine.LastName = "";
                                    seatDetailsReportLine.DeviceModel = "";
                                    seatDetailsReportLine.OptionalData = "";

                                }

                                // insert seat details report record
                                _seatDetailsReportService.Add(seatDetailsReportLine);

                                if (!isReportCounted) { reportsGeneratedForThisSubscription++; isReportCounted = true; }


                            }

                        }

                    }

                }

                //Insert Device OS Report
                foreach (var item in deviceOsDictionary)
                {

                    deviceOsReportModel.result.Add(new ReportGroupItemModel() { name = item.Key, count = item.Value });

                }

                var deviceosdata = JsonConvert.SerializeObject(deviceOsReportModel.result);

                _deviceOSReportService.Add(new Entity.Model.Reporting.DeviceOsReport() { CreationTime = DateTime.UtcNow, ReportId = reportId, Data = deviceosdata });
                reportsGeneratedForThisSubscription++;

                //Insert Device Manufacturer Report
                foreach (var item in deviceManuFacturerDictionary)
                {

                    deviceManufacturerReportModel.result.Add(new ReportGroupItemModel() { name = item.Key, count = item.Value });

                }

                var devicemanufacturerdata = JsonConvert.SerializeObject(deviceManufacturerReportModel.result);

                _deviceManufacturerReportService.Add(new Entity.Model.Reporting.DeviceManufacturerReport() { CreationTime = DateTime.UtcNow, ReportId = reportId, Data = devicemanufacturerdata });
                reportsGeneratedForThisSubscription++;

                //Insert Device Type Report
                foreach (var item in deviceTypeDictionary)
                {

                    deviceTypeReportModel.result.Add(new ReportGroupItemModel() { name = item.Key, count = item.Value });

                }

                var devicetypedata = JsonConvert.SerializeObject(deviceTypeReportModel.result);

                _deviceTypeReportService.Add(new Entity.Model.Reporting.DeviceTypeReport() { CreationTime = DateTime.UtcNow, ReportId = reportId, Data = devicetypedata });
                reportsGeneratedForThisSubscription++;
                
           

            return true;

        }

        #region Test
        public async void GenerateSubscriptionReportsTestAsync(Subscription sub)
        {


            if (sub != null)
            {


                // Usage Sub Report
                var licenceDetailsObj = await FillUsageReportTestAsync(sub);
                reportsGeneratedForThisSubscription++;

                //DeviceOsReport + DevManufacturerReport + DeviceTypeReport + SeatDetailsReport
                ExtractLicenceObjectAndFillReportsTest(licenceDetailsObj);

         

            }

           
        }

        private async System.Threading.Tasks.Task<Licence.Model.SafeCentralLicenceDetail> FillUsageReportTestAsync(Subscription subscription)
        {


            var le = _licenceEnvironmentService.GetBymanagerId(subscription.LicencingEnvironment);


            var seatsUsed = 0;
            var seatsAvailable = 0;

            if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Development)
            {
                manager.Licence.Proxy.LsxDevCore lsxDevCore = new manager.Licence.Proxy.LsxDevCore(url: le.CoreURL,
                username: subscription.CoreAuthUsername,
                password: subscription.CoreAuthPassword,
                brandid: subscription.BrandId,
                campaignid: subscription.Campaign,
                logger: _logger);

                var result = await lsxDevCore.GetLicenceDetailsAsync(subscription.LicenceKey);


                seatsUsed = result.SeatsUsed;
                seatsAvailable = result.SeatsAvailable;

                //_usageReportService.Add(new Entity.Model.Reporting.UsageReport() { Available = seatsAvailable, Used = seatsUsed, CreationTime = DateTime.UtcNow, ReportId = reportId });

                return result;

            }
            else if (subscription.LicencingEnvironment == (int)LicencingEnvironmentEnum.Production)
            {
                manager.Licence.Proxy.Ls1Core Ls1Core = new manager.Licence.Proxy.Ls1Core(url: le.CoreURL,
               username: subscription.CoreAuthUsername,
               password: subscription.CoreAuthPassword,
               brandid: subscription.BrandId,
               campaignid: subscription.Campaign,
               logger: _logger);

                var result = await Ls1Core.GetLicenceDetailsAsync(subscription.LicenceKey);

                seatsUsed = result.SeatsUsed;
                seatsAvailable = result.SeatsAvailable;

                //_usageReportService.Add(new Entity.Model.Reporting.UsageReport() { Available = seatsAvailable, Used = seatsUsed, CreationTime = DateTime.UtcNow, ReportId = reportId });

                return result;
            }



            //if code reaches here something went wrong
            return null;

        }

        private bool ExtractLicenceObjectAndFillReportsTest(Licence.Model.SafeCentralLicenceDetail licenceObj)
        {

            Dictionary<string, int> deviceOsDictionary = new Dictionary<string, int>();
            Dictionary<string, int> deviceManuFacturerDictionary = new Dictionary<string, int>();
            Dictionary<string, int> deviceTypeDictionary = new Dictionary<string, int>();

            DeviceOsReportDataModel deviceOsReportModel = new DeviceOsReportDataModel();
            DeviceManufacturerReportDataModel deviceManufacturerReportModel = new DeviceManufacturerReportDataModel();
            DeviceTypeReportDataModel deviceTypeReportModel = new DeviceTypeReportDataModel();

            var userDataEnumerable = _userDataService.GetAll();
            var isReportCounted = false;
            if (licenceObj != null)
            {
                foreach (var item in licenceObj.ComputersCurrentlyInstalled)
                {

                    SeatDetailsReport seatDetailsReportLine = new SeatDetailsReport();

                    //gather device OS data for - DeviceOsReport
                    if (deviceOsDictionary.ContainsKey(item.OperatingSystem))
                    {
                        deviceOsDictionary[item.OperatingSystem] += 1;
                    }
                    else
                    {
                        deviceOsDictionary.Add(item.OperatingSystem, 1);
                    }

                    if (item.Seats.Count > 0)
                    {
                        UserData userDataRecord = new UserData(); ;
                        LicenceProductComputerSeat activeseat = new LicenceProductComputerSeat(); ;

                        var activeseats = item.Seats.Where(x => x.IsBlocked == false).OrderByDescending(x => x.ActivationDate);

                        foreach (var licenceActiveSeat in activeseats)
                        {
                            var _tmp = userDataEnumerable.Where(x => x.SeatKey == licenceActiveSeat.SeatKey).OrderByDescending(x => x.CreationTime).FirstOrDefault();

                            if (_tmp != null)
                            {
                                userDataRecord = _tmp;
                                activeseat = licenceActiveSeat;
                            }

                        }

                        if (!string.IsNullOrEmpty(activeseat.SeatKey))
                        {



                            // the following can be gathered from the licence details
                            seatDetailsReportLine.ActivationDate = activeseat.ActivationDate;
                            seatDetailsReportLine.LastUpdateDate = activeseat.LastUpdate;
                            seatDetailsReportLine.ReportId = -1;
                            seatDetailsReportLine.CreationTime = DateTime.UtcNow;
                            seatDetailsReportLine.OsVersion = item.OperatingSystem;
                            seatDetailsReportLine.DeviceName = item.Name;
                            seatDetailsReportLine.SeatKey = activeseat.SeatKey;

                            if (!string.IsNullOrEmpty(userDataRecord.SeatKey)) //corresponding user data found
                            {
                                //gather device mode data for - DevManufacturerReport
                                if (deviceManuFacturerDictionary.ContainsKey(userDataRecord.DeviceModel))
                                {
                                    deviceManuFacturerDictionary[userDataRecord.DeviceModel] += 1;
                                }
                                else
                                {
                                    deviceManuFacturerDictionary.Add(userDataRecord.DeviceModel, 1);
                                }

                                //gather device type data for - DeviceTypeReport
                                if (deviceTypeDictionary.ContainsKey(userDataRecord.DeviceType))
                                {
                                    deviceTypeDictionary[userDataRecord.DeviceType] += 1;
                                }
                                else
                                {
                                    deviceTypeDictionary.Add(userDataRecord.DeviceType, 1);
                                }

                                seatDetailsReportLine.DeviceType = userDataRecord.DeviceType;
                                seatDetailsReportLine.FirstName = Convert.ToBase64String(userDataRecord.FirstName);
                                seatDetailsReportLine.LastName = Convert.ToBase64String(userDataRecord.LastName);
                                seatDetailsReportLine.DeviceModel = userDataRecord.DeviceModel;
                                seatDetailsReportLine.OptionalData = userDataRecord.OptionalData == null ? string.Empty : Convert.ToBase64String(userDataRecord.OptionalData);



                            }
                            else // corresponding user data not found
                            {

                                seatDetailsReportLine.DeviceType = "";
                                seatDetailsReportLine.FirstName = "";
                                seatDetailsReportLine.LastName = "";
                                seatDetailsReportLine.DeviceModel = "";
                                seatDetailsReportLine.OptionalData = "";

                            }

                            // insert seat details report record
                            //_seatDetailsReportService.Add(seatDetailsReportLine);

                            //if (!isReportCounted) { reportsGeneratedForThisSubscription++; isReportCounted = true; }


                        }

                    }

                }

            }

            //Insert Device OS Report
            foreach (var item in deviceOsDictionary)
            {

                deviceOsReportModel.result.Add(new ReportGroupItemModel() { name = item.Key, count = item.Value });

            }

            var deviceosdata = JsonConvert.SerializeObject(deviceOsReportModel.result);

            //_deviceOSReportService.Add(new Entity.Model.Reporting.DeviceOsReport() { CreationTime = DateTime.UtcNow, ReportId = -1, Data = deviceosdata });
            reportsGeneratedForThisSubscription++;

            //Insert Device Manufacturer Report
            foreach (var item in deviceManuFacturerDictionary)
            {

                deviceManufacturerReportModel.result.Add(new ReportGroupItemModel() { name = item.Key, count = item.Value });

            }

            var devicemanufacturerdata = JsonConvert.SerializeObject(deviceManufacturerReportModel.result);

            //_deviceManufacturerReportService.Add(new Entity.Model.Reporting.DeviceManufacturerReport() { CreationTime = DateTime.UtcNow, ReportId = reportId, Data = devicemanufacturerdata });
            reportsGeneratedForThisSubscription++;

            //Insert Device Type Report
            foreach (var item in deviceTypeDictionary)
            {

                deviceTypeReportModel.result.Add(new ReportGroupItemModel() { name = item.Key, count = item.Value });

            }

            var devicetypedata = JsonConvert.SerializeObject(deviceTypeReportModel.result);

            //_deviceTypeReportService.Add(new Entity.Model.Reporting.DeviceTypeReport() { CreationTime = DateTime.UtcNow, ReportId = reportId, Data = devicetypedata });
            reportsGeneratedForThisSubscription++;



            return true;

        }
        #endregion

    }
}
