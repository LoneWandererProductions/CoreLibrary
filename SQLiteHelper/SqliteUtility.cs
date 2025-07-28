/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqlLiteUtility.cs
 * PURPOSE:     Tools for SqlLite and Conversion of Objects into more appropriate for Tables
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression, for once for readability

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SqliteHelper;

/// <inheritdoc cref="SqliteUtility" />
/// <summary>
///     The sql lite utility class.
/// </summary>
public sealed class SqliteUtility : ISqliteUtility
{
    /// <inheritdoc />
    /// <summary>
    ///     Suggests a table format; don't expect any wonders.
    /// </summary>
    /// <param name="obj">Generic Object</param>
    /// <returns>Table Suggestion</returns>
    [return: MaybeNull]
    public DictionaryTableColumns ConvertObjectToTableColumns(object obj)
    {
        if (obj == null)
        {
            MessageHandling.SetMessage("Object is null in ConvertObjectToTableColumns.", 0);
            return null;
        }

        var tableColumns = new DictionaryTableColumns();

        try
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var column = new TableColumns
                {
                    DataType = GetDataType(propertyInfo.PropertyType.Name),
                    PrimaryKey = false,
                    Unique = false,
                    NotNull = false
                };
                tableColumns.DColumns.Add(propertyInfo.Name, column);
            }
        }
        catch (Exception ex)
        {
            MessageHandling.SetMessage($"Error in ConvertObjectToTableColumns: {ex.Message}", 0);
        }

        return tableColumns;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Convert a single object into a list of strings.
    ///     Works with Enum, but no guarantees.
    /// </summary>
    /// <param name="obj">Generic Object</param>
    /// <returns>List of Attribute as String, can return null.</returns>
    [return: MaybeNull]
    public List<string> ConvertObjectToAttributes(object obj)
    {
        if (obj == null)
        {
            MessageHandling.SetMessage("Object is null in ConvertObjectToAttributes.", 0);
            return null;
        }

        try
        {
            return ConvertAttributes(obj);
        }
        catch (Exception ex)
        {
            MessageHandling.SetMessage($"Error in ConvertObjectToAttributes: {ex.Message}", 0);
            return null;
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Converts to a table set.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>TableSet of Attribute, can return null.</returns>
    [return: MaybeNull]
    public TableSet ConvertObjectToTableSet(object obj)
    {
        if (obj == null)
        {
            MessageHandling.SetMessage("Object is null in ConvertObjectToTableSet.", 0);
            return null;
        }

        try
        {
            var attributes = ConvertAttributes(obj);
            return attributes == null ? null : new TableSet(attributes);
        }
        catch (Exception ex)
        {
            MessageHandling.SetMessage($"Error in ConvertObjectToTableSet: {ex.Message}", 0);
            return null;
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Fill in attributes by name into the object.
    ///     Strictly depends on the order, so it's quite fragile.
    ///     Works with Enum, but no guarantees.
    /// </summary>
    /// <param name="attributes">Dictionary of Attribute Names and Values</param>
    /// <param name="obj">Object to be filled</param>
    /// <returns>Filled Object</returns>
    [return: MaybeNull]
    public object FillObjectFromAttributes(List<string> attributes, object obj)
    {
        if (attributes == null || obj == null || attributes.Count != obj.GetType().GetProperties().Length)
        {
            MessageHandling.SetMessage("Attributes count mismatch in FillObjectFromAttributes.", 0);
            return null;
        }

        try
        {
            var count = 0;
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var value = propertyInfo.PropertyType.IsEnum
                    ? Enum.Parse(propertyInfo.PropertyType, attributes[count], true)
                    : Convert.ChangeType(attributes[count], propertyInfo.PropertyType);
                propertyInfo.SetValue(obj, value);
                count++;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or TargetException or TargetParameterCountException
                                       or MethodAccessException or TargetInvocationException or OverflowException)
        {
            MessageHandling.SetMessage($"Error in FillObjectFromAttributes: {ex.Message}", 0);
        }

        return obj;
    }

    /// <summary>
    ///     Converts the attributes.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>List of Attribute as String, can return null</returns>
    [return: MaybeNull]
    public List<string> ConvertAttributes(object obj)
    {
        var attributes = new List<string>();

        try
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                {
                    attributes.Add(string.Empty);
                    continue;
                }

                var value = prop.GetValue(obj)?.ToString() ?? string.Empty;
                attributes.Add(value);
            }
        }
        catch (Exception ex) when (ex is ArgumentNullException or TargetParameterCountException)
        {
            MessageHandling.SetMessage($"Error in ConvertAttributes: {ex.Message}", 0);
            return null;
        }

        return attributes;
    }

    /// <summary>
    ///     Try to convert Data Type into the correct format.
    /// </summary>
    /// <param name="dataType">Get C# Data Type</param>
    /// <returns>SqlLite DataType</returns>
    private static SqLiteDataTypes GetDataType(string dataType)
    {
        return dataType switch
        {
            "Int32" => SqLiteDataTypes.Integer,
            "String" => SqLiteDataTypes.Text,
            "Decimal" => SqLiteDataTypes.Decimal,
            "Single" => SqLiteDataTypes.Real,
            "DateTime" => SqLiteDataTypes.DateTime,
            _ => SqLiteDataTypes.Text
        };
    }
}
