using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class TestWindow : EditorWindow
{
    BehaviorNodeTreeViewHelper m_TreeViewHelper;

    [MenuItem("Window/TestTreeView")]
    static public void Open()
    {
        CreateInstance<TestWindow>().Show();
    }

    void OnEnable()
    {
        m_TreeViewHelper = new BehaviorNodeTreeViewHelper();
        m_TreeViewHelper.OnEnable();
    }

    void OnDisable()
    {
        m_TreeViewHelper.OnDisable();
    }

    void OnGUI()
    {
        m_TreeViewHelper.DrawToolbar();
        var rect = EditorGUILayout.GetControlRect(false, position.size.y - 20);
        m_TreeViewHelper.OnGUI(rect);
    }
}