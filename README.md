# UI Manager System for Unity

A lightweight and flexible system to manage your game’s UI panels in Unity. It helps you show, hide, and organize multiple UI screens easily, even if you have several instances of the same panel type.

---

## What It Does

- Manages UI panels like menus, HUDs, popups, and dialogs.  
- Supports multiple copies of the same panel type, each identified uniquely.  
- Automatically finds UI panels placed in your scene and manages them.  
- Provides events to track when panels show or hide, so you can add animations or logic.  
- Optionally destroys panels when hidden to save memory.  

---

## How To Use

Before using, make sure **all your panel scripts inherit from the base `UIPanel` class**.

You have two ways to use the UI Manager system in your project:

### Step 1: Setup UIManager

- Add the `UIManager` script to a GameObject in your scene (commonly your Canvas).  
- If you use panel prefabs, assign them to the UIManager’s **Panel Prefabs** list.  
- If you use scene panels, simply place them in your scene and attach the panel scripts; the UIManager will automatically find and register them at runtime.

### Option 1: Use Panel Prefabs

- Create UI panels as prefabs that inherit from `UIPanel`.  
- Assign these prefabs to the UIManager component’s **Panel Prefabs** list.  
- The UIManager will instantiate these prefabs when you show a panel.  
- This approach is great for dynamic UI that you create and destroy at runtime.

### Option 2: Use Scene Panels

- Place UI panels directly in your scene hierarchy (usually under your Canvas).  
- Attach your panel scripts (inheriting from `UIPanel`) to these GameObjects.  
- The UIManager will automatically find and register these panels when the scene starts.  
- Use this method if your panels are static and always present in the scene.  
- Note: Scene panels are not destroyed automatically when hidden unless you explicitly destroy them.

---

## Why Use This System?

- Simplifies UI management in complex projects.  
- Keeps your UI code organized and decoupled.  
- Supports dynamic UI with multiple instances and data passing.  
- Reduces boilerplate code for showing/hiding panels.  

---

## Getting Help or Contributing

If you find bugs or want to add features, feel free to open issues or submit pull requests on the project repository.

---
