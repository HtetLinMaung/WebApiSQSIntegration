using Microsoft.AspNetCore.Mvc;
using WebApiSQSIntegration.Services;

namespace WebApiSQSIntegration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SqsController : ControllerBase
    {
        private readonly SqsService _sqsService;

        public SqsController(SqsService sqsService)
        {
            _sqsService = sqsService;
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            await _sqsService.SendMessageAsync("TestQueue", "Hello, SQS!");
            var messages = await _sqsService.ReceiveMessagesAsync("TestQueue");
            foreach (var message in messages)
            {
                Console.WriteLine($"Received message: {message.Body}");
                await _sqsService.DeleteMessageAsync("TestQueue", message.ReceiptHandle);
            }
            return Ok("Successful.");
        }
    }
}