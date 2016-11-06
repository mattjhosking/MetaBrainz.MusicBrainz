﻿using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;

namespace MetaBrainz.MusicBrainz.Entities.Objects {

  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
  [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
  [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
  [JsonObject(MemberSerialization.OptIn)]
  internal sealed class LabelInfo : ILabelInfo {

    [JsonProperty("catalog-number")]
    public string CatalogNumber { get; private set; }

    public ILabel Label => this._label;

    [JsonProperty("label")]
    private Label _label = null;

    public override string ToString() {
      var text = string.Empty;
      if (this.Label != null) {
        text += this.Label;
        if (this.CatalogNumber != null)
          text += ": ";
      }
      if (this.CatalogNumber != null)
        text += this.CatalogNumber;
      return text;
    }

  }

}
