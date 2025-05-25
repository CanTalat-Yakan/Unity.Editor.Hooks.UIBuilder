#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UI.Builder;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Codice.CM.Common.CmCallContext;

namespace UnityEssentials
{
    [InitializeOnLoad]
    public class UIBuilderHook
    {
        public static VisualElement Inspector;

        private static Builder s_builderWindow;
        private static VisualElement s_lastSelectedElement;

        public static Action OnInitialization { get; set; }
        public static Action OnSelectionChanged { get; set; }

        static UIBuilderHook()
        {
            EditorWindow.windowFocusChanged += OnFocusChanged;
            EditorApplication.update += PollSelectionChanged;
        }

        private static void OnFocusChanged()
        {
            if (Builder.ActiveWindow == null)
                return;

            if (Builder.ActiveWindow != s_builderWindow)
            {
                s_builderWindow = Builder.ActiveWindow;
                Inspector = s_builderWindow?.inspector;

                s_lastSelectedElement = null; // Reset on window change

                OnInitialization?.Invoke();
            }
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

        public static string GetSelectedElementPath()
        {
            var element = GetSelectedElement();
            if (element == null)
                return null;

            // Get the path of the selected element
            return GetElementPath(element);
        }

        public static string GetElementPath(VisualElement element)
        {
            // Walk up the hierarchy until we hit the document root
            var path = new List<string>();
            var current = element;
            var docRoot = element.GetFirstAncestorOfType<TemplateContainer>();

            while (current != null && current != docRoot)
            {
                path.Insert(0, GetElementName(current));
                current = current.parent;
            }

            return string.Join("/", path);
        }

        public static string GetSelectedElementName(VisualElement element) =>
            GetElementName(GetSelectedElement());

        public static string GetElementName(VisualElement element) =>
            !string.IsNullOrEmpty(element.name)
                ? element.name
                : element.GetType().Name;
    }
}
#endif