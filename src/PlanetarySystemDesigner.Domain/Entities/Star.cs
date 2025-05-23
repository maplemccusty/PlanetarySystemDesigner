using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetarySystemDesigner.Domain.Entities;

public class Star
{
    public Guid Id { get; set; } =  Guid.NewGuid();
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    // User Inputs
    //----------------------------
    
    // star mass in Msol (Solar masses).
    public double Mass { get; set; } = 1.0;
    // star age in Billions of years
    public double Age { get; set; } = 4.6;
    
    // Derived Properties
    //----------------------------
    
    /// <summary>
    /// Spectral class (O, B, A, F, G, K, M) Further subdivided by number between 0 and 9 - derived from temperature
    /// </summary>
    [NotMapped]
    public string SpectralClass => CalculateSpectralClass();

    /// <summary>
    /// Star expected lifetime in billions of years
    /// </summary>
    [NotMapped]
    public double ExpectedLifetime => CalculateExpectedLifetime();
    
    /// <summary>
    /// Star radius in solar radii - derived from mass
    /// </summary>
    [NotMapped]
    public double Radius => CalculateRadius();
    
    /// <summary>
    /// Star luminosity in solar luminosities - derived from mass
    /// </summary>
    [NotMapped]
    public double Luminosity => CalculateLuminosity();
    
    /// <summary>
    /// Star density in solar densities - derived from mass
    /// </summary>
    [NotMapped]
    public double Density => CalculateDensity();
    
    /// <summary>
    /// Surface temperature in Kelvin - derived from mass and luminosity
    /// </summary>
    [NotMapped]
    public double Temperature => CalculateTemperature();
    
    /// <summary>
    /// Habitable zone inner border in AU
    /// </summary>
    [NotMapped]
    public double HabitableZoneInner => CalculateHabitableZoneInner();
    
    /// <summary>
    /// Habitable zone outer border in AU
    /// </summary>
    [NotMapped]
    public double HabitableZoneOuter => CalculateHabitableZoneOuter();
    
    /// <summary>
    /// Frost line (where water freezes) distance in AU 
    /// </summary>
    [NotMapped]
    public double FrostLine => CalculateFrostLine();

    /// <summary>
    /// Determine if this star can support a planet capable of earth-like life 
    /// </summary>
    [NotMapped]
    public string EarthLikeCompatible => CalculateEarthLikeCompatible();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Calculation Methods
    //----------------------------
    
    private double CalculateLuminosity()
    {
        return Mass switch
        {
            // Main sequence mass-luminosity relation
            <= 0.43 => 0.23 * Math.Pow(Mass, 2.3),
            <= 2.0 => Math.Pow(Mass, 4.0),
            <= 55.0 => 1.4 * Math.Pow(Mass, 3.5),
            _ => 32000 * Math.Pow(Mass / 20.0, 1.0)
        };
    }
    private double CalculateRadius()
    {
        // Main sequence mass-radius relation
        return Math.Pow(Mass, 0.78);
    }
    private double CalculateTemperature()
    {
        var luminosity = Luminosity;
        var radius = Radius;
        var tempRatio = Math.Pow(luminosity / (radius * radius), 0.25);
        return 5778 * tempRatio; // 5778K = Sun's effective temperature
    }
    private string CalculateSpectralClass()
    {
        var temp = Temperature;
        
        // Get the basic spectral type and numerical subdivision
        var (spectralType, subdivision) = GetSpectralTypeAndSubdivision(temp);
        return $"{spectralType}{subdivision}";
    }

    private (string spectralType, int subdivision) GetSpectralTypeAndSubdivision(double temperature)
    {
        // Each spectral class is subdivided 0-9 (0 = hottest, 9 = coolest within that class)
        
        switch (temperature)
        {
            case >= 30000:
            {
                // O class: 30,000K to ~50,000K
                var subdivision = Math.Min(9, (int)((50000 - temperature) / 2222)); // 0-9 scale
                return ("O", Math.Max(0, subdivision));
            }
            case >= 10000:
            {
                // B class: 10,000K to 30,000K  
                var subdivision = (int)((30000 - temperature) / 2222); // 0-9 scale
                return ("B", Math.Min(9, Math.Max(0, subdivision)));
            }
            case >= 7500:
            {
                // A class: 7,500K to 10,000K
                var subdivision = (int)((10000 - temperature) / 278); // 0-9 scale
                return ("A", Math.Min(9, Math.Max(0, subdivision)));
            }
            case >= 6000:
            {
                // F class: 6,000K to 7,500K
                var subdivision = (int)((7500 - temperature) / 167); // 0-9 scale  
                return ("F", Math.Min(9, Math.Max(0, subdivision)));
            }
            case >= 5200:
            {
                // G class: 5,200K to 6,000K (Sun is G2)
                var subdivision = (int)((6000 - temperature) / 89); // 0-9 scale
                return ("G", Math.Min(9, Math.Max(0, subdivision)));
            }
            case >= 3700:
            {
                // K class: 3,700K to 5,200K
                var subdivision = (int)((5200 - temperature) / 167); // 0-9 scale
                return ("K", Math.Min(9, Math.Max(0, subdivision)));
            }
            default:
            {
                // M class: Below 3,700K
                var mSubdivision = Math.Min(9, (int)((3700 - temperature) / 200)); // 0-9 scale
                return ("M", Math.Max(0, mSubdivision));
            }
        }
    }
    private double CalculateExpectedLifetime()
    {
        // Lifetime approximately proportional to M/L
        return 10.0 * Mass / Luminosity; // 10 billion years for Sun
    }
    private double CalculateHabitableZoneInner()
    {
        return Math.Sqrt(Luminosity / 1.107);
    }
    
    private double CalculateHabitableZoneOuter()
    {
        return Math.Sqrt(Luminosity / 0.356);
    }
    private double CalculateFrostLine()
    {
        // Approximate frost line distance
        return 2.7 * Math.Sqrt(Luminosity); // AU
    }
    private string CalculateEarthLikeCompatible()
    {
        if (Age <= 3.5) return "Star Too Young";
        return Mass is >= 0.5 or <= 1.4 ? "Yes" : "No";
    }
    private double CalculateDensity()
    {
        return Mass / Math.Pow(Radius, 3);
    }
}