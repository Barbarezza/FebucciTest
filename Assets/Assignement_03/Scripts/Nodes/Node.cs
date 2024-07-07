using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Node
{
    [SerializeReference] public List<NodeInputPort> NodeInputPorts = new ();
    
    [SerializeReference] public List<NodeOutputPort> NodeOutputPorts = new();

    public Rect OriginalRect
    {
        get
        {
            return originalRect;
        }

        private set
        {
            originalRect = value;
        }
    }
    
    [SerializeField] private Rect originalRect;

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
    
    

    [SerializeField] protected string title;
    
    public Node(Rect parentRect,Vector2 position, float widthPercentage, float heightPercentage)
    {
        float width = Screen.currentResolution.width / 100f * widthPercentage;
        float height = Screen.currentResolution.height / 100f * heightPercentage;

        Rect tempRect = new Rect(0, 0, width, height);
        
        tempRect.center = position;
        
        UsedRect = OriginalRect = tempRect;
    }
    
    public virtual void DrawNode()
    {
        GUIStyle style = new GUIStyle(EditorStyles.helpBox)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        };
        
        GUI.backgroundColor = Color.white;

        GUI.Box(UsedRect, title, style);
    }

    public virtual void DrawNodePorts()
    {
        foreach (NodeInputPort nodeInputPort in NodeInputPorts)
        {
            nodeInputPort.Draw();
        }
        
        foreach (NodeOutputPort nodeOutputPort in NodeOutputPorts)
        {
            nodeOutputPort.Draw();
        }
    }
    
    public void Drag(Vector2 delta)
    {
        Vector2 newPosition = UsedRect.position + delta;
        
        Rect rect = UsedRect;
        
        rect.position = newPosition;
        
        UsedRect = rect;
    }
}
