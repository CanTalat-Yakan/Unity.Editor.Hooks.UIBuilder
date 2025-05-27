using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEssentials
{
    [Serializable]
    public struct UIElementPathEntry
    {
        public string Name;
        public string DisplayName;
        public int TypeIndex;
        public int OrderIndex;

        public UIElementPathEntry(string name, string displayName, int typeIndex, int orderIndex)
        {
            Name = name;
            DisplayName = displayName;
            TypeIndex = typeIndex;
            OrderIndex = orderIndex;
        }
    }

    public class UIBuilderHookUtilities : MonoBehaviour
    {
        public static void SetSelectedElement(IEnumerable<UIElementPathEntry> path)
        {
#if UNITY_EDITOR
            var element = FindElementByPath(UIBuilderHook.RootVisualElement, path);
            UIBuilderHook.SetSelectedElement(element);
#endif
        }
        
        public static IEnumerable<UIElementPathEntry> GetSelectedElementPath(out int orderIndex)
        {
            orderIndex = 0;
            VisualElement element = null;

#if UNITY_EDITOR
            element = UIBuilderHook.GetSelectedElement();
#endif

            if (element == null)
                return null;

            return GetElementPath(element, out orderIndex);
        }

        public static IEnumerable<UIElementPathEntry> GetElementPath(VisualElement element, out int orderIndex)
        {
            var path = new List<UIElementPathEntry>();
            var current = element;
            var docRoot = element.GetFirstAncestorOfType<TemplateContainer>();

            orderIndex = 0;

            while (current != null && current != docRoot)
            {
                var name = current.name;
                var typeIndex = GetElementInfo(current);
                var order = 0;

                if (current.parent != null)
                {
                    var siblings = current.parent.Children()
                        .Where(e => e.name == name && GetElementInfo(e) == typeIndex)
                        .ToList();
                    order = siblings.IndexOf(current);
                }

                if (current == element)
                    orderIndex = order; // Only set out parameter for the original element

                var displayName = GetElementDisplayName(current, orderIndex);

                path.Insert(0, new UIElementPathEntry(name, displayName, typeIndex, order));
                current = current.parent;
            }

            return path;
        }

        public static VisualElement FindElementByPath(VisualElement root, IEnumerable<UIElementPathEntry> path)
        {
            var current = root;
            foreach (var element in path)
            {
                if (current == null)
                    return null;

                // Find all children that match name and type index
                var matchingChildren = current.Children()
                    .Where(child =>
                        child.name == element.Name &&
                        (int)UIElementTypes.GetElementType(child) == element.TypeIndex)
                    .ToList();

                // Select the child at the specified order index, if available
                if (element.OrderIndex < 0 || element.OrderIndex >= matchingChildren.Count)
                    return null;

                current = matchingChildren[element.OrderIndex];
            }
            return current;
        }

        public static string GetElementDisplayName(VisualElement element, int typeIndex = 0) =>
            (string.IsNullOrEmpty(element.name) 
                ? element.GetType().Name
                : element.name) + 
            (typeIndex > 0 ? $" {typeIndex}" : string.Empty);

        public static int GetElementInfo(VisualElement element) =>
            (int)UIElementTypes.GetElementType(element);
    }
}
