﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Infrastructure.InferiorBot;

public partial class Guild
{
    public decimal GuildId { get; set; }

    public List<decimal> BotChannels { get; set; }

    public List<decimal> DjRoles { get; set; }

    public bool ConvertUrls { get; set; }

    public virtual ICollection<ConvertedUrl> ConvertedUrls { get; set; } = new List<ConvertedUrl>();
}