using System;
using UnityEngine;

[Serializable]
public class NodeSingle : Node
{
    private const float TEXT_MARGIN_TOP_BOTTOM = 70;
    private const float TEXT_MARGIN_LEFT_RIGHT = 100;
    
    private const int TEXT_MAX_WIDTH = 400;
    private float linesNumber = 1;
    
    [SerializeField] public string Text = "";
    
    
    public NodeSingle(Rect parentRect, Vector2 position, float widthPercentage, float heightPercentage) 
        : base(parentRect, position, widthPercentage, heightPercentage)
    {
        title = "Single";
        
        NodeInputPorts.Add(new NodeInputPort(this));
        
        NodeOutputPorts.Add(new NodeOutputPort(this));
    }
    
    public override void DrawNode()
    {
        Rect textRect = new Rect(0,0, UsedRect.width - TEXT_MARGIN_LEFT_RIGHT, UsedRect.height - TEXT_MARGIN_TOP_BOTTOM)
        {
            center = UsedRect.center
        };
        
        GUIStyle textStyle = new GUIStyle(GUI.skin.textField)
        {
            wordWrap = true
        };
        
        ResizeNodeBasedOnTextArea(textStyle);
        
        Text = GUI.TextArea(textRect, Text, textStyle);
        
        base.DrawNode();
    }

    private void ResizeNodeBasedOnTextArea(GUIStyle textAreaStyle)
    {
        Vector2 textSize = textAreaStyle.CalcSize(new GUIContent(Text))
                           - new Vector2(textAreaStyle.padding.horizontal, textAreaStyle.padding.vertical); 

        Vector2 center = UsedRect.center;

        float maxTextSize = TEXT_MAX_WIDTH - TEXT_MARGIN_LEFT_RIGHT - textAreaStyle.padding.horizontal;

        if (textSize.x > maxTextSize*linesNumber)
        {
            linesNumber++;
        }
        else if(textSize.x <= maxTextSize*(linesNumber-1))
        {
            linesNumber--;
        }
        
        UsedRect = new Rect(UsedRect.x, UsedRect.y, 
                Mathf.Max(OriginalRect.width,Mathf.Min(textSize.x + TEXT_MARGIN_LEFT_RIGHT + textAreaStyle.padding.horizontal*2,TEXT_MAX_WIDTH)), 
                OriginalRect.height + textAreaStyle.lineHeight * linesNumber); 
        
        
        var rect = UsedRect;
        rect.center = center;
        UsedRect = rect;
    }
}
