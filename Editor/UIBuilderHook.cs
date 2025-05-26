#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UI.Builder;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEssentials
{
    [InitializeOnLoad]
    public class UIBuilderHook
    {
        public static VisualElement Inspector;
        public static VisualTreeAsset VisualTreeAsset;
        public static VisualElement RootVisualElement;

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

            try
            {
                s_builderWindow = Builder.ActiveWindow;
                Inspector = s_builderWindow.inspector;
                VisualTreeAsset = s_builderWindow.document.visualTreeAsset;
                RootVisualElement = s_builderWindow.canvas.documentRootElement;
            }
            catch (Exception) { }

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

            var selection = s_builderWindow.selection;

            if (selection == null || selection.selectionCount == 0)
                return null;

            return selection?.selection.First();
        }

        public static void SetSelectedElement(VisualElement element)
        {
            if (s_builderWindow == null)
                return;

            var selection = s_builderWindow.selection;
            if (selection == null || element == null)
                return;

            // Ensure the element is part of the current visual tree
            if (!IsElementInHierarchy(element, RootVisualElement))
                return;

            selection.Select(null, element);
            element.MarkDirtyRepaint();
            s_builderWindow.Repaint();
        }

        private static bool IsElementInHierarchy(VisualElement element, VisualElement root)
        {
            if (element == null || root == null)
                return false;

            var current = element;
            while (current != null)
            {
                if (current == root)
                    return true;
                current = current.parent;
            }
            return false;
        }
    }
}
#endif