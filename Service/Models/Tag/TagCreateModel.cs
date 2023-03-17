﻿using Data.Enums;

namespace Service.Models.Tag;

public record TagCreateModel()
{
    public string Name = null!;
    
    public TagType Type;
}