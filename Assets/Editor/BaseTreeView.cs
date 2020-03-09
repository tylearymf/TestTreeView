using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System;

public class BaseTreeView : TreeView
{
    protected TreeViewItem m_Root;
    const string Drag_Title = "BaseTreeView";
    const string Drag_Data_Flag = "BaseTreeViewItem";

    public BaseTreeView(TreeViewState state) : base(state)
    {
        m_Root = new TreeViewItem(int.MaxValue, -1, "Root");
        m_Root.parent = null;
        m_Root.children = new List<TreeViewItem>();

        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        return m_Root;
    }

    public void AddChild(BaseTreeViewItem item)
    {
        rootItem.AddChild(item);
    }

    /// <summary>
    /// 递归查找Item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    public TreeViewItem FindItemRecursive(int id, TreeViewItem root)
    {
        if (root.id == id) return root;

        if (root.hasChildren)
        {
            foreach (var item in root.children)
            {
                var find = FindItemRecursive(id, item);
                if (find != null) return find;
            }
        }

        return null;
    }

    /// <summary>
    /// 移除标记的Item
    /// </summary>
    /// <param name="root"></param>
    void RemoveItemsRecursive(TreeViewItem root)
    {
        var item = root as BaseTreeViewItem;
        if (item != null && item.CanRemove)
        {
            throw new System.NotImplementedException();
        }

        if (root.hasChildren)
        {
            root.children.RemoveAll(x => (x as BaseTreeViewItem).CanRemove);
            foreach (var child in root.children)
            {
                RemoveItemsRecursive(child);
            }
        }
    }

    /// <summary>
    /// item是否属于parents的自身或者子对象
    /// </summary>
    /// <param name="parents"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    bool IsSelfOrChild(List<TreeViewItem> parents, TreeViewItem item)
    {
        if (item == null || parents == null) return false;
        if (parents.Contains(item)) return true;

        foreach (var child in parents)
        {
            var result = IsSelfOrChild(child.children, item);
            if (result) return true;
        }

        return false;
    }

    /// <summary>
    /// 重载并选中列表中指定的对象
    /// </summary>
    /// <param name="selects"></param>
    void ReloadAndSelect(IList<int> selects)
    {
        Reload();
        SetSelection(selects, TreeViewSelectionOptions.RevealAndFrame);
        SelectionChanged(selects);
    }

    protected override bool CanStartDrag(CanStartDragArgs args)
    {
        return true;
    }

    protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
    {
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.paths = null;
        DragAndDrop.objectReferences = new UnityEngine.Object[] { };
        DragAndDrop.SetGenericData(Drag_Data_Flag, args.draggedItemIDs);
        DragAndDrop.visualMode = DragAndDropVisualMode.None;
        DragAndDrop.StartDrag(Drag_Title);
    }

    protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
    {
        var visualMode = DragAndDropVisualMode.None;
        var draggedItemIDs = DragAndDrop.GetGenericData(Drag_Data_Flag) as IList<int>;
        if (draggedItemIDs != null && draggedItemIDs.Count > 0)
        {
            var insertAtIndex = args.insertAtIndex;
            var parentItem = args.parentItem;
            if (parentItem == null) return DragAndDropVisualMode.None;

            var draggedItems = draggedItemIDs.ToList().ConvertAll(x => FindItemRecursive(x, rootItem));
            var childCount = parentItem.children == null ? 0 : parentItem.children.Count;

            if (!IsSelfOrChild(draggedItems, parentItem))
            {
                visualMode = DragAndDropVisualMode.Move;
                if (args.performDrop)
                {
                    var newItems = new List<BaseTreeViewItem>();
                    //标记拖拽的Item为移除状态
                    foreach (var draggedItem in draggedItems)
                    {
                        var baseTreeView = (BaseTreeViewItem)draggedItem;
                        baseTreeView.CanRemove = true;

                        var newItem = new BaseTreeViewItem(baseTreeView);
                        newItem.CanRemove = false;

                        newItems.Add(newItem);
                    }

                    if (parentItem.hasChildren)
                    {
                        if (insertAtIndex < 0 || insertAtIndex > childCount)
                        {
                            parentItem.children.AddRange(newItems);
                        }
                        else
                        {
                            parentItem.children.InsertRange(insertAtIndex, newItems);
                        }
                    }
                    else
                    {
                        foreach (var item in newItems)
                        {
                            parentItem.AddChild(item);
                        }
                    }
                }

                RemoveItemsRecursive(rootItem);

                BaseTreeViewItem.UpdateDepth(parentItem);
            }

            if (visualMode != DragAndDropVisualMode.None)
            {
                ReloadAndSelect(draggedItemIDs);
                Repaint();
            }
        }

        return visualMode;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        base.RowGUI(args);

        var current = Event.current;
        if (args.selected && current.button == 1)
        {
            if (GUI.Button(args.rowRect, string.Empty, GUIStyle.none))
            {
                var tMenu = new GenericMenu();

                tMenu.AddItem("打印当前选中的Item").Enable(true).Action(() =>
                {
                    var item = args.item as BaseTreeViewItem;
                    Debug.LogError(string.Format("name:{0},childCount:{1}", item.displayName, item.children == null ? 0 : item.children.Count));
                });
                tMenu.AddSeparator();
                tMenu.AddItem("复制成员名").Enable(true).Action(() =>
                {
                    GUIUtility.systemCopyBuffer = args.item.displayName;
                });
                tMenu.ShowAsContext();
            }
        }
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);

        var current = Event.current;
        if (current.type == EventType.Layout) return;

        switch (current.keyCode)
        {
            case KeyCode.Delete:
                var items = state.selectedIDs.ConvertAll(x => FindItemRecursive(x, rootItem));
                var hasChange = false;
                items.ForEach(x =>
                {
                    if (x as BaseTreeViewItem != null)
                    {
                        hasChange = true;
                        (x as BaseTreeViewItem).CanRemove = true;
                    }
                });

                if (hasChange)
                {
                    RemoveItemsRecursive(rootItem);
                    current.Use();
                    Reload();
                }
                break;
        }
    }
}

public interface IBaseTreeView
{
    bool CanRemove { set; get; }
}

[Serializable]
public class BaseTreeViewItem : TreeViewItem, IBaseTreeView
{
    public BaseTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
    {
    }
    public BaseTreeViewItem(int id, int depth, string displayName, bool canRemove = false) : this(id, depth, displayName)
    {
        CanRemove = canRemove;
    }
    public BaseTreeViewItem(BaseTreeViewItem item) : this(item.id, item.depth, item.displayName, item.CanRemove)
    {
        this.children = item.children;
        this.parent = item.parent;
        this.icon = item.icon;
    }

    public bool CanRemove { get; set; }

    static public void UpdateDepth(TreeViewItem parentItem)
    {
        if (parentItem.hasChildren)
        {
            foreach (var item in parentItem.children)
            {
                item.parent = parentItem;
                item.depth = parentItem.depth + 1;

                UpdateDepth(item);
            }
        }
    }
}