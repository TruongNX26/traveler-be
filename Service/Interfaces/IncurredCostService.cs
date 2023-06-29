﻿using Service.Models.IncurredCost;
using Service.Models.TourGroup;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IIncurredCostService
{
    Task<Result<IncurredCostViewModel>> Create(IncurredCostCreateModel model);

    Task<Result> Delete(Guid incurredCostId);

    Task<Result<List<IncurredCostViewModel>>> ListAll(Guid tourGroupId);

    Task<Result> UpdateCurrentSchedule(Guid tourGroupId, CurrentScheduleUpdateModel model);
}