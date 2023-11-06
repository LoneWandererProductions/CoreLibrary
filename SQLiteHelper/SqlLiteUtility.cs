/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/SqlLiteUtility.cs
 * PURPOSE:     Tools for SqlLite and Conversion of Objects into more appropriate for Tables
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression, for once for readability

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SQLiteHelper
{
    /// <inheritdoc cref="ISqlLiteUtility" />
    /// <summary>
    ///     The sql lite utility class.
    /// </summary>
    public sealed class SqlLiteUtility : ISqlLiteUtility
    {
        /// <inheritdoc />
        /// <summary>
        ///     Suggests a table Format don't expect any wonders
        /// </summary>
        /// <param name="obj">Generic Object</param>
        /// <returns>Table Suggestion</returns>
        [return: MaybeNull]
        public DictionaryTableColumns ConvertObject(object obj)
        {
            //well obvious don't fuck with me and don't expect an Debug Message
            if (obj == null)
            {
                return null;
            }

            var dct = new DictionaryTableColumns();

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var element = new TableColumns
                {
                    DataType = GetDataTyp(propertyInfo.PropertyType.Name),
                    PrimaryKey = false,
                    Unique = false,
                    NotNull = false
                };
                dct.DColumns.Add(propertyInfo.Name, element);
            }

            return dct;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Convert Single Object into List of strings
        ///     Works with Enum, sort of no guarantees
        ///     if an Attribute is empty it will add an empty string
        /// </summary>
        /// <param name="obj">Generic Object</param>
        /// <returns>List of Attribute as String, can return null.</returns>
        [return: MaybeNull]
        public List<string> ConvertToAttribute(object obj)
        {
            //well obvious don't fuck with me and don't expect an Debug Message
            return obj == null ? null : ConvertAttribute(obj);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Converts to table set.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>TableSet of Attribute,  can return null.</returns>
        [return: MaybeNull]
        public TableSet ConvertToTableSet(object obj)
        {
            //well obvious don't fuck with me and don't expect an Debug Message
            if (obj == null)
            {
                return null;
            }

            var lst = ConvertAttribute(obj);

            return lst == null ? null : new TableSet(lst);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Fill in Attributes by Name into Object
        ///     Strictly depends on the Order so well it is quite fragile
        ///     Works with Enum, sort of no guarantees
        /// </summary>
        /// <param name="row">Dictionary of Attribute Names and Values</param>
        /// <param name="obj">Object to be filled</param>
        /// <returns>Filled Object</returns>
        [return: MaybeNull]
        public object FillObject(List<string> row, object obj)
        {
            if (row == null || obj == null)
            {
                return null;
            }

            if (row.Count != obj.GetType().GetProperties().Length)
            {
                return null;
            }

            try
            {
                var count = -1;

                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    count++;

                    if (!propertyInfo.PropertyType.IsEnum)
                    {
                        propertyInfo.SetValue(obj, Convert.ChangeType(row[count], propertyInfo.PropertyType), null);
                    }
                    else
                    {
                        propertyInfo.SetValue(obj, Enum.Parse(propertyInfo.PropertyType, row[count], true), null);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (TargetException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (TargetParameterCountException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (MethodAccessException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (TargetInvocationException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (OverflowException ex)
            {
                Trace.WriteLine(ex);
            }

            return obj;
        }

        /// <summary>
        ///     Converts the attribute.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>List of Attribute as String, can return null</returns>
        [return: MaybeNull]
        private static List<string> ConvertAttribute(object obj)
        {
            var lst = new List<string>();

            try
            {
                foreach (var prop in obj.GetType().GetProperties())
                {
                    var value = string.Empty;

                    if (!prop.CanRead)
                    {
                        //Does the property has a Get accessor, if not add a blank
                        Trace.WriteLine(SqLiteHelperResources.InformationPropertyProtected);
                        lst.Add(value);
                        continue;
                    }

                    if (prop.GetIndexParameters().Length != 0)
                    {
                        //Does the property requires any Parameter?
                        Trace.WriteLine(SqLiteHelperResources.InformationPropertyNeedsParameter);
                        lst.Add(value);
                        continue;
                    }

                    if (prop.GetValue(obj) != null)
                    {
                        lst.Add(prop.GetValue(obj)?.ToString());
                    }
                    else
                    {
                        //Was the Property null, if not add a blank
                        Trace.WriteLine(SqLiteHelperResources.InformationPropertyWasNull);
                        lst.Add(value);
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
            catch (TargetParameterCountException ex)
            {
                //https://docs.microsoft.com/en-us/dotnet/api/system.reflection.targetparametercountexception?view=net-5.0
                //but should be catched anyways
                //https://stackoverflow.com/questions/6156577/targetparametercountexception-when-enumerating-through-properties-of-string
                Trace.WriteLine(ex);
                return null;
            }

            return lst;
        }

        /// <summary>
        ///     Try to convert Data Type into the correct Format
        /// </summary>
        /// <param name="dataType">Get C# Data Type</param>
        /// <returns>SqlLite DataType</returns>
        private static SqLiteDataTypes GetDataTyp(string dataType)
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
}
