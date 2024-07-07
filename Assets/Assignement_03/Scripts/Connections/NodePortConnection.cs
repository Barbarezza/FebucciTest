using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class NodePortConnection : IDisposable
{
    protected const float LINE_WIDTH = 2;
    [SerializeField] protected Vector2 START_OFFSET = new Vector2(0,-1);
    [SerializeField] protected Vector2 END_OFFSET = new Vector2(0,-1);

    [Serializable]
    public struct NodePortCouple
    {
        [SerializeReference] public NodePort Port1;
        [SerializeReference] public NodePort Port2;
    }
    
    [SerializeField] public NodePortCouple connectedPorts;
    
    private bool disposed = false;
    
    public NodePortConnection(NodePort port1, NodePort port2)
    {
        connectedPorts = new NodePortCouple()
        {
            Port1 = port1,
            Port2 = port2
            
        };
    }
    
    
    //We use this constructor to draw a line from a port and make it follow the mouse
    public NodePortConnection(NodePort port1)
    {
        connectedPorts = new NodePortCouple()
        {
            Port1 = port1,
            Port2 = null
        };
    }

    public void Draw()
    {
        Vector2 start = connectedPorts.Port1.UsedRect.center + START_OFFSET;
        Vector2 end = connectedPorts.Port2 is null ? Event.current.mousePosition : connectedPorts.Port2.UsedRect.center + END_OFFSET;
        
        float length = Vector2.Distance(start, end);

        Matrix4x4 originalMatrix = GUI.matrix;
        
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        GUIUtility.RotateAroundPivot(angle,start);
            
        GUI.DrawTexture(
            new Rect(start,new Vector2(length,LINE_WIDTH)),
            Texture2D.whiteTexture);
        
        GUI.matrix = originalMatrix;
    }

    public void ConnectToSecondPort(NodePort port)
    {
        connectedPorts.Port2 = port;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                if (connectedPorts.Port1 is not null)
                {
                    connectedPorts.Port1.NodePortConnections.Remove(this);
                }
                
                if (connectedPorts.Port2 is not null)
                {
                    connectedPorts.Port2.NodePortConnections.Remove(this);
                }
            }

            disposed = true;
        }
    }
}
