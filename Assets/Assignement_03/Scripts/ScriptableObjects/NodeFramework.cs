using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Node Framework", menuName = "Node Framework/Node Framework Scriptable Object", order = 1)]
[Serializable]
public class NodeFramework : ScriptableObject
{
    [SerializeField]
    private string guid = System.Guid.NewGuid().ToString();
    
    public string Guid
    {
        get
        {
            return guid;
        }
    }
    
    [Space(10)]
    public string optionalJsonPath;
    
    
    [Serializable] public class NodeFrameworkWrapper 
    { 
        [SerializeReference] public List<Node> Nodes;
        [SerializeReference] public List<NodePortConnection> NodePortConnections;
    }
    
    
    public NodeFrameworkWrapper nodeFrameworkWrapper;
    
    
    
    [SerializeReference] public List<Node> Nodes;
    
    [SerializeReference] public List<NodePortConnection> NodePortConnections = new List<NodePortConnection>();
    
    [SerializeReference] public Rect windowGroupRect;

    private void Reset()
    {
        Rect groupRect = new Rect(0, 0, 99999, 99999);
            
        Nodes = new List<Node>
        {
            new NodeStarter(groupRect,
                groupRect.center, 
                7, 
                7)
        };
        
    }

    public void RecalculateDependencies()
    {
        foreach (Node node in Nodes)
        {
            foreach (NodePort nodePort in node.NodeInputPorts.Concat<NodePort>(node.NodeOutputPorts))
            {
                nodePort.ParentNode = node;
                nodePort.NodePortConnections = new List<NodePortConnection>();
            }
        }

        foreach (NodePortConnection nodePortConnection in NodePortConnections)
        {
            nodePortConnection.connectedPorts.Port1.NodePortConnections.Add(nodePortConnection);
                
            nodePortConnection.connectedPorts.Port2.NodePortConnections.Add(nodePortConnection);
        }
    }



    public void ExportJson(string optionalAdditionalPath = "")
    {
        string json = EditorJsonUtility.ToJson(new NodeFramework.NodeFrameworkWrapper()
        {
            Nodes = Nodes,
            NodePortConnections = NodePortConnections
        },
            true);
        
        EditorPrefs.SetString(guid,json);

        if (optionalAdditionalPath != "")
        {
            File.WriteAllText(optionalAdditionalPath, json);
        }
    }

    public void ImportJson(string optionalPath = "")
    {
        
        string json = "";
        
        if (optionalPath != "")
        {
            if (File.Exists(optionalPath))
            {
                json = File.ReadAllText(optionalPath);
            }
            else
            {
                Debug.LogError("File not found at path: " + optionalPath + " - Will try to read from EditorPrefs.");
                
                json = EditorPrefs.GetString(guid, "{}");
            }
        }
        else
        {
            json = EditorPrefs.GetString(guid, "{}");
        }
        
        NodeFrameworkWrapper wrapper = JsonUtility.FromJson<NodeFrameworkWrapper>(json);

        if (wrapper.Nodes is not null || wrapper.Nodes.Count > 0)
        {
            Nodes = wrapper.Nodes;
            
            NodePortConnections = wrapper.NodePortConnections;
            
            RecalculateDependencies();
        }
        else
        {
            Debug.LogError("No nodes found in json");
        }
    }
    
    public void AddNode<NodeType>(Rect windowRect,Vector2 position) where NodeType : Node
    {
        NodeType newNode = (NodeType) Activator.CreateInstance(typeof(NodeType),windowRect, position, 
            7, 
            7);
        
        Nodes.Add(newNode);
        
        EditorUtility.SetDirty(this);
    }

    public void DeleteNode(Node node)
    {
        foreach (NodePort nodePort in node.NodeInputPorts.Concat<NodePort>(node.NodeOutputPorts))
        {
            DeleteAllConnectionsToNodePort(nodePort);
        }
        
        Nodes.Remove(node);
        
        EditorUtility.SetDirty(this);
    }
    
    public void DeleteConnection(NodePortConnection nodePortConnection)
    {
        NodePortConnections.Remove(nodePortConnection);
        nodePortConnection.Dispose();
        
        EditorUtility.SetDirty(this);
    }
    
    public void DeleteAllConnectionsToNodePort(NodePort nodePort)
    {
        List<NodePortConnection> connectionsToDelete = new List<NodePortConnection>(nodePort.NodePortConnections);
        
        foreach (NodePortConnection nodePortConnection in connectionsToDelete)
        {
            DeleteConnection(nodePortConnection);
        }
    }
    


    
    /**
     * This function will not return recursively connected Nodes.
     * If you want to check if a new connection will create a loop, get all connected Nodes and check if the new node is in the list.
     *
     * You can add Nodes to the connectedNodes parameter if for some reason you already know some of the Nodes.
     */
    public HashSet<Node> GetAllConnectedNodes(Node node, HashSet<Node> connectedNodes = null)
    {
        if (connectedNodes is null)
        {
            connectedNodes = new HashSet<Node>() {node};
        }
        
        foreach (NodePort nodePort in  node.NodeInputPorts.Concat<NodePort>(node.NodeOutputPorts))
        {
            foreach (NodePortConnection nodePortConnection in nodePort.NodePortConnections)
            {
                Node node1 = nodePortConnection.connectedPorts.Port1.ParentNode;
                Node node2 = nodePortConnection.connectedPorts.Port2.ParentNode;
                
                if (!connectedNodes.Contains(node1))
                {
                    connectedNodes.Add(node1);
                    GetAllConnectedNodes(node1,connectedNodes);
                }
                
                if (!connectedNodes.Contains(node2))
                {
                    connectedNodes.Add(node2);
                    GetAllConnectedNodes(node2,connectedNodes);
                }
            }
        }

        return connectedNodes;
    }
}
