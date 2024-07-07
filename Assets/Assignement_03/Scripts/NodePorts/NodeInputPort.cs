using System;
using UnityEngine;

[Serializable]
public class NodeInputPort : NodePort
{
    
    private const float PORT_MARGIN_LEFT_RIGHT = 5;

    public NodeInputPort(Node parentNode) : base(parentNode)
    {
        
    }
    protected override void UpdateRectBasedOnParentNode()
    {
        int countInParentNode = ParentNode.NodeInputPorts.Count;
        int indexInParentNode = ParentNode.NodeInputPorts.IndexOf(this);

        float x = ParentNode.UsedRect.x + PORT_WIDTH + PORT_MARGIN_LEFT_RIGHT;
        float y = ParentNode.UsedRect.y + ParentNode.UsedRect.height/2 - PORT_HEIGTH/2;
        
        UsedRect = new Rect(x, y, PORT_WIDTH,PORT_HEIGTH);
    }
}
