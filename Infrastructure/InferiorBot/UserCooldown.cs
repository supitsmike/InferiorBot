﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Infrastructure.InferiorBot;

public partial class UserCooldown
{
    public decimal UserId { get; set; }

    public DateTime? DailyCooldown { get; set; }

    public DateTime? WorkCooldown { get; set; }

    public virtual User User { get; set; }
}