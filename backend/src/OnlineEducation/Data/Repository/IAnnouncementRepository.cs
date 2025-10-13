using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

/// <summary>
/// Interface for the announcement repository, providing data access methods for <see cref="AnnouncementDO"/> entities.
/// Inherits from the generic <see cref="IRepository{T}"/> interface.
/// </summary>
public interface IAnnouncementRepository : IRepository<AnnouncementDO>
{ }