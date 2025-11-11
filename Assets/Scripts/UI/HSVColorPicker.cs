using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPHSimulator.UI
{
    [UxmlElement]
    public partial class HSVColorPicker : VisualElement
    {
        private const int TextureSize = 200;

        private VisualElement colorArea;
        private Texture2D colorTexture;
        private Slider valueSlider;
        private VisualElement preview;

        private float hue;
        private float saturation;
        private float value = 1.0f;

        public event Action<Color> OnColorChanged;

        public Color SelectedColor => Color.HSVToRGB(hue, saturation, value);

        public HSVColorPicker()
        {
            style.flexDirection = FlexDirection.Column;

            // Create color area
            colorArea = new VisualElement();
            colorArea.style.width = TextureSize;
            colorArea.style.height = TextureSize;
            colorArea.style.marginBottom = 4;

            GenerateColorTexture();
            colorArea.style.backgroundImage = new StyleBackground(colorTexture);
            colorArea.RegisterCallback<PointerDownEvent>(OnColorPicked);

            // Create value slider (for brightness)
            valueSlider = new Slider(0f, 1f);
            valueSlider.label = "Brightness";
            valueSlider.value = value;
            valueSlider.RegisterValueChangedCallback
            (
                changeEvent =>
                {
                    value = changeEvent.newValue;
                    UpdatePreview();
                    OnColorChanged?.Invoke(SelectedColor);
                }
            );

            // Create preview box
            preview = new VisualElement();
            preview.style.height = 30;
            preview.style.marginTop = 6;
            preview.style.backgroundColor = SelectedColor;

            Add(colorArea);
            Add(valueSlider);
            Add(preview);
        }

        private void GenerateColorTexture()
        {
            if (colorTexture == null)
            {
                colorTexture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false);
                colorTexture.wrapMode = TextureWrapMode.Clamp;
            }

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float h = (float)x / (TextureSize - 1); // Hue (left to right)
                    float s = (float)y / (TextureSize - 1); // Saturation (bottom to top)

                    Color color = Color.HSVToRGB(h, s, value);
                    colorTexture.SetPixel(x, y, color);
                }
            }

            colorTexture.Apply();
        }

        private void OnColorPicked<T>(PointerEventBase<T> evt) where T : PointerEventBase<T>, new()
        {
            Vector2 local = evt.localPosition;

            float x = Mathf.Clamp(local.x, 0, TextureSize - 1);
            float y = Mathf.Clamp(local.y, 0, TextureSize - 1);

            hue = x / (TextureSize - 1);
            // Flip Y because pointer position is top-down, but texture is bottom-up
            saturation = 1f - (y / (TextureSize - 1));

            UpdatePreview();
            OnColorChanged?.Invoke(SelectedColor);
        }

        private void UpdatePreview()
        {
            preview.style.backgroundColor = SelectedColor;
        }

        public void SetColor(Color color)
        {
            Color.RGBToHSV(color, out hue, out saturation, out value);
            valueSlider.SetValueWithoutNotify(value);
            UpdatePreview();
        }
    }
}