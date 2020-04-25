﻿using System;
using System.Collections.Generic;

using MetaBrainz.MusicBrainz.Interfaces.Entities;
using MetaBrainz.MusicBrainz.Interfaces.Searches;

namespace MetaBrainz.MusicBrainz.Objects.Entities {

  internal sealed class Event : Entity, IFoundEvent {

    public Event(Guid id) : base(EntityType.Event, id) {
    }

    public IReadOnlyList<IAlias>? Aliases { get; set; }

    public string? Annotation { get; set; }

    public bool Cancelled { get; set; }

    public string? Disambiguation { get; set; }

    public IReadOnlyList<ITag>? Genres { get; set; }

    public ILifeSpan? LifeSpan { get; set; }

    public string? Name { get; set; }

    public IRating? Rating { get; set; }

    public IReadOnlyList<IRelationship>? Relationships { get; set; }

    public string? Setlist { get; set; }

    public IReadOnlyList<ITag>? Tags { get; set; }

    public string? Time { get; set; }

    public string? Type { get; set; }

    public Guid? TypeId { get; set; }

    public IReadOnlyList<IUserTag>? UserGenres { get; set; }

    public IUserRating? UserRating { get; set; }

    public IReadOnlyList<IUserTag>? UserTags { get; set; }

    public override string ToString() {
      var text = string.Empty;
      if (this.SearchScore.HasValue)
        text += $"[Score: {this.SearchScore.Value}] ";
      if (this.Name != null)
        text += this.Name;
      if (!string.IsNullOrEmpty(this.Disambiguation))
        text += $" ({this.Disambiguation})";
      if (this.Type != null)
        text += $" ({this.Type})";
      return text;
    }

  }

}
