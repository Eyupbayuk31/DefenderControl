namespace DefenderControl.Models;

public class DefenderStatus
{
    public bool IsRealTimeProtectionEnabled { get; set; }
    public bool IsIoavProtectionEnabled { get; set; }
    public bool IsBehaviorMonitorEnabled { get; set; }
    public bool IsAntivirusEnabled { get; set; }
    public string? LastScanTime { get; set; }
    public string? AntivirusSignatureVersion { get; set; }
    public string? AntispywareSignatureVersion { get; set; }
    public string? EngineVersion { get; set; }
}