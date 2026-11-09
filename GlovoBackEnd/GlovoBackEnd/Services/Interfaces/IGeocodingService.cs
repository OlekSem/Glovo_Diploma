using NetTopologySuite.Geometries;

namespace GlovoBackEnd.Services.Interfaces;

public interface IGeocodingService
{
    /// <summary>Returns a WGS 84 point for a human-readable address, when found.</summary>
    Task<Point?> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}
