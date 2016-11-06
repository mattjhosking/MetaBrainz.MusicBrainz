﻿using System.Collections.Generic;

namespace MetaBrainz.MusicBrainz.Entities {

  /// <summary>An entity with a name.</summary>
  public interface INamedEntity {

    /// <summary>The aliases for this entity.</summary>
    IEnumerable<IAlias> Aliases { get; }

    /// <summary>The text used to distinguish this entity from others with the same name.</summary>
    string Disambiguation { get; }

    /// <summary>The entity's name.</summary>
    string Name { get; }

  }

}