﻿namespace Service.Models.Location;

public record LocationCreateModel
(
    string Name,
    string Address,
    string? Country,
    string? City,
    long Longitude,
    long Latitude,
    string? Description,
    ICollection<Guid> Tags
);