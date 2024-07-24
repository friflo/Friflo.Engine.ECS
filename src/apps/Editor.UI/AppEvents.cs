// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Friflo.Editor.UI.Panels;
using Friflo.Engine.ECS;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
namespace Friflo.Editor;

public abstract class AppEvents
{
#region public properties
    public              EntityStore             Store       => store;
    public   static     Window                  Window      => _window;
    public   static     AppEvents               Instance    => _appEvents;
    #endregion
    
#region protected fields
    protected           EntityStore             store;
    protected readonly  List<EditorObserver>    observers   = new List<EditorObserver>();
    protected           bool                    isReady;
    #endregion

#region static fields
    private static      Func<Window>            _createMainWindow;
    private static      Window                  _window;
    private static      AppEvents               _appEvents;
    #endregion
    
    public static void Init(AppEvents appEvents, Func<Window> createMainWindow) {
        _appEvents          = appEvents;
        _createMainWindow   = createMainWindow;
    }
    
    public static Window CreateMainWindow() {
        _window = _createMainWindow();
        return _window;
    }
    
    
    public void AddObserver(EditorObserver observer)
    {
        observers.Add(observer);
        if (isReady) {
            observer.SendEditorReady();  // could be deferred to event loop
        }
    }
    
    public void SelectionChanged(EditorSelection selection) {
        StoreDispatcher.Post(() => {
            EditorObserver.CastSelectionChanged(observers, selection);    
        });
    }
    
    // -------------------------------------- panel / commands --------------------------------------
    protected PanelControl activePanel;
    
    internal void SetActivePanel(PanelControl panel)
    {
        if (activePanel != null) {
            activePanel.Header.PanelActive = false;
        }
        activePanel = panel;
        if (panel != null) {
            panel.Header.PanelActive = true;
        }
    }
}