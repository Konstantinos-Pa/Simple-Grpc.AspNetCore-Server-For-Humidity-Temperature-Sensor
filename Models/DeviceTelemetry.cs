namespace GrpcService.Models
{
    public class DeviceTelemetry
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime Timestamp {  get; set; }

        public string? DeviceId { get; set; }
        public DeviceInformation? DeviceInformation { get; set; }
    }
}
