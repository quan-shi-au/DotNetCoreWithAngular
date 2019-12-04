using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ent.manager.WebApi.Results
{

    public class GetEventsResult
    {
        public string s { get; set; }
        public string c { get; set; }
        public Event[] d { get; set; }


    }

    public class Event
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string EventSubType { get; set; }
        public string EventDate { get; set; }
        public string Description { get; set; }
    }

    //Deivce Information
    public class DeviceInformationResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public IDeviceInformation deviceInformation { get; set; }

    }

    public interface IDeviceInformation
    {

    }

    public class IOSDeviceInformation : IDeviceInformation
    {
        public string DeviceName { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceType { get; set; }
        public string OperatingSystem { get; set; }
        public string Architecture { get; set; }
        public string Memory { get; set; }

    }

    public class AndroidDeviceInformation : IDeviceInformation
    {
        public string DeviceName { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceType { get; set; }
        public string OperatingSystem { get; set; }
        public string Architecture { get; set; }
        public string Memory { get; set; }

    }

    public class WinDeviceInformation : IDeviceInformation
    {
        public string DeviceName { get; set; }
        public string OperatingSystem { get; set; }
        public string Architecture { get; set; }
        public string Memory { get; set; }
        public string DisplayMemory { get; set; }

    }

    public class MacDeviceInformation : IDeviceInformation
    {
        public string DeviceName { get; set; }
        public string OperatingSystem { get; set; }
        public string Architecture { get; set; }
        public string Memory { get; set; }

    }

    //Web Protect Result
    public class WebProtectResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public WebProtectStatus webProtectStatus { get; set; } //0 OFF - 1 ON

    }

    public class WebProtectStatus
    {
        public string Status { get; set; }
    }

    //Real Time Protection Event
    public class RealTimeProtectionEventResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public RealTimeProtectionEvent realTimeProtectionEvent { get; set; } //0 OFF - 1 ON

    }

    public class RealTimeProtectionEvent
    {
        public string Status { get; set; }
    }

    //Firewall Event
    public class FirewallEventResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public FirewallEvent firewallEvent { get; set; } //0 OFF - 1 ON

    }

    public class FirewallEvent
    {
        public string Status { get; set; }
    }

    //Firewall Policy Event
    public class FirewallPolicyEventResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public FirewallPolicyEvent firewallPolicyEvent { get; set; } //0 OFF - 1 ON

    }

    public class FirewallPolicyEvent
    {
        public string Status { get; set; }
    }

    //Device Health Result
    public class DeviceHealthResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public DeviceHealth deviceHealth { get; set; } //0 OFF - 1 ON

    }

    public class DeviceHealth
    {
        public string StatusBarColour { get; set; }
        public string StatusDescription { get; set; }
    }

    //Scan Summary
    public class ScanSummaryResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public List<IScanSummaryResult> scanSummaryResult { get; set; }

        public ScanSummaryResult()
        {
            scanSummaryResult = new List<IScanSummaryResult>();
        }

    }


    public interface IScanSummaryResult
    {

    }

    public class IOSScanSummaryResult : IScanSummaryResult
    {
        public string ScanType { get; set; }
        public string ScanStartDateTime { get; set; }
        public string ScanEndDateTime { get; set; }
        public string StartupStatus { get; set; }
        public string InternetStatus { get; set; }
        public string PasscodeStatus { get; set; }
    }

    public class AndroidScanSummaryResult : IScanSummaryResult
    {
        public string ScanType { get; set; }
        public string ScanStartDateTime { get; set; }
        public string ScanEndDateTime { get; set; }
        public string ScanDuration { get; set; }
        public string AppScanCount { get; set; }
        public string MaliciousAppCount { get; set; }

    }

    public class WinScanSummaryResult : IScanSummaryResult
    {
        public string ScanType { get; set; }
        public string ScanStartDateTime { get; set; }
        public string ScanEndDateTime { get; set; }
        public string ScanDuration { get; set; }
        public string FileScanCount { get; set; }
        public string MaliciousItemCount { get; set; }

    }

    public class MacScanSummaryResult : IScanSummaryResult
    {
        public string ScanType { get; set; }
        public string ScanStartDateTime { get; set; }
        public string ScanEndDateTime { get; set; }
        public string ScanDuration { get; set; }
        public string FileScanCount { get; set; }
        public string MaliciousItemCount { get; set; }
        public string ScanDate { get; set; }
        public string ScanDurationMinutes { get; set; }

    }

    //Malware Remediation Event
    public class MalwareRemediationEventResult
    {
        public int result { get; set; } //-1 failed //0 no information //1 success
        public List<IMalwareRemediationEvent> malwareRemediationEventResult { get; set; }

        public MalwareRemediationEventResult()
        {
            malwareRemediationEventResult = new List<IMalwareRemediationEvent>();
        }

    }

    public interface IMalwareRemediationEvent
    {



    }

    public class WinMalwareRemediationEventResult : IMalwareRemediationEvent
    {
        public string MalwareNameType { get; set; }
        public string InfectedItem { get; set; }
        public string RemediationAction { get; set; }


    }

    public class MacMalwareRemediationEventResult : IMalwareRemediationEvent
    {
        public string MalwareNameType { get; set; }
        public string InfectedItem { get; set; }
        public string RemediationAction { get; set; }


    }


    //Malware Detection Events
    public class MalwareDetectionEventResult
    {

        public int result { get; set; } //-1 failed //0 no information //1 success
        public List<IMalwareDetectionEvent> malwareDetectionEvents { get; set; } //0 OFF - 1 ON

        public MalwareDetectionEventResult()
        {
            malwareDetectionEvents = new List<IMalwareDetectionEvent>();
        }

    }

    public class IMalwareDetectionEvent
    {

    }

    public class AndroidMalwareDetectionEvent : IMalwareDetectionEvent
    {
        public string MalwareNameType { get; set; }
        public string InfectedItem { get; set; }
        public string eventDate{ get; set; }
    }

    public class WinMalwareDetectionEvent : IMalwareDetectionEvent
    {
        public string MalwareNameType { get; set; }
        public string InfectedItem { get; set; }
        public string eventDate { get; set; }
    }

    public class MacMalwareDetectionEvent : IMalwareDetectionEvent
    {
        public string MalwareNameType { get; set; }
        public string InfectedItem { get; set; }
        public string eventDate { get; set; }
    }

    //Secure Apps Events
    public class SecureAppsEventResult
    {

        public int result { get; set; } //-1 failed //0 no information //1 success
        public List<SecureAppsEvent> secureAppsEvents { get; set; } //0 OFF - 1 ON

        public SecureAppsEventResult()
        {
            secureAppsEvents = new List<SecureAppsEvent>();

        }

    }

    public class SecureAppsEvent
    {
        public string Status { get; set; }
        public string AppName { get; set; }
        public string eventDate { get; set; }
    }


}
