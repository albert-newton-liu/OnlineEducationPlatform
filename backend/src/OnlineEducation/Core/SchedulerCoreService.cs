using Quartz;

namespace OnlineEducation.Core;

/// <summary>
/// Scheduled job for generating bookable slots for teachers in the Online Education Platform.
/// Implements <see cref="ITeacherBookSlotJob"/> and is triggered by the Quartz scheduler.
/// </summary>
public class TeacherBookSlotJob : ITeacherBookSlotJob
{
    private readonly ILogger<TeacherBookSlotJob> _logger;
    private readonly IBookingCoreService _bookingCoreService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeacherBookSlotJob"/> class.
    /// </summary>
    /// <param name="logger">Logger for logging job execution details.</param>
    /// <param name="bookingCoreService">Core service for booking operations.</param>
    public TeacherBookSlotJob(ILogger<TeacherBookSlotJob> logger, IBookingCoreService bookingCoreService)
    {
        _logger = logger;
        _bookingCoreService = bookingCoreService;
    }

    /// <summary>
    /// Executes the scheduled job to generate bookable slots for all teachers.
    /// </summary>
    /// <param name="context">The job execution context provided by Quartz.</param>
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("execute TeacherBookSlotJob!, time: {now}", DateTimeOffset.Now);
        try
        {
           await _bookingCoreService.GenerateBookableSlot(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "execute TeacherBookSlotJob fail.");
        }
    }
}