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
        public int TypeIndex;
        public int OrderIndex;

        public UIElementPathEntry(string name, int typeIndex, int orderIndex)
        {
            Name = name;
            TypeIndex = typeIndex;
            OrderIndex = orderIndex;
        }
    }

    public class UIBuilderHookUtilities : MonoBehaviour
    {
        public static IEnumerable<UIElementPathEntry> GetSelectedElementPath()
        {
            VisualElement element = null;

#if UNITY_EDITOR
            element = UIBuilderHook.GetSelectedElement();
#endif

            if (element == null)
                return null;

            return GetElementPath(element);
        }

        public static IEnumerable<UIElementPathEntry> GetElementPath(VisualElement element)
        {
            var path = new List<UIElementPathEntry>();
            var current = element;
            var docRoot = element.GetFirstAncestorOfType<TemplateContainer>();

            while (current != null && current != docRoot)
            {
                var name = GetElementName(current);
                var typeIndex = GetElementInfo(current);
                var orderIndex = 0;

                if (current.parent != null)
                {
                    // Find the index among siblings with the same name and type
                    var siblings = current.parent.Children()
                        .Where(e => GetElementName(e) == name && GetElementInfo(e) == typeIndex)
                        .ToList();
                    orderIndex = siblings.IndexOf(current);
                }

                path.Insert(0, new UIElementPathEntry(name, typeIndex, orderIndex));
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

        public static string GetSelectedElementName(VisualElement element) =>
            GetElementName(element);

        public static string GetElementName(VisualElement element) =>
            element.name;

        public static int GetElementInfo(VisualElement element) =>
            (int)UIElementTypes.GetElementType(element);
    }
}
