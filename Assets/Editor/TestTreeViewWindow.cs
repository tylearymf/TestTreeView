using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class TestTreeViewWindow : EditorWindow
{
    [SerializeField]
    TreeViewState m_TreeViewState;
    [SerializeField]
    SearchField m_SearchField;
    [SerializeField]
    TestTreeView1 m_TestTreeView;

    [MenuItem("Window/TestTreeView")]
    static public void Open()
    {
        CreateInstance<TestTreeViewWindow>().Show();
    }

    void OnEnable()
    {
        if (m_TreeViewState == null)
        {
            m_TreeViewState = new TreeViewState();
        }

        m_TestTreeView = new TestTreeView1(m_TreeViewState);
        m_TestTreeView.searchString = string.Empty;
        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_TestTreeView.SetFocusAndEnsureSelectedItem;

        for (int i = 0; i < 5000; i++)
        {
            var item = new TestTreeViewItem1(i, 0, i.ToString());
            m_TestTreeView.AddChild(item);
        }
    }

    void OnGUI()
    {
        if (m_TestTreeView == null) return;

        m_TestTreeView.searchString = m_SearchField.OnToolbarGUI(m_TestTreeView.searchString);
        var rect = EditorGUILayout.GetControlRect(false, position.size.y - 20);
        m_TestTreeView.OnGUI(rect);
    }
}


public class TestTreeView1 : BaseTreeView
{
    public TestTreeView1(TreeViewState state) : base(state)
    {
    }
}

public class TestTreeViewItem1 : BaseTreeViewItem
{
    public TestTreeViewItem1(int id, int depth, string displayName) : base(id, depth, displayName)
    {
    }
}