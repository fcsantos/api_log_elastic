using APILoggingElastic.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace APILoggingElastic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateLog([FromBody] LogModel log)
        {
            // Criando um objeto anônimo para melhor estruturação do log
            var logData = new
            {
                Application = log.Application,
                Message = log.Message,
                AdditionalData = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    JsonSerializer.Serialize(log.AdditionalData)
                ),
                Timestamp = DateTime.UtcNow
            };

            switch (log.Level.ToLower())
            {
                case "information":
                    _logger.LogInformation("{@LogData}", logData);
                    break;
                case "warning":
                    _logger.LogWarning("{@LogData}", logData);
                    break;
                case "error":
                    _logger.LogError("{@LogData}", logData);
                    break;
                default:
                    _logger.LogInformation("{@LogData}", logData);
                    break;
            }

            return Ok();
        }
    }
}