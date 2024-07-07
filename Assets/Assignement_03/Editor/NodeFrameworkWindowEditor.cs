using System.Linq;
using UnityEditor;
using UnityEngine;


public class NodeFrameworkWindowEditor : EditorWindow
{
    private NodeFramework scriptableObject;
    
    
    private Rect groupRect;
    

    private Node currentSelectedNode;
    private NodePort currentSelectedNodePort;
    private NodePortConnection currentSelectedNodePortConnection;


    private bool isShiftPressed;
    private Vector2 pan = new Vector2(0, 0);

    private void OnDisable()
    {
        EditorPrefs.SetFloat("NodeFrameworkEditorWindowX", 
            100f / Screen.currentResolution.width * position.x);
        EditorPrefs.SetFloat("NodeFrameworkEditorWindowY", 
            100f / Screen.currentResolution.height * position.y);
        
        EditorPrefs.SetFloat("NodeFrameworkWindowWidth", 
            100f / Screen.currentResolution.width * position.width);
        EditorPrefs.SetFloat("NodeFrameworkWindowHeight", 
            100f / Screen.currentResolution.height * position.height);

    }
    

    public static NodeFrameworkWindowEditor CreateWindow(NodeFramework nodeFramework)
    {
        NodeFrameworkWindowEditor window = GetWindow<NodeFrameworkWindowEditor>();
        
        
        float savedX = EditorPrefs.GetFloat("NodeFrameworkEditorWindowX", 5);
        float savedY = EditorPrefs.GetFloat("NodeFrameworkEditorWindowY", 5);
        
        float savedWidth = EditorPrefs.GetFloat("NodeFrameworkWindowWidth", 80);
        float savedHeight = EditorPrefs.GetFloat("NodeFrameworkWindowHeight", 80);
        
        
        window.position = new Rect(
            Screen.currentResolution.width / 100f * savedX, 
            Screen.currentResolution.height / 100f * savedY, 
            Screen.currentResolution.width / 100f * savedWidth, 
            Screen.currentResolution.height / 100f * savedHeight);
            
        
        window.titleContent = new GUIContent("Node Framework");
        
        
        
        window.groupRect = new Rect(0, 0, 99999, 99999);
        
        
        
        window.scriptableObject = nodeFramework;

        return window;

    }

    public void SetPanCenteredOnStarterNode()
    {
        foreach (Node node in scriptableObject.Nodes)
        {
            if (node.GetType() == typeof(NodeStarter))
            {
                pan = new Vector2(position.width / 2 + (groupRect.center.x - node.UsedRect.center.x), 
                    position.height / 2+ (groupRect.center.y - node.UsedRect.center.y));
                
                break;
            }
        }
    }

    private void OnGUI()
    {
        Repaint();

        groupRect.center = pan;

        GUIStyle groupStyle = new GUIStyle();
        
        GUI.BeginGroup(groupRect,groupStyle);
        
        DrawNodes();
        
        DrawConnectionsBetweenPorts();
        
        GUI.EndGroup();
        
        ManageMouseEvents();
        ManageKeyboardEvents();
        
    }
    
    private void DrawNodes()
    {
        foreach (var node in scriptableObject.Nodes)
        {
            node.DrawNode();
            node.DrawNodePorts();
        }
    }

    private void ManageMouseEvents()
    {
        Event ev = Event.current;

        switch (ev.type)
        {
            case EventType.MouseDown:
                
                foreach (Node node in scriptableObject.Nodes)
                {
                    if ( (node.GetType() == typeof(NodeStarter) && 
                          ((NodeStarter) node).StartLabelRect.Contains(ev.mousePosition - groupRect.position)) 
                         || node.UsedRect.Contains(ev.mousePosition - groupRect.position))
                    {
                        foreach (NodePort nodePort in node.NodeInputPorts.Concat<NodePort>(node.NodeOutputPorts))
                        {
                            if (nodePort.UsedRect.Contains(ev.mousePosition - groupRect.position))
                            {
                                currentSelectedNodePort = nodePort;
                                
                                break;
                            }
                        }

                        if (currentSelectedNodePort is null)
                        {
                            currentSelectedNode = node;
                        }

                    }
                }
                
                if (ev.button == 1)
                {
                    CreateContextMenu(ev,
                        currentSelectedNode is not null, 
                        currentSelectedNodePort is not null,
                        false);

                    currentSelectedNode = null;
                    currentSelectedNodePort = null;
                }
                else if (ev.button == 0)
                {
                    if (currentSelectedNodePort is not null)
                    {
                        currentSelectedNodePortConnection = new NodePortConnection(currentSelectedNodePort);
                                
                        scriptableObject.NodePortConnections.Add(currentSelectedNodePortConnection);
                    }

                }
                
                break;
            
            case EventType.MouseUp:
                
                if (ev.button == 0)
                {
                    if (currentSelectedNode != null)
                    {
                        currentSelectedNode = null;
                    }
                    else if (currentSelectedNodePort != null)
                    {
                        bool foundNodePortToConnect = false;
                        
                        foreach (Node node in scriptableObject.Nodes)
                        {
                            foreach (NodePort nodePort in node.NodeInputPorts.Concat<NodePort>(node.NodeOutputPorts))
                            {
                                if (nodePort.UsedRect.Contains(ev.mousePosition - groupRect.position)
                                && nodePort != currentSelectedNodePort
                                && currentSelectedNodePort.GetType() != nodePort.GetType()
                                && currentSelectedNodePort.ParentNode != nodePort.ParentNode)
                                {
                                    foundNodePortToConnect = true;
                                    
                                    currentSelectedNodePortConnection.ConnectToSecondPort(nodePort);
                                    
                                    currentSelectedNodePortConnection.connectedPorts
                                        .Port1.NodePortConnections.Add(currentSelectedNodePortConnection);
                                    
                                    currentSelectedNodePortConnection.connectedPorts
                                        .Port2.NodePortConnections.Add(currentSelectedNodePortConnection);
                                    
                                    EditorUtility.SetDirty(scriptableObject);

                                    break;
                                }
                            }

                            if (foundNodePortToConnect)
                            {
                                break;
                            }
                        }
                        
                        currentSelectedNodePort = null;

                        if (!foundNodePortToConnect)
                        {
                            scriptableObject.NodePortConnections.Remove(currentSelectedNodePortConnection);
                        }
                        
                        currentSelectedNodePortConnection = null;
                    }
                }
                
                break;
            
            case EventType.MouseDrag:
                
                if (ev.button == 0)
                {
                    if (currentSelectedNode != null)
                    {
                        if (isShiftPressed)
                        {
                            foreach (Node node in scriptableObject.GetAllConnectedNodes(currentSelectedNode))
                            {
                                node.Drag(ev.delta);
                            }
                        }
                        else
                        {
                            currentSelectedNode.Drag(ev.delta);  
                        }
                        
                        EditorUtility.SetDirty(scriptableObject);
                    }
                    else if (currentSelectedNodePort != null)
                    {
                        
                    }
                    else
                    {
                        pan += ev.delta;
                        
                        EditorUtility.SetDirty(scriptableObject);
                    }
                    
                }
                
                break;
                
        }
    }

    private void ManageKeyboardEvents()
    {
        Event ev = Event.current;

        switch (ev.type)
        {
            case EventType.KeyDown:

                switch (ev.keyCode)
                {
                    case KeyCode.LeftShift:
                        isShiftPressed = true;
                        break;
                }
                
                break;
            
            
            case EventType.KeyUp:

                switch (ev.keyCode)
                {
                    case KeyCode.LeftShift:
                        isShiftPressed = false;
                        break;
                }
                
                break;
        }
    }

    private void CreateContextMenu(Event currentEvent, bool isOnNode, bool isOnNodePort, bool isOnNodePortConnection)
    {
        GenericMenu menu = new GenericMenu();

        if (isOnNode)
        {
            if (currentSelectedNode.GetType() != typeof(NodeStarter))
            {
                Node node = currentSelectedNode; //We store it here so we are always sure that DeleteNode has something
                
                menu.AddItem(new GUIContent("Delete Node"), false, () => scriptableObject.DeleteNode(node));
            }
        }
        else if (isOnNodePort)
        {
            NodePort nodePort = currentSelectedNodePort; //Same reason as before
                
            menu.AddItem(new GUIContent("Delete all Connections to this Port"), false, 
                () =>  scriptableObject.DeleteAllConnectionsToNodePort(nodePort));
        }
        else if (isOnNodePortConnection)
        {
            NodePortConnection nodePort = null; //Same reason as before
                
            menu.AddItem(new GUIContent("Delete Connection"), false, 
                () =>  scriptableObject.DeleteConnection(nodePort));
        }
        else
        {
            menu.AddItem(new GUIContent("Add Single Node"), false, () => 
                scriptableObject.AddNode<NodeSingle>(groupRect, currentEvent.mousePosition - groupRect.position) );
            
            menu.AddItem(new GUIContent("Add Binary Node"), false, () => 
                scriptableObject.AddNode<NodeBinary>(groupRect, currentEvent.mousePosition - groupRect.position));
        }
        
        menu.ShowAsContext();
    }

    public void DrawConnectionsBetweenPorts()
    {
        foreach (NodePortConnection nodePortConnection in scriptableObject.NodePortConnections)
        {
            nodePortConnection.Draw();
        }
    }
    
    
}
