namespace GrpcService.Models
{
    public class Commands
    {
        public int Id { get; set; }
        public string? Command { get; set; }
        public string? Parameters { get; set; }

        public string? DeviceId { get; set; }
        public DeviceInformation? DeviceInformation { get; set; }
    }
}
