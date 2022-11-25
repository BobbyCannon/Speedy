#region References

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.Location;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Data.Location;

[TestClass]
public class ComparerTests
{
    #region Methods

    [TestMethod]
    public void GetElevation()
    {
        var latitude = 34.204313;
        var longitude = -82.615585;
        var key = "AtbtGXCf0Y0SjsJdosOslWlz2mM8a1TPBkvp4jl_CeLsKZ7y-Zl4YTwOd7k9in8v";

        //var url = $"http://dev.virtualearth.net/REST/v1/Elevation/List?points={latitude},{longitude}&heights=ellipsoid&key={key}";
        var url = $"http://dev.virtualearth.net/REST/v1/Elevation/List?points={latitude},{longitude}&heights=sealevel&key={key}";
        var targetString = "\"elevations\":[";
        var httpClient = new HttpClient();
        var message = httpClient.GetAsync(url).AwaitResults();

        if (!message.IsSuccessStatusCode)
        {
            throw new Exception($"HTTP Response Error: [{message}]");
        }

        using var inStream = message.Content.ReadAsStreamAsync().AwaitResults();
        using var reader = new StreamReader(inStream);

        var readString = reader.ReadToEnd();
        var index = readString.IndexOf(targetString);

        if (index < 0)
        {
            throw new Exception($"No elevations found in the return string: [{readString}]");
        }

        var elevationList = readString.Substring(index + targetString.Length);
        index = elevationList.IndexOf(']');

        if (index <= 0)
        {
            throw new Exception($"Format Error: [{readString}]");
        }

        elevationList = elevationList.Substring(0, index);

        var result = elevationList.Split(',');
        if (!result.Any())
        {
            "Nope".Dump();
            return;
        }

        (decimal.TryParse(result.FirstOrDefault(), out var elevation) ? elevation : -1).Dump();
    }

    [TestMethod]
    public void name()
    {
        var latitude = 34.204313;
        var longitude = -82.615585;

        // Maps: 145 Ellipsoid, 176 sealevel, 176-145 = 31 difference

        Converter.ConvertHeight(latitude, longitude, 176, ConvertFlag.GeoidToEllipsoid).Dump("Ellipsoid: ");
        Converter.ConvertHeight(latitude, longitude, 145, ConvertFlag.EllipsoidToGeoid).Dump("Geoid: ");
    }

    #endregion
}