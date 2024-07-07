using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class NodeStarter : Node
{
    [SerializeField] public Rect StartLabelRect;
    
    public NodeStarter(Rect parentRect, Vector2 position, float widthPercentage, float heightPercentage) 
        : base(parentRect, position, widthPercentage, heightPercentage)
    {
        NodeOutputPorts.Add(new NodeOutputPort(this));
    }

    public override void DrawNode()
    {
        base.DrawNode();
        
        StartLabelRect = new Rect(UsedRect.x, UsedRect.y - 20, UsedRect.width, 20);
        
        GUIStyle style = new GUIStyle(EditorStyles.helpBox)
        {
            normal =
            {
                textColor = Color.white
            },
            
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
        
        GUI.backgroundColor = Color.green;
        
        GUI.Box(StartLabelRect, "Starter", style);
    }
}
