using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Mvc;

namespace Provider.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IEventPublisher _eventPublisher;

        private readonly EventHubProducerClient _producerClient;

        public StudentsController(
            IStudentRepository studentRepository,
            IEventPublisher eventPublisher,
            EventHubProducerClient producerClient)
        {
            _studentRepository = studentRepository;
            _eventPublisher = eventPublisher;
            _producerClient = producerClient;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var student = _studentRepository.GetById(id);

            return student is null ? NotFound() : Ok(student);
        }

        [HttpPost("rabbitMQ")]
        public async Task<IActionResult> PostToRabbitMQ(Student student)
        {
            var createdStudent = _studentRepository.Add(student);

            await _eventPublisher.Publish(new StudentCreatedEvent(createdStudent.Id), "student-created");

            return Ok();
        }

        [HttpPost("eventsHub")]
        public async Task<IActionResult> PostToEventHub(Student student)
        {
            using var eventBatch = await _producerClient.CreateBatchAsync();

            if (!eventBatch.TryAdd(new EventData(JsonSerializer.SerializeToUtf8Bytes(eventBatch))))
                return BadRequest("Event is too large for the batch and cannot be sent.");

            try
            {
                await _producerClient.SendAsync(eventBatch);
                return Ok();
            }
            catch (Exception exception)
            {
                return StatusCode(500, exception);
            }
        }
    }
}
