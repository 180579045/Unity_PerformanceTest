﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class TreeViewRenderStrategy : EditorRenderStrategy
{
    public TreeViewRenderStrategy()
    {

        nodeSelectedBk = MakeSimpleColorTexture(new Color(0.4f, 0.4f, 0.7f, 1.0f));

        nodeSelectedStyle = new GUIStyle();
        nodeSelectedStyle.normal.textColor = Color.white;
        nodeSelectedStyle.normal.background = nodeSelectedBk;

        labelStyle = new GUIStyle();
        labelStyle.padding.top = -1;
        labelStyle.padding.bottom = 1;
        //labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = labelTextColor;

        foldOutStyle = new GUIStyle(EditorStyles.foldout);
        //foldOutStyle.fontStyle = FontStyle.Bold;
        //将长度固定为一个很长的区域，防止挤压图标
        foldOutStyle.fixedWidth = 500f;
        foldOutStyle.imagePosition = ImagePosition.ImageLeft;
        foldOutStyle.padding.left = 14;
        foldOutStyle.padding.top = 0;
        foldOutStyle.padding.right = 0;
        foldOutStyle.padding.bottom = 0;
         

        foldOutStyle.hover.textColor = foldOutStyle.normal.textColor;
        foldOutStyle.active.textColor = foldOutStyle.normal.textColor;
        foldOutStyle.focused.textColor = foldOutStyle.normal.textColor;

        foldOutStyle.hover.background = foldOutStyle.normal.background;
        foldOutStyle.active.background = foldOutStyle.normal.background;
        foldOutStyle.focused.background = foldOutStyle.normal.background;

        foldOutStyle.onHover.textColor = foldOutStyle.onNormal.textColor;
        foldOutStyle.onActive.textColor = foldOutStyle.onNormal.textColor;
        foldOutStyle.onFocused.textColor = foldOutStyle.onNormal.textColor;

        foldOutStyle.onHover.background = foldOutStyle.onNormal.background;
        foldOutStyle.onActive.background = foldOutStyle.onNormal.background;
        foldOutStyle.onFocused.background = foldOutStyle.onNormal.background;
        

        toggleStyle = new GUIStyle(EditorStyles.toggle);
        toggleStyle.alignment = TextAnchor.MiddleCenter;
        toggleStyle.margin.top = 0;
        toggleStyle.margin.bottom = 0;
        toggleStyle.padding.top = 1;
        toggleStyle.padding.bottom = 1;
        toggleStyle.border.top = 0;
        toggleStyle.border.bottom = 0;


        toggleLabelStyle = new GUIStyle(labelStyle);
        toggleLabelStyle.fontStyle = FontStyle.Normal;

    }

    public Texture2D MakeSimpleColorTexture(Color c)
    {
        Color[] pix = new Color[1];
        pix[0] = c;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }

    public override bool PreVisit(EditorControl c)
    {
        currCtrl = c as TreeViewCtrl;

        if (currCtrl == null)
            return false;

        GUILayoutOption[] options = new GUILayoutOption[] { 
            GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true)};

        EditorGUILayout.BeginHorizontal(options);
        EditorGUILayout.BeginVertical(SpecialEffectEditorStyle.PanelBox);
        return true;
    }

    public override void AfterVisit(EditorControl c)
    {
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    public override void Visit(EditorControl c)
    {
        currCtrl = c as TreeViewCtrl;

        if (
               (null == currCtrl)
            )
        {
            return;
        }


        PreDrawTree();

        foreach (var root in currCtrl.Roots)
        {
            TreeViewCtrl.PreorderTraverse(root, DrawTreeNode);
        }

        AfterDrawTree();

    }



    public void PreDrawTree()
    {
        EditorGUI.BeginDisabledGroup(!currCtrl.Enable);

        Vector2 oldPos = new Vector2(currCtrl.scrollPos.x, currCtrl.scrollPos.y);
        currCtrl.scrollPos = EditorGUILayout.BeginScrollView(
            currCtrl.scrollPos, true, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUIStyle.none);
        if (oldPos != currCtrl.scrollPos)
        {
            currCtrl.frameTriggerInfo.isScroll = true;
            currCtrl.frameTriggerInfo.scrollPos = new Vector2(currCtrl.scrollPos.x, currCtrl.scrollPos.y);
        }
        EditorGUILayout.BeginVertical();
    }

    public void AfterDrawTree()
    {
        EditorGUILayout.EndVertical(); 
        EditorGUILayout.EndScrollView();

        EditorGUI.EndDisabledGroup();

        currCtrl.UpdateLastRect();

        CheckInputEvent(currCtrl);

        //处理重绘请求
        if (requestRepaint)
        {
            requestRepaint = false;
            currCtrl.RequestRepaint();
        }
    }

    public bool DrawTreeNode(TreeViewNode n)
    {
        float spacePixelsPerLevel = 10f;
        float offset = 10f;

        nodeContent = new GUIContent();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(offset);

        foreach (var p in n.state.userParams)
        {
            if (p.param.GetType() == typeof(bool))
            {
                EditorGUILayout.LabelField(p.desc, toggleLabelStyle, GUILayout.Width(toggleLabelStyle.CalcSize(new GUIContent(p.desc)).x));
                bool newState = EditorGUILayout.Toggle((bool)p.param, toggleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(20f) });

                if (newState != (bool)p.param)
                {
                    currCtrl.frameTriggerInfo.isValueChanged = true;
                    currCtrl.frameTriggerInfo.isCtrlBehaveChange = true;
                    currCtrl.lastValueChangeNodePath = n.GetPathString();
                    currCtrl.CurrValue = n;
                }
                p.param = newState;
            }
        }

        GUILayout.Space(offset + spacePixelsPerLevel * (float)n.Level());
        if (n.IsLeaf())
        {
            n.state.IsExpand = false;
        }
        bool newExpandState = false;

        nodeContent.text = n.name;
        if (n.image != null)
        {
            nodeContent.image = n.image; 
        }

        if(n.tooptip != "")
        {
           nodeContent.tooltip = n.tooptip;
        }

        if (n.IsLeaf())
        {//叶节点绘制直接使用标签
            GUILayout.Space(14f);
            EditorGUILayout.LabelField(nodeContent, labelStyle, GUILayout.Width(labelStyle.CalcSize(nodeContent).x));
        }
        else
        {
            newExpandState = EditorGUILayout.Foldout(n.state.IsExpand, nodeContent, foldOutStyle);
        }
        
        SpecialEffectEditorUtility.GetLastRect(ref n.lastLabelRect);

        //若节点展开状态有变化则重绘
        if (newExpandState != n.state.IsExpand)
        {
            currCtrl.frameTriggerInfo.isCtrlBehaveChange = true;
            RequestRepaint();
        }

        if(currCtrl.Enable)
        {
            n.state.IsExpand = newExpandState;
        }

        //被选中节点绘制选中方形
        if (n.Equals(currCtrl.currSelectNode))
        {
            GUI.Box(n.lastRect, GUIContent.none);
            if (n.lastRect.size == new Vector2(0, 0))
            {
                RequestRepaint();
            }
        }

        EditorGUILayout.EndHorizontal();

        SpecialEffectEditorUtility.GetLastRect(ref n.lastRect);

        HandleMouseInput(n);

        //若当前节点没有展开则略过此节点子树渲染
        if (!n.state.IsExpand)
            return false;

        return true;
    }

    private void HandleMouseInput(TreeViewNode n)
    {
        if(
               (null == n)
            || (null == currCtrl)
            || !currCtrl.IsCurrentCtrlEnable()
            || currCtrl.IsEventTriggered()
            )
        {
            return;
        }

        if (!currCtrl.LastRect.Contains(CalcLocalPos(currCtrl, FrameInputInfo.GetInstance().currPos)))
        {
            return;
        }

        //CheckInputEvent(currCtrl);

        //处理树状控件节点点击
        Vector2 currMousePos = FrameInputInfo.GetInstance().currPos;
        Vector2 localMousePos = currCtrl.CalcLocalPos(currMousePos);

        float scrollBarWidth = 16f;
        Rect viewRect = new Rect(currCtrl.LastRect);
        viewRect.width = currCtrl.LastRect.width - scrollBarWidth;
        viewRect.height = currCtrl.LastRect.height - scrollBarWidth;

        if (
                n.lastRect.Contains(localMousePos + currCtrl.scrollPos) &&
                viewRect.Contains(currMousePos) &&
                FrameInputInfo.GetInstance().leftBtnPress
            )
        {
            if (!Object.ReferenceEquals(currCtrl.currSelectNode, n))
            {
                currCtrl.frameTriggerInfo.lastSelectItem = 0;
            }
            currCtrl.currSelectNode = n;

            currCtrl.frameTriggerInfo.isPressDown = true;

            CustomDragPrepare(currCtrl);

            RequestRepaint();
        }
        else if(
                n.lastRect.Contains(localMousePos + currCtrl.scrollPos) &&
                viewRect.Contains(currMousePos) &&
                FrameInputInfo.GetInstance().leftBtnDoubleClick 
            )
        {
            if (!Object.ReferenceEquals(currCtrl.currSelectNode, n))
            {
                currCtrl.frameTriggerInfo.lastSelectItem = 0;
            }

            currCtrl.frameTriggerInfo.isDoubleClick = true;
            currCtrl.ClickObject = n;
            currCtrl.currSelectNode = n;

            RequestRepaint();
        }
        else if(
                n.lastRect.Contains(localMousePos + currCtrl.scrollPos) &&
                viewRect.Contains(currMousePos) &&
                FrameInputInfo.GetInstance().leftBtnOnPress     
            )
        {
            currCtrl.frameTriggerInfo.isOnPress = true;
            currCtrl.ClickObject = n;
        
            RequestRepaint();
        }
        else if (
                   n.lastRect.Contains(localMousePos + currCtrl.scrollPos)
                && viewRect.Contains(currMousePos)
                && FrameInputInfo.GetInstance().leftBtnPressUp
            )
        {
            currCtrl.frameTriggerInfo.isPressUp = true;
            currCtrl.ClickObject = null;

            RequestRepaint();
        }
        else if (
                   n.lastRect.Contains(localMousePos + currCtrl.scrollPos)
                && viewRect.Contains(currMousePos)
                && FrameInputInfo.GetInstance().drag
            )
        {
            CustomDragStart(currCtrl);
        }
        else if(
                   n.lastRect.Contains(localMousePos + currCtrl.scrollPos)
                && viewRect.Contains(currMousePos)
                && FrameInputInfo.GetInstance().rightBtnPress
            )
        {
            currCtrl.frameTriggerInfo.isRPressDown = true;
            if (!Object.ReferenceEquals(currCtrl.currSelectNode, n))
            {
                currCtrl.frameTriggerInfo.lastSelectItem = 0;
            }
            currCtrl.currSelectNode = n;

            RequestRepaint();
        }
        else if(
                   n.lastRect.Contains(localMousePos + currCtrl.scrollPos)
                && viewRect.Contains(currMousePos)
                && FrameInputInfo.GetInstance().rightBtnPressUp
            )
        {
            currCtrl.frameTriggerInfo.isRPressUP = true;

            RequestRepaint();
        }
}

    private void CustomDragPrepare(EditorControl c)
    {
        object dragObject = null;
        TreeViewCtrl tree = c as TreeViewCtrl;

        if (null == tree)
        {
            return;
        }

        if (tree.onPrepareCustomDrag != null)
        {
            dragObject = tree.onPrepareCustomDrag(tree);

            if (dragObject != null)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { null };   // 必须在这里初始化objectReferences，否则会遇到Unity drag & drop的bug
                DragAndDrop.SetGenericData(tree.DragStartType, dragObject);
                m_IsCusDragPrepare = true;
            }
        }
    }

    private void CustomDragStart(EditorControl c)
    {
        TreeViewCtrl tree = c as TreeViewCtrl;

        if (null == tree)
        {
            return;
        }

        if ((Event.current.type != EventType.MouseDown)
            && (Event.current.type != EventType.MouseDrag))
        {
            return;
        }

        if (m_IsCusDragPrepare)
        {
            DragAndDrop.StartDrag(tree.DragStartType);
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

            m_DragBegionCtrl = tree;

            m_IsCusDragStart = true;

            m_IsCusDragPrepare = false;
        }

    }

    public void RequestRepaint()
    {
        requestRepaint = true;
    }


    TreeViewCtrl currCtrl = null;
    bool requestRepaint = false;
    //Stack<TreeViewNode> visitStack = new Stack<TreeViewNode>(); 

    Texture2D nodeSelectedBk;
    GUIStyle nodeSelectedStyle;
    GUIStyle labelStyle;
    GUIStyle foldOutStyle;
    GUIStyle toggleStyle;
    GUIStyle toggleLabelStyle;

    Color labelTextColor =
        new Color(179f / 255f, 179f / 255f, 179f / 255f); // dark style下foldout文字颜色

    GUIContent nodeContent = new GUIContent();


}
