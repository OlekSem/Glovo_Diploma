using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GlovoBackEnd.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace GlovoBackEnd.Services.Implementations;

/// <summary>
/// Geocodes the low-volume branch-address workflow through the public Nominatim API.
/// Nominatim permits at most one request per second, so requests are throttled in-process.
/// </summary>
public sealed class NominatimGeocodingService(HttpClient httpClient) : IGeocodingService
{
    private static readonly SemaphoreSlim RequestLock = new(1, 1);
    private static DateTimeOffset _lastRequestAt = DateTimeOffset.MinValue;
    private static readonly TimeSpan MinimumRequestInterval = TimeSpan.FromSeconds(1);

    public async Task<Point?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        await RequestLock.WaitAsync(cancellationToken);
        try
        {
            var remaining = MinimumRequestInterval - (DateTimeOffset.UtcNow - _lastRequestAt);
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining, cancellationToken);

            _lastRequestAt = DateTimeOffset.UtcNow;

            var query = Uri.EscapeDataString(address);
            using var response = await httpClient.GetAsync(
                $"search?format=jsonv2&limit=1&countrycodes=ua&q={query}",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var matches = await response.Content.ReadFromJsonAsync<List<NominatimSearchResult>>(cancellationToken);
            var match = matches?.FirstOrDefault();
            if (match is null ||
                !double.TryParse(match.Latitude, CultureInfo.InvariantCulture, out var latitude) ||
                !double.TryParse(match.Longitude, CultureInfo.InvariantCulture, out var longitude))
                return null;

            return new Point(longitude, latitude) { SRID = 4326 };
        }
        finally
        {
            RequestLock.Release();
        }
    }

    private sealed record NominatimSearchResult(
        [property: JsonPropertyName("lat")] string Latitude,
        [property: JsonPropertyName("lon")] string Longitude);
}
