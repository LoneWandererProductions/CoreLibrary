using System;
using System.Windows.Markup;

//TODO implement Further

/// <summary>
/// https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/
/// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/advanced/markup-extensions-and-wpf-xaml?view=netframeworkdesktop-4.8
/// </summary>
namespace CommonControls
{
    public sealed class EnumBindingSourceExtension : MarkupExtension
    {
        private readonly Type _enumType;

        public EnumBindingSourceExtension()
        {
        }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public Type EnumType
        {
            get => _enumType;
            init
            {
                if (value == _enumType)
                {
                    return;
                }

                if (value != null)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;

                    if (!enumType.IsEnum)
                    {
                        throw new ArgumentException("Type must be for an Enum.");
                    }
                }

                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_enumType == null)
            {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            var enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == _enumType)
            {
                return enumValues;
            }

            var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}
