﻿using Service.Models.Trip;

namespace Service.Models.TourGroup;

public class TourGroupViewModel
{
    public Guid Id;
    public DateTime CreatedAt;
    public string GroupName = null!;
    public Guid? TourGuideId;
    public int MaxOccupancy;
    public Guid TripId;
    public int TravelerCount;
    public TripViewModel? Trip;
}