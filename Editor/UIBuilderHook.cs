#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UI.Builder;
using UnityEditor;
using UnityEngine.UIElements;

namespace UnityEssentials
{
    [InitializeOnLoad]
    public class UIBuilderHook
    {
        public static VisualElement Inspector;
        public static VisualTreeAsset VisualTreeAsset;

        private static Builder s_builderWindow;
        private static VisualElement s_lastSelectedElement;

        public static Action OnFocusedChanged { get; set; }
        public static Action OnInitialization { get; set; }
        public static Action OnSelectionChanged { get; set; }

        static UIBuilderHook()
        {
            EditorWindow.windowFocusChanged += OnFocusChanged;
            EditorApplication.update += PollSelectionChanged;
            EditorApplication.update += PollForBuilderWindow;
        }

        private static void PollForBuilderWindow()
        {
            if (Builder.ActiveWindow != null)
            {
                OnFocusChanged();
                EditorApplication.update -= PollForBuilderWindow;
            }
        }

        [InitializeOnLoadMethod]
        public static void Initialize() =>
            OnInitialization?.Invoke();

        private static void OnFocusChanged()
        {
            if (Builder.ActiveWindow == null)
                return;

            if (Builder.ActiveWindow != s_builderWindow)
                OnInitialization?.Invoke();

            s_builderWindow = Builder.ActiveWindow;
            Inspector = s_builderWindow.inspector;
            VisualTreeAsset = s_builderWindow.document.visualTreeAsset;

            OnFocusedChanged?.Invoke();
        }

        private static void PollSelectionChanged()
        {
            if (s_builderWindow == null)
                return;

            var selection = s_builderWindow.selection;
            var currentSelected = selection?.selection?.FirstOrDefault();

            if (!ReferenceEquals(currentSelected, s_lastSelectedElement))
            {
                s_lastSelectedElement = currentSelected;
                OnSelectionChanged?.Invoke();
            }
        }

        public static VisualElement GetSelectedElement()
        {
            if (s_builderWindow == null)
                return null;

            // Get the selection from the Builder window
            var selection = s_builderWindow.selection;

            if (selection == null || selection.selectionCount == 0)
                return null;

            // Return the first selected element
            return selection?.selection.First();
        }

        public static IEnumerable<(string Name, int TypeIndex, int OrderIndex)> GetSelectedElementPath()
        {
            var element = GetSelectedElement();
            if (element == null)
                return null;

            return GetElementPath(element);
        }

        public static IEnumerable<(string Name, int TypeIndex, int OrderIndex)> GetElementPath(VisualElement element)
        {
            var path = new List<(string Name, int TypeIndex, int OrderIndex)>();
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

                path.Insert(0, (name, typeIndex, orderIndex));
                current = current.parent;
            }

            return path;
        }

        public static string GetSelectedElementName(VisualElement element) =>
            GetElementName(element);

        public static string GetElementName(VisualElement element) =>
            element.name;

        public static int GetElementInfo(VisualElement element) =>
            (int)UIElementTypes.GetElementType(element);
    }
}
#endif