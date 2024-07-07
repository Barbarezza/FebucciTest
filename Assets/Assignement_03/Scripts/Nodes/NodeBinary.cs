using System;
using UnityEngine;

[Serializable]
public class NodeBinary : Node
{
    public NodeBinary(Rect parentRect, Vector2 position, float widthPercentage, float heightPercentage) 
        : base(parentRect, position, widthPercentage, heightPercentage)
    {
        title = "Binary";
        
        NodeInputPorts.Add(new NodeInputPort(this));
        
        NodeOutputPorts.Add(new NodeOutputPort(this));
        NodeOutputPorts.Add(new NodeOutputPort(this));
    }
}
