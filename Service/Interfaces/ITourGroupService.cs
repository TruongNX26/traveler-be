﻿using Service.Models.AttendanceEvent;
using Service.Models.TourGroup;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourGroupService
{
    Task<Result<TourGroupViewModel>> Create(TourGroupCreateModel model);

    Task<Result<TourGroupViewModel>> Update(Guid groupId, TourGroupUpdateModel model);

    Task<Result> Delete(Guid groupId);

    Task<Result> AddTravelers(Guid tourGroupId, ICollection<Guid> travelerIds);

    Task<Result> RemoveTravelers(Guid tourGroupId, List<Guid> travelerIds);

    Task<Result<List<Guid>>> ListTravelers(Guid tourGroupId);

    Task<Result<List<AttendanceEventViewModel>>> ListAttendanceEvents(Guid tourGroupId);

    Task<Result<TourGroupViewModel>> Get(Guid id);
}