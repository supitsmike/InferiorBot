﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Infrastructure.InferiorBot;

public partial class GameUser
{
    public Guid GameId { get; set; }

    public decimal UserId { get; set; }

    public string UserData { get; set; }

    public virtual Game Game { get; set; }

    public virtual User User { get; set; }
}