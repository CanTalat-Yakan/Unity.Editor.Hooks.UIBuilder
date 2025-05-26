using UnityEngine.UIElements;

namespace UnityEssentials
{
    public enum UIElementType
    {
        VisualElement,
        BindableElement,
        Button,
        Label,
        Scroller,
        TextField,
        Foldout,
        Slider,
        SliderInt,
        ProgressBar,
        DropdownField,
        TextElement
    }

    public static class UIElementTypes
    {
        public static System.Type GetElementType(UIElementType type) =>
            type switch
            {
                UIElementType.VisualElement => typeof(VisualElement),
                UIElementType.BindableElement => typeof(BindableElement),
                UIElementType.Button => typeof(Button),
                UIElementType.Label => typeof(Label),
                UIElementType.Scroller => typeof(Scroller),
                UIElementType.TextField => typeof(TextField),
                UIElementType.Foldout => typeof(Foldout),
                UIElementType.Slider => typeof(Slider),
                UIElementType.SliderInt => typeof(SliderInt),
                UIElementType.ProgressBar => typeof(ProgressBar),
                UIElementType.DropdownField => typeof(DropdownField),
                UIElementType.TextElement => typeof(TextElement),
                _ => null,
            };

        public static UIElementType GetElementType(this VisualElement element) =>
            element switch
            {
                Button => UIElementType.Button,
                Label => UIElementType.Label,
                Scroller => UIElementType.Scroller,
                TextField => UIElementType.TextField,
                Foldout => UIElementType.Foldout,
                Slider => UIElementType.Slider,
                SliderInt => UIElementType.SliderInt,
                ProgressBar => UIElementType.ProgressBar,
                DropdownField => UIElementType.DropdownField,
                TextElement => UIElementType.TextElement,
                _ => UIElementType.VisualElement
            };
    }
}