using Quartz;

namespace OnlineEducation.Core;

public class TeacherBookSlotJob : ITeacherBookSlotJob
{
    private readonly ILogger<TeacherBookSlotJob> _logger;
    private readonly IBookingCoreService _bookingCoreService;

    public TeacherBookSlotJob(ILogger<TeacherBookSlotJob> logger, IBookingCoreService bookingCoreService)
    {
        _logger = logger;
        _bookingCoreService = bookingCoreService;
    }


    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("execute TeacherBookSlotJob!, time: {now}", DateTimeOffset.Now);
        try
        {
           await  _bookingCoreService.GenerateBookableSlot(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "execute TeacherBookSlotJob fail.");
        }
        
    }
}