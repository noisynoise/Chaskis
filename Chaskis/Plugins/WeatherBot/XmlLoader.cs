﻿
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Chaskis.Plugins.WeatherBot
{
    /// <summary>
    /// Parses XML for this plugin.
    /// </summary>
    public static class XmlLoader
    {
        /// <summary>
        /// Error XML from NOAA.  Mainly here so we don't have to deal with XML arrays (which I didn't know were
        /// even a thing 0_0).
        /// </summary>
        private static readonly Regex errorRegex = new Regex(
            @"<error>.+\[faultstring\]\s+=>\s+(?<fault>[\w :]+)\s+\[detail\]\s.+<error>(?<errorStr>.+)</error>.+</error>",
            RegexOptions.Singleline | RegexOptions.Compiled
        );

        /// <summary>
        /// Parses the given XML from NOAA to a WeatherReport our plugin can use.
        /// </summary>
        /// <exception cref="ApplicationException">XML from NOAA is invalid.</exception>
        /// <param name="noaaXml">The XML from NOAA's SOAP service.</param>
        /// <param name="zipCode">The zip code we are getting the information from.</param>
        /// <returns>A Weather Report object.</returns>
        public static WeatherReport ParseWeatherReport( string noaaXml, string zipCode )
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml( noaaXml );

            XmlNodeList forecastNodes = doc.GetElementsByTagName( "app:Forecast_Gml2Point" );
            if ( forecastNodes.Count == 0 )
            {
                CheckForErrorResponse( noaaXml );

                throw new NOAAException(
                    NOAAErrors.MissingForecast,
                    "Got invalid XML from NOAA.  Could not get weather report from zip code " + zipCode
                );
            }

            WeatherReport report = new WeatherReport();
            report.ZipCode = zipCode;

            XmlNode forecastNode = forecastNodes[0];
            foreach ( XmlNode childNode in forecastNode.ChildNodes )
            {
                switch ( childNode.Name )
                {
                    // High Temperature:
                    case "app:maximumTemperature":
                        report.HighTemp = childNode.InnerText;
                        break;

                    // Low Temperature:
                    case "app:minimumTemperature":
                        report.LowTemp = childNode.InnerText;
                        break;

                    // Current Temperature:
                    case "app:temperature":
                        report.CurrentTemp = childNode.InnerText;
                        break;
                    
                    // Apparent Temperature:
                    case "app:apparentTemperature":
                        report.ApparentTemp = childNode.InnerText;
                        break;
                    
                    // Chance of Precipitation:
                    case "app:probOfPrecip12hourly":
                        report.ChanceOfPrecipitation = childNode.InnerText;
                        break;
                    
                    // Current Conditions:
                    case "app:weatherPhrase":
                        report.CurrentConditions = childNode.InnerText;
                        break;
                }
            }

            return report;
        }

        /// <summary>
        /// Parses the XML from NOAA which contains the latitude/longitude from
        /// a zip code.
        /// </summary>
        /// <param name="noaaXml">XML from NOAA.</param>
        /// <param name="zipCode">The zip code that was queried.</param>
        /// <returns>A tuple.  First element is latitude, second is longitude.</returns>
        public static Tuple<string, string> ParseLatitudeLongitude( string noaaXml, string zipCode )
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml( noaaXml );

            XmlNodeList zipcodeNodes = doc.GetElementsByTagName( "latLonList" );
            if ( zipcodeNodes.Count == 0 )
            {
                // If we can't find latLonList, search for an error.  Its possible we got an invalid zip code
                // from the user.  Search for it.
                CheckForErrorResponse( noaaXml );

                // If there is no error, throw an Exception.  We got something invalid from NOAA.
                throw new NOAAException(
                    NOAAErrors.MissingLatLon,
                    "Got invalid XML from NOAA.  Missing coordinates from zipcode " + zipCode
                );

            }

            string[] latLongString = zipcodeNodes[0].InnerText.Split( ',' );

            if ( latLongString.Count() != 2 )
            {
                // If there is no error, throw an Application Exception.  We got something invalid from NOAA.
                throw new NOAAException(
                    NOAAErrors.InvalidLatLon,
                    "Got invalid Lat/Long from NOAA.  Coordinates from zipcode " + zipCode
                );
            }

            return new Tuple<string, string>( latLongString[0], latLongString[1] );
        }

        /// <summary>
        /// Checks for an error from NOAA and throws a NOAAException if it finds one.
        /// </summary>
        /// <param name="noaaXml">The response from NOAA</param>
        private static void CheckForErrorResponse( string noaaXml )
        {
            Match errorMatch = errorRegex.Match( noaaXml );
            if ( errorMatch.Success )
            {
                // Throw an application exception, though from the error message from NOAA.
                throw new NOAAException(
                    NOAAErrors.SOAPError,
                    errorMatch.Groups["fault"].Value + " " + errorMatch.Groups["errorStr"].Value
                );
            }
        }
    }
}