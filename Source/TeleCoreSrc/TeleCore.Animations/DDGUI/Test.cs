using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.DDGUI;

public static class Test
{
    public static void MenuBarNormal(Rect inRect)
    {
        //Define the main bar
        var menuBarRect = inRect.TopPartPixels(35);
        var buttonRowRect = menuBarRect.ContractedBy(1);
        
        //Define the buttons
        var menuButton1 = new Rect(buttonRowRect.x, buttonRowRect.y, 100, buttonRowRect.height); //Or buttonRowRect.GetLeftPartPixels(100);
        var menuButton2 = new Rect(buttonRowRect.x + 100, buttonRowRect.y, 100, buttonRowRect.height); //More complex to solve with rect extensions
        var menuButton3 = new Rect(buttonRowRect.x + 200, buttonRowRect.y, 100, buttonRowRect.height);
        
        //Define menu options for buttons
        var menuB1Option1 = ("New", new Action(() => {}));
        var menuB1Option3 = ("Save/Load", new Action(() => {}));
        
        var menuB2Option1 = ("Allow View 1", new Action(() => {}));
        var menuB2Option2 = ("Allow View 2", new Action(() => {}));
        
        //Button 3 is just a direct action
        
        //Draw the main bar
        GUI.Box(menuBarRect, GUIContent.none); //Use more complex box renderer later
        
        MenuButton(menuButton1, "File", ref _menu1, new List<(string, Action)> {menuB1Option1, menuB1Option3});
        MenuButton(menuButton2, "View", ref _menu2, new List<(string, Action)> {menuB2Option1, menuB2Option2});
        MenuButton(menuButton3, "Help", () => {});
    }

    private static bool _menu1;
    private static bool _menu2;

    private static void MenuButton(Rect rect, string label, Action action)
    {
        //var toggle = DGUI.Button(rect).Label(label).Action(action);
    }

    private static void MenuButton(Rect rect, string label, ref bool toggle, IList<(string, Action)> options)
    {
        //This already becomes complex as this requires a state attachment
        if (ToggleButton(rect, label, ref toggle)) //Allow more complex styling
        {
            
        }
        
        //OR
        
        //var toggle = DGUI.ToggleButton(rect).Label(label).Action(DGUI.ComboBox().Attach(rect, Attachment.Down).Options(source:options))
    }

    public static bool ToggleButton(Rect rect, string label, ref bool value)
    {
        var prev = value;
        value = GUI.Toggle(rect, value, label);
        return value != prev;
    }
}