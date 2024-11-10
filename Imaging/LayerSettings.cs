namespace Imaging
{
    public class LayerSettings
    {
        public LayerSettings()
        {
        }

        public LayerSettings(bool isVisible, float alpha)
        {
            IsVisible = isVisible;
            Alpha = alpha;
        }

        public bool IsVisible { get; set; } = true; // Determines if the layer is visible
        public float Alpha { get; set; } = 1.0f; // Transparency level (0 = fully transparent, 1 = fully opaque)
        public string LayerName { get; internal set; }
    }
}