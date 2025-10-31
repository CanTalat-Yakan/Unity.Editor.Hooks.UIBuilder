# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# UI Builder Hooks

> Quick overview: Editor-only hooks into the Unity UI Builder. Access the active Builder window, get references to the Inspector, VisualTreeAsset, and RootVisualElement, react to focus/selection changes, and get/set the selected VisualElement. Includes path helpers to serialize/restore selections robustly.

Add a thin integration layer over the Unity UI Builder to power custom tools. Listen for initialization and focus changes, detect selection changes, read the active document and root visual tree, and programmatically select elements by object or by a path that is stable across sessions.

![screenshot](Documentation/Screenshot.png)

## Features
- Active UI Builder context (Editor-only)
  - `UIBuilderHook.Inspector` – the Builder inspector VisualElement
  - `UIBuilderHook.VisualTreeAsset` – the active document VTA
  - `UIBuilderHook.RootVisualElement` – the canvas root
- Lifecycle and selection events
  - `OnInitialization` – fired on load and when the active Builder window changes
  - `OnFocusedChanged` – fired when Builder gains/loses focus or first becomes available
  - `OnSelectionChanged` – fired when the selected VisualElement changes
- Selection helpers
  - `GetSelectedElement()` returns the currently selected VisualElement
  - `SetSelectedElement(VisualElement element)` selects an element if it belongs to the current document
- Stable element paths (serialize/restore selection)
  - Encode by `name` + `type` + `order` among same‑name same‑type siblings
  - `UIBuilderHookUtilities.GetSelectedElementPath(out int orderIndex)` → `IEnumerable<UIElementPathEntry>`
  - `UIBuilderHookUtilities.SetSelectedElement(IEnumerable<UIElementPathEntry> path)`
- Element type mapping
  - `UIElementTypes` maps common UI Toolkit controls to a compact enum for path persistence

## Requirements
- Unity Editor 6000.0+ (Editor-only; no runtime code)
- UI Builder installed (e.g., `com.unity.ui.builder`)
- UI Toolkit (VisualElement/UITK)

## Usage

Subscribe to UI Builder events and react to selection changes:

```csharp
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEssentials;

[InitializeOnLoad]
public static class UIBuilderHooksExample
{
    static UIBuilderHooksExample()
    {
        UIBuilderHook.OnInitialization += OnInit;
        UIBuilderHook.OnFocusedChanged += OnFocus;
        UIBuilderHook.OnSelectionChanged += OnSelection;
    }

    private static void OnInit()
    {
        // Access the active Builder context
        var vta = UIBuilderHook.VisualTreeAsset;
        var root = UIBuilderHook.RootVisualElement;
        // Debug.Log($"Builder initialized: {vta}");
    }

    private static void OnFocus()
    {
        // The active Builder window changed focus or was found
        // You can inspect UIBuilderHook.Inspector / RootVisualElement here
    }

    private static void OnSelection()
    {
        VisualElement selected = UIBuilderHook.GetSelectedElement();
        if (selected != null)
        {
            // Example: highlight the element’s bounds
            selected.MarkDirtyRepaint();
        }
    }
}
```

### Serialize and restore selection by path
Save the current selection and reapply it later, even across domain reloads or editor sessions (as long as the tree structure still matches):

```csharp
using System.Collections.Generic;
using UnityEssentials;
using UnityEngine.UIElements;

public static class UISelectionPersistence
{
    private static List<UIElementPathEntry> _savedPath;

    public static void SaveCurrentSelection()
    {
        int order;
        var path = UIBuilderHookUtilities.GetSelectedElementPath(out order);
        _savedPath = path != null ? new List<UIElementPathEntry>(path) : null;
    }

    public static void RestoreSelection()
    {
        if (_savedPath != null && _savedPath.Count > 0)
            UIBuilderHookUtilities.SetSelectedElement(_savedPath);
    }
}
```

### Programmatic selection by object
If you already have a reference to a VisualElement in the active document, you can select it directly:

```csharp
VisualElement ve = /* find or create */ null;
UIBuilderHook.SetSelectedElement(ve);
```

### Element path format
Each path segment is a `UIElementPathEntry`:
- `Name`: element.name (string)
- `DisplayName`: friendly name (name or type + optional index)
- `TypeIndex`: `UIElementType` index for the element’s type
- `OrderIndex`: zero‑based index among siblings with the same name and type

Resolution walks from `RootVisualElement` -> child… using (Name, TypeIndex, OrderIndex) per level. If any segment can’t be matched, selection will not change.

## Notes and Limitations
- UI Builder must be open: hooks are active only when an active Builder window exists
- Document scope: `SetSelectedElement` only succeeds for elements under the current `RootVisualElement`
- Path stability: selection path relies on `name`, element type, and sibling order; structural changes in the UXML can break resolution
- Polling: selection changes are detected by polling in `EditorApplication.update`
- Type coverage: `UIElementTypes` includes many common controls; unknown types fallback to `VisualElement`
- Editor-only: no runtime behavior

## Files in This Package
- `Editor/UIBuilderHook.cs` – Core Editor hook (events, context references, selection get/set)
- `Runtime/UIBuilderHookUtilities.cs` – Path encode/decode, helpers to find elements
- `Runtime/UIElementTypes.cs` – Mapping between VisualElement types and enum for persistence
- `Editor/InternalBridgeDef.asmdef` – Editor assembly definition
- `package.json` – Package manifest metadata

## Tags
unity, unity-editor, ui builder, ui toolkit, uitoolkit, uxml, uielements, visualelement, selection, visualtreeasset, path, inspector, editor-tool
