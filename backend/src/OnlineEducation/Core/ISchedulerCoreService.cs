using Quartz;

namespace OnlineEducation.Core;

/// <summary>
/// Core service interface for scheduling teacher book slot jobs.
/// Inherits from <see cref="IJob"/> to define scheduled background tasks for teacher bookable slots.
/// </summary>
public interface ITeacherBookSlotJob : IJob
{

}