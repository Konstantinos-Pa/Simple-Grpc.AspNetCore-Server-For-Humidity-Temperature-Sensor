using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcService.Services
{
    public class IoTSensorService : IoTSensor.IoTSensorBase
    {
        private readonly ILogger<IoTSensorService> _logger;
        private readonly ApplicationDbContext _context;
        public IoTSensorService(ILogger<IoTSensorService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async override Task<RegisterResponse> RegisterDevice(DeviceInfo request, ServerCallContext context)
        {
            if (request is null)
            {
                return new RegisterResponse { Success = false, Message = "Device Information Null" };
            }
            if (request.DeviceId == null)
            {
                return new RegisterResponse { Success = false, Message = "Device Id null" };
            }
            var device = new DeviceInformation
            {
                DeviceId = request.DeviceId,
                DeviceType = request.DeviceType,
                Location = request.Location
            };

            if (await _context.DeviceInformation.AnyAsync(d => d.DeviceId == request.DeviceId))
            {
                return new RegisterResponse { Success = false, Message = "Device already registered" };
            }

            try
            {
                await _context.DeviceInformation.AddAsync(device);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error writing to DB.");
                return new RegisterResponse { Success = false, Message = ex.Message };
            }

            return new RegisterResponse { Success = true, Message = "Device Information Registered" };
        }

        public async override Task<Ack> SendTelemetry(IAsyncStreamReader<TelemetryEvent> requestStream, ServerCallContext context)
        {
            var batch = new List<DeviceTelemetry>();
            await foreach (var message in requestStream.ReadAllAsync())
            {
                if (message.DeviceId == null)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "DeviceId is required"));
                }
                var telemetry = new DeviceTelemetry
                {
                    Temperature = message.Temperature,
                    Humidity = message.Humidity,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).UtcDateTime,
                    DeviceId = message.DeviceId
                };
                batch.Add(telemetry);

                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (batch.Count >= 50) // batch size
                {
                    try
                    {
                        await _context.DeviceTelemetry.AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError(ex, "Error writing to DB.");
                        throw new RpcException(new Status(StatusCode.Internal, $"DB insert failed: {ex.Message}"));
                    }
                    batch.Clear();
                }
            }

            // Save remaining records
            if (batch.Count > 0)
            {
                try
                {
                    await _context.DeviceTelemetry.AddRangeAsync(batch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error writing to DB.");
                    return new Ack { Success = false, Message = ex.Message };
                }
            }
            return new Ack { Success = true, Message = "Saved all Telemetry Successfully" };
        }
        public async override Task WatchCommands(DeviceId request, IServerStreamWriter<DeviceCommand> responseStream, ServerCallContext context)
        {
            if (request is null)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Id is null"));
            }
            if (responseStream == null)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Stream is null"));
            }
            List<Commands> commands = new List<Commands>();
            try
            {
                commands = await _context.Commands.Where(i => i.DeviceId == request.DeviceId_).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            for (int i = 0; i < commands.Count; i++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    // Stop sending
                    break;
                }
                var deviceCommand = new DeviceCommand
                {
                    Command = commands[i].Command,
                    Parameters = commands[i].Parameters,
                };
                try
                {
                    await responseStream.WriteAsync(deviceCommand);
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    // Client disconnected, stop streaming
                    _logger.LogInformation("Client disconnected, stopping stream.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to gRPC stream.");
                    throw;
                }
            }
        }
        public override async Task PushFirmware(DeviceId request, IServerStreamWriter<FirmwareRequest> responseStream, ServerCallContext context)
        {
            await using var fs = File.OpenRead("firmware_v1.2.bin");
            int chunkSize = 1024 * 1024; // 1 MB
            var buffer = new byte[chunkSize];
            int bytesRead;
            while ((bytesRead = await fs.ReadAsync(buffer, context.CancellationToken)) > 0)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    await responseStream.WriteAsync(new FirmwareRequest
                    {
                        FirmwareData = ByteString.CopyFrom(buffer, 0, bytesRead),
                        Version = "1.2"
                    });
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    // Client disconnected, stop streaming
                    _logger.LogInformation("Client disconnected, stopping stream.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to gRPC stream.");
                    throw;
                }
            }
        }

        public async override Task<Ack> GetDeviceStatus(DeviceStatus request, ServerCallContext context)
        {
            _logger.LogInformation(
            "Device {DeviceId} has battery level {BatteryLevel}, is {Status} and last error was {Error}.",
            request.DeviceId,
            request.BatteryLevel,
            request.Online ? "Online" : "Offline",
            request.LastError);
            return new Ack { Success = true, Message = "Status Logged" };
        }
    }
}
