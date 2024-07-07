
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public abstract class NodePort
{
    protected const float PORT_WIDTH = 12;
    protected const float PORT_HEIGTH = 12;
    protected const float SMALLER_RECT_DIVIDER = 3;
    
    [SerializeField] protected Color PORT_COLOR = Color.black;
    [SerializeField] protected Color SMALLER_RECT_COLOR = Color.white;

    public Rect UsedRect
    {
        get
        {
            return usedRect;
        }
        protected set
        {
            usedRect = value;
        }
    }

    [SerializeField] private Rect usedRect;

    public Rect SmallerUsedRect
    {
        get
        {
            return smallerUsedRect;
        }
        protected set
        {
            smallerUsedRect = value;
        }
    }

    [SerializeField] private Rect smallerUsedRect;

    
    //We do not serialize this to avoid cyclic references when we save / export the data.
    //This parameter is relinked when we load the data.
    [NonSerialized]
    public Node ParentNode;
    
    //Same as before
    [NonSerialized]
    public List<NodePortConnection> NodePortConnections = new List<NodePortConnection>();

    public NodePort(Node parentNode)
    {
        ParentNode = parentNode;
    }
    
    protected abstract void UpdateRectBasedOnParentNode();

    private void UpdateSmallerRect()
    {
        SmallerUsedRect = new Rect(UsedRect.x + PORT_WIDTH/SMALLER_RECT_DIVIDER,
            UsedRect.y + PORT_HEIGTH/SMALLER_RECT_DIVIDER,
            UsedRect.width/SMALLER_RECT_DIVIDER,
            UsedRect.height/SMALLER_RECT_DIVIDER);
    }

    public virtual void Draw()
    {
        UpdateRectBasedOnParentNode();
        
        UpdateSmallerRect();
        
        GUI.backgroundColor = PORT_COLOR;
        GUI.Box(UsedRect,"");
        
        GUIStyle smalleRectStyle = new GUIStyle(GUI.skin.box);
        GUI.backgroundColor = SMALLER_RECT_COLOR;
        smalleRectStyle.normal.background = EditorGUIUtility.whiteTexture;
        GUI.Box(SmallerUsedRect,"",smalleRectStyle);
    }
}
