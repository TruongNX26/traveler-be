﻿using Data.Enums;

namespace Service.Models.Schedule;

public record ScheduleCreateModel
(
    int Sequence,
    string Description,
    double? Longitude,
    double? Latitude,
    Vehicle? Vehicle
);