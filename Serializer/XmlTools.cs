/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/XmlTools.cs
 * PURPOSE:     Some Xml Tools
 *              Logging Via Events
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;

namespace Serializer;

/// <summary>
///     The XML tools class.
///     Important: Object we serialize must be public!
/// </summary>
public static class XmlTools
{
    /// <summary>
    ///     Event triggered when an error occurs.
    /// </summary>
    public static event Action<string, Exception>? OnError;

    /// <summary>
    ///     Event triggered for informational messages.
    /// </summary>
    public static event Action<string>? OnInformation;

    /// <summary>
    ///     Logs the error.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="ex">The ex.</param>
    private static void LogError(string message, Exception ex = null)
    {
        OnError?.Invoke(message, ex);
    }

    /// <summary>
    ///     Logs the information.
    /// </summary>
    /// <param name="message">The message.</param>
    private static void LogInformation(string message)
    {
        OnInformation?.Invoke(message);
    }

    /// <summary>
    ///     Read a specific attribute value.
    /// </summary>
    /// <param name="path">Target file path with extension.</param>
    /// <param name="property">The property in the XML file.</param>
    /// <returns>First value as string. Can return null.</returns>
    public static string GetFirstAttributeFromXml(string path, string property)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var doc = LoadXml(path);
        if (doc == null)
        {
            return string.Empty;
        }

        var item = doc.DocumentElement?.SelectSingleNode("//Item");

        var attribute = item?.Attributes[property];

        if (attribute != null)
        {
            return attribute.Value;
        }

        LogError($"Attribute '{property}' not found in XML file: {path}");
        return null;
    }

    /// <summary>
    ///     Gets the elements from XML.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="elementName">Name of the element.</param>
    /// <returns>Element from Name</returns>
    public static List<string> GetElementsFromXml(string path, string elementName)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var doc = LoadXml(path);

        var elements = doc?.GetElementsByTagName(elementName);

        if (elements?.Count != 0)
        {
            return elements?.Cast<XmlNode>().Select(e => e.InnerText).ToList();
        }

        LogError($"Element '{elementName}' not found in XML file: {path}");
        return null;
    }

    /// <summary>
    ///     Read all values of a specific property from the XML file.
    /// </summary>
    /// <param name="path">Target file path with extension.</param>
    /// <param name="property">The property in the XML file.</param>
    /// <returns>A list of all values found. Can return null.</returns>
    public static List<string> GetAttributesFromXml(string path, string property)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var doc = LoadXml(path);
        if (doc == null)
        {
            return null;
        }

        var items = doc.DocumentElement?.SelectNodes("//Item");
        if (items == null || items.Count == 0)
        {
            LogError($"No <Item> elements found in XML file: {path}");
            return null;
        }

        var values = new List<string>();

        foreach (XmlNode item in items)
        {
            var attribute = item.Attributes[property];
            if (attribute != null)
            {
                values.Add(attribute.Value);
            }
        }

        if (values.Count == 0)
        {
            LogError($"Attribute '{property}' not found in XML file: {path}");
            return null;
        }

        return values;
    }


    /// <summary>
    ///     Load an XML file.
    /// </summary>
    /// <param name="path">Target file path with extension.</param>
    /// <returns>XML document or null in case of error.</returns>
    public static XmlDocument? LoadXml(string path)
    {
        var doc = new XmlDocument();
        try
        {
            doc.Load(path);
            LogInformation($"Successfully loaded XML file: {path}");
        }
        catch (Exception ex) when (ex is FileNotFoundException or ArgumentException or XmlException or IOException
                                       or NotSupportedException or SecurityException)
        {
            LogError($"Error loading XML file: {path} - {ex.Message}", ex);
            return null;
        }

        return doc;
    }
}
