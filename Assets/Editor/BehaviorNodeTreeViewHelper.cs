using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class BehaviorNodeTreeViewHelper
{
    [SerializeField]
    TreeViewState m_TreeViewState;
    [SerializeField]
    SearchField m_SearchField;
    [SerializeField]
    BehaviorNodeTreeView m_TreeView;

    public void OnEnable()
    {
        if (m_TreeViewState == null)
        {
            m_TreeViewState = new TreeViewState();
        }

        m_TreeView = new BehaviorNodeTreeView(m_TreeViewState);
        m_TreeView.searchString = string.Empty;
        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

        //测试数据
        for (int i = 0; i < 5000; i++)
        {
            var item = new BehaviorNodeTreeViewItem(i, 0, i.ToString());
            m_TreeView.AddChild(item);
        }
    }

    public void OnDisable()
    {
        m_SearchField.downOrUpArrowKeyPressed -= m_TreeView.SetFocusAndEnsureSelectedItem;
    }

    public void DrawToolbar()
    {
        if (m_TreeView != null)
        {
            m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
        }
    }

    public void OnGUI(Rect rect)
    {
        if (m_TreeView == null) return;
        m_TreeView.OnGUI(rect);
    }
}

public class BehaviorNodeTreeView : BaseTreeView
{
    public BehaviorNodeTreeView(TreeViewState state) : base(state)
    {
    }
}

public class BehaviorNodeTreeViewItem : BaseTreeViewItem
{
    public BehaviorNodeTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
    {
    }
}

