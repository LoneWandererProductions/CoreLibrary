using System.Collections.Generic;
using System.Xml.Serialization;

namespace LightVector
{
    /// <summary>
    ///     Save Container
    /// </summary>
    [XmlRoot(ElementName = "Element")]
    public sealed class SaveContainer
    {
        /// <summary>
        ///     Gets or sets the objects.
        /// </summary>
        /// <value>
        ///     The objects.
        /// </value>
        public List<SaveObject> Objects { get; init; } = new();

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; init; }

        /// <summary>
        ///     Gets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; init; }

        //TODO add more stuff
    }
}
