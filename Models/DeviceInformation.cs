namespace GrpcService.Models
{
    public class DeviceInformation
    {
        public string? DeviceId { get; set; }
        public string? DeviceType { get; set; }
        public string? Location { get; set; }
        public bool Online { get; set; }

        public ICollection<Commands>? Commands { get; set; }

        public ICollection<DeviceTelemetry>? DeviceTelemetry { get; set; }
    }
}
