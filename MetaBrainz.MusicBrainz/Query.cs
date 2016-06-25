﻿using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Xml.Serialization;

using MetaBrainz.MusicBrainz.Model;
using MetaBrainz.MusicBrainz.Model.Lists;

namespace MetaBrainz.MusicBrainz {

  /// <summary>Class providing access to the MusicBrainz API.</summary>
  public sealed class Query {

    #region Static Fields / Properties

    static Query() {
      // Mono's C# compiler does not like initializers on auto-properties, so set them up here instead.
      Query.DefaultPort      = -1;
      Query.DefaultUrlScheme = "https";
      Query.DefaultUserAgent = null;
      Query.DefaultWebSite   = "musicbrainz.org";
    }

    /// <summary>The default port number to use for requests (-1 to not specify any explicit port).</summary>
    public static int DefaultPort { get; set; }

    /// <summary>The default internet access protocol to use for requests.</summary>
    public static string DefaultUrlScheme { get; set; }

    /// <summary>The default user agent to use for requests.</summary>
    public static string DefaultUserAgent { get; set; }

    /// <summary>The default web site to use for requests.</summary>
    public static string DefaultWebSite { get; set; }

    /// <summary>
    ///   The amount of seconds to leave between requests. Set to 0 (or a negative value) to send all requests as soon as they are made.
    /// </summary>
    /// <remarks>
    ///   Note that this is a global delay, affecting all threads.
    ///   When querying the official musicbrainz site, setting this below the default of one second may incur penalties (ranging from rate limiting to IP bans).
    /// </remarks>
    public static double DelayBetweenRequests {
      get { return Query._requestDelay; }
      set {
        Query._requestDelay = value;
      }
    }

    /// <summary>The root location of the web service.</summary>
    public const string WebServiceRoot = "/ws/2";

    #endregion

    #region Constructors

    /// <summary>Creates a new instance of the <see cref="T:Query"/> class.</summary>
    /// <param name="userAgent">The user agent to use for all requests.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="userAgent"/> is null, and no default was set via <see cref="DefaultUserAgent"/>.</exception>
    public Query(string userAgent = null) {
      this.Port      =              Query.DefaultPort;
      this.UrlScheme =              Query.DefaultUrlScheme;
      this.UserAgent = userAgent ?? Query.DefaultUserAgent;
      this.WebSite   =              Query.DefaultWebSite;
      if (this.UserAgent == null)
        throw new ArgumentNullException(nameof(userAgent));
      // libmetabrainz does not validate/change the user agent in any way, so neither do we
    }

    #endregion

    #region Public Methods

    #region Lookup

    #region Generic

    /// <summary>Performs a general MBID-based lookup.</summary>
    /// <param name="entity">The type of entity to look up.</param>
    /// <param name="mbid">The MBID of the entity to retrieve.</param>
    /// <param name="extra">Any additional query parameters (e.g. "?inc=annotation").</param>
    /// <returns>The requested metadata.</returns>
    public Metadata Lookup(string entity, Guid mbid, string extra = null) => this.PerformRequest(entity, mbid.ToString("D"), extra);

    /// <summary>Performs a general lookup.</summary>
    /// <param name="entity">The type of entity to look up.</param>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="extra">Any additional query parameters (e.g. "?inc=annotation").</param>
    /// <returns>The requested metadata.</returns>
    public Metadata Lookup(string entity, string id, string extra = null) => this.PerformRequest(entity, id, extra);

    #endregion

    #region Specific Entities

    public Area LookupArea(Guid mbid, string extra = null) => this.Lookup("area", mbid, extra).Area;

    public Artist LookupArtist(Guid mbid, string extra = null) => this.Lookup("artist", mbid, extra).Artist;

    public Collection LookupCollection(Guid mbid, string extra = null) => this.Lookup("collection", mbid, extra).Collection;

    public DiscIdLookupResult LookupDiscId(string discid, string extra = null) => new DiscIdLookupResult(this.Lookup("discid", discid, extra));

    public Event LookupEvent(Guid mbid, string extra = null) => this.Lookup("event", mbid, extra).Event;

    public Instrument LookupInstrument(Guid mbid, string extra = null) => this.Lookup("instrument", mbid, extra).Instrument;

    public Isrc LookupIsrc(string isrc, string extra = null) => this.Lookup("isrc", isrc, extra).Isrc;

    public WorkList LookupIswc(string iswc, string extra = null) => this.Lookup("iswc", iswc, extra).WorkList;

    public Label LookupLabel(Guid mbid, string extra = null) => this.Lookup("label", mbid, extra).Label;

    public Place LookupPlace(Guid mbid, string extra = null) => this.Lookup("place", mbid, extra).Place;

    public Recording LookupRecording(Guid mbid, string extra = null) => this.Lookup("recording", mbid, extra).Recording;

    public Release LookupRelease(Guid mbid, string extra = null) => this.Lookup("release", mbid, extra).Release;

    public ReleaseGroup LookupReleaseGroup(Guid mbid, string extra = null) => this.Lookup("release-group", mbid, extra).ReleaseGroup;

    public Series LookupSeries(Guid mbid, string extra = null) => this.Lookup("series", mbid, extra).Series;

    public Url LookupUrl(Guid mbid, string extra = null) => this.Lookup("url", mbid, extra).Url;

    public Work LookupWork(Guid mbid, string extra = null) => this.Lookup("work", mbid, extra).Work;

    [XmlElement("rating")]        public Rating       Rating;
    [XmlElement("user-rating")]   public byte         UserRating;
    [XmlIgnore]                   public bool         UserRatingSpecified;

    #endregion

    #endregion

    #endregion

    #region Instance Fields / Properties

    /// <summary>The OAuth2 bearer token to use for authenticated requests; takes precedence over <see cref="Credential"/>.</summary>
    public string BearerToken { get; set; }

    /// <summary>The credential to use for authenticated requests; not used if <see cref="BearerToken"/> is also set.</summary>
    /// <remarks>The user name is <em>case sensitive</em> (unlike the logon on the MusicBrainz website).</remarks>
    public NetworkCredential Credential {
      get { return this._credential; }
      set {
        this._credential = value;
        this._lastDigest = null;
      }
    }

    /// <summary>The port number to use for requests (-1 to not specify any explicit port).</summary>
    public int Port { get; set; }

    /// <summary>The internet access protocol to use for requests.</summary>
    public string UrlScheme { get; set; }

    /// <summary>The user agent to use for requests.</summary>
    public string UserAgent { get; }

    /// <summary>The web site to use for requests.</summary>
    public string WebSite { get; set; }

    /// <summary>The base URI for all requests.</summary>
    public Uri BaseUri => new UriBuilder(this.UrlScheme, this.WebSite, this.Port, Query.WebServiceRoot).Uri;

    #endregion

    #region Internals

    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Metadata));

    private static readonly ReaderWriterLockSlim RequestLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

    private static DateTime _lastRequestTime;

    private static double _requestDelay = 1.0;

    private NetworkCredential _credential;

    private string _lastDigest;

    private Metadata PerformRequest(string entity, string id, string extra) {
      if (Query._requestDelay <= 0.0)
        return this.PerformDirectRequest(entity, id, extra);
      while (true) {
        Query.RequestLock.EnterWriteLock();
        try {
          if ((DateTime.UtcNow - Query._lastRequestTime).TotalSeconds >= Query._requestDelay) {
            try {
              return this.PerformDirectRequest(entity, id, extra);
            }
            finally {
              Query._lastRequestTime = DateTime.UtcNow;
            }
          }
        }
        finally {
          Query.RequestLock.ExitWriteLock();
        }
        Thread.Sleep((int) (500 * Query._requestDelay));
      }
    }

    private Metadata PerformDirectRequest(string entity,string id, string extra) {
      var uri = new UriBuilder(this.UrlScheme, this.WebSite, this.Port, $"{Query.WebServiceRoot}/{entity}/{id}", extra).Uri;
      Debug.Print($"[{DateTime.UtcNow}] WEB SERVICE REQUEST: {uri}");
      var firstTry = true;
    retry:
      var req = WebRequest.Create(uri) as HttpWebRequest;
      if (req == null)
        throw new InvalidOperationException("Only HTTP-compatible URL schemes are supported.");
      req.Method = "GET";
      req.Accept = "application/xml";
      {
        var an = Assembly.GetExecutingAssembly().GetName();
        req.UserAgent = $"{this.UserAgent} {an.Name}/v{an.Version}";
      }
      if (this.BearerToken != null)
        req.Headers.Add("Authorization", $"Bearer {this.BearerToken}");
      else if (this._lastDigest != null)
        req.Headers.Add("Authorization", this._lastDigest);
      try {
        using (var response = (HttpWebResponse) req.GetResponse()) {
          using (var stream = response.GetResponseStream()) {
            if (stream != null)
              return (Metadata) Query.Serializer.Deserialize(stream);
          }
        }
      }
      catch (WebException we) {
        var response = we.Response as HttpWebResponse;
        if (response != null) {
          if (firstTry && response.StatusCode == HttpStatusCode.Unauthorized) {
            firstTry = false; // only retry authentication once
            var digest = HttpDigestHelper.GetDigest(response, this.Credential);
            if (digest != null && this._lastDigest != digest) {
              this._lastDigest = digest;
              goto retry;
            }
          }
          // FIXME: Is there a better way to be sure it's an MB WS error response?
          if (response.ContentLength > 0 && response.ContentType.StartsWith("application/xml"))
            throw new QueryException(we);
        }
        // If not handled in some way, just rethrow the WebException.
        throw;
      }
      // We got a response without any content (probably impossible).
      throw new QueryException("Query did not produce results.");
    }

    #endregion

  }

}