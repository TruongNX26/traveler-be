﻿using Service.Models.TourGroup;
using Service.Models.Traveler;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITravelerService
{
    Task<Result> Register(TravelerRegistrationModel model);

    Task<Result<List<TourGroupViewModel>>> ListJoinedGroups(Guid travelerId);
}