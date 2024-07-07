using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;



//-----------------------------------------------------------------------------------------------------------------
//--------------------------------------- CUSTOM SHAPES CREATOR TOOLS FROM HERE -----------------------------------
//-----------------------------------------------------------------------------------------------------------------

[EditorTool("Cube Creator")]
public class CubeCreatorTool : ShapeCreatorTool
{
    public override void OnActivated()
    {
        primitive = PrimitiveType.Cube;

        base.OnActivated();
    }
}

//TODO: implement other shapes creators






//-----------------------------------------------------------------------------------------------------------------
//--------------------------------------- GENERIC SHAPES CREATOR FROM HERE ----------------------------------------
//-----------------------------------------------------------------------------------------------------------------




//--------------------------------------------- GENERIC OVERLAY ---------------------------------------------------
public class ShapeCreatorOverlay : Overlay
{
    internal VisualElement root;
    
    private ShapeCreatorTool shapeCreatorTool;

    private PrimitiveType primitive;

    internal Button destroyButton;
    
    private bool areTransformButtonsVisible;
    
    internal Button moveButton;
    internal Button rotateButton;
    internal Button scaleButton;
    
    
    internal Toggle localTransformToggle;
    private bool previousLocalTransformToggleValue;
    
    
    public ShapeCreatorOverlay(ShapeCreatorTool shapeCreatorTool,PrimitiveType primitive)
    {
        this.shapeCreatorTool = shapeCreatorTool;
        this.primitive = primitive;
    }
    
    public override VisualElement CreatePanelContent()
    {
        VisualElement root = new VisualElement();
        
        //Build Create and Destroy Buttons
        BuildCreationButtons(root);
        
        //Build Position, Rotation and Scale Buttons
        if (areTransformButtonsVisible)
        {
            localTransformToggle = new Toggle { value = shapeCreatorTool.IsTransformLocal, label = "Local Transform" };
            localTransformToggle.RegisterValueChangedCallback((evt) => shapeCreatorTool.IsTransformLocal = evt.newValue);
            root.Add(localTransformToggle);
            
            BuildTransformButtons(root);
        }
        
        return root;
    }

    private void BuildCreationButtons(VisualElement root)
    {
        Button createButton = new Button() { text = "Create New " +  primitive};
        createButton.clicked += () => shapeCreatorTool.CreateShape();
        createButton.style.marginBottom = 10;
        root.Add(createButton);
        
        
        destroyButton = new Button() { text = "Destroy " +  primitive};
        destroyButton.clicked += () => shapeCreatorTool.DestroyShape();

        if (shapeCreatorTool.IsGameObjectShape(Selection.activeGameObject))
        {
            destroyButton.SetEnabled(true);
        }
        else
        {
            destroyButton.SetEnabled(false);
        }

        destroyButton.style.marginBottom = 25;
        
        root.Add(destroyButton);
    }

    private void BuildTransformButtons(VisualElement root)
    {
        moveButton = new Button() { text = "Move "};
        
        moveButton.clicked += () => 
        {
            shapeCreatorTool.CurrentTransformMode = ShapeCreatorTool.TransformMode.Move;
            
            localTransformToggle.value = previousLocalTransformToggleValue;
            previousLocalTransformToggleValue = localTransformToggle.value;
            
            localTransformToggle.SetEnabled(true);
        };
        
        moveButton.SetEnabled(shapeCreatorTool.CurrentTransformMode != ShapeCreatorTool.TransformMode.Move);
        moveButton.style.marginTop = 10;
        moveButton.style.marginBottom = 10;
        root.Add(moveButton);
        
        
        
        rotateButton = new Button() { text = "Rotate "};
        
        rotateButton.clicked += () => 
        {
            shapeCreatorTool.CurrentTransformMode = ShapeCreatorTool.TransformMode.Rotate;
            
            
            localTransformToggle.value = previousLocalTransformToggleValue;
            previousLocalTransformToggleValue = localTransformToggle.value;
            
            localTransformToggle.SetEnabled(true);
        };
        
        rotateButton.SetEnabled(shapeCreatorTool.CurrentTransformMode != ShapeCreatorTool.TransformMode.Rotate);
        rotateButton.style.marginBottom = 10;
        root.Add(rotateButton);
        
        
        
        scaleButton = new Button() { text = "Scale "};
        
        scaleButton.clicked += () => 
        {
            shapeCreatorTool.CurrentTransformMode = ShapeCreatorTool.TransformMode.Scale;
            
            //We save the previous value of the toggle to restore it later.
            previousLocalTransformToggleValue = localTransformToggle.value;
            
            //The scale based on local transform is always true.
            //With this system, it does not make sense to do it based on global transform.
            shapeCreatorTool.IsTransformLocal = true;
            localTransformToggle.value = true;
            
            localTransformToggle.SetEnabled(false);
            
        };
        
        scaleButton.SetEnabled(shapeCreatorTool.CurrentTransformMode != ShapeCreatorTool.TransformMode.Scale);
        
        if (shapeCreatorTool.CurrentTransformMode == ShapeCreatorTool.TransformMode.Scale)
        {
            //The scale based on local transform is always true.
            //With this system, it does not make sense to do it based on global transform.
            shapeCreatorTool.IsTransformLocal = true;
            localTransformToggle.value = true;
            
            localTransformToggle.SetEnabled(false);
        }
        
        scaleButton.style.marginBottom = 10;
        root.Add(scaleButton);



        foreach (Button transformButton in new[] { moveButton, rotateButton, scaleButton })
        {
            transformButton.clicked += () => 
            {
                moveButton.SetEnabled(shapeCreatorTool.CurrentTransformMode != ShapeCreatorTool.TransformMode.Move);
                rotateButton.SetEnabled(shapeCreatorTool.CurrentTransformMode != ShapeCreatorTool.TransformMode.Rotate);
                scaleButton.SetEnabled(shapeCreatorTool.CurrentTransformMode != ShapeCreatorTool.TransformMode.Scale);
            };
        }
    }

    internal void BuildOverlayAndSetTransformButtonsVisibility(bool areTransformButtonsVisible)
    {
        SceneView.RemoveOverlayFromActiveView(this);
        
        this.areTransformButtonsVisible = areTransformButtonsVisible;
        CreatePanelContent();
        
        
        SceneView.AddOverlayToActiveView(this);
    }
}



//--------------------------------------------- GENERIC TOOL ---------------------------------------------------
public abstract class ShapeCreatorTool : EditorTool
{
    
    //--------------------- Graphics Parameters -------------------------
    
    private readonly float DISTANCE_FROM_EDITOR_VIEW = 10f;
    
    private readonly float ARROW_SIZE = 1f;
    private readonly float ARROW_POSITION_OFFSET = 0.2f;

    private readonly Color ARROW_X_COLOR = Color.red;
    private readonly Color ARROW_Y_COLOR = Color.green;
    private readonly Color ARROW_Z_COLOR = Color.blue;
    
    private readonly Color DISC_X_COLOR = Color.red;
    private readonly Color DISC_Y_COLOR = Color.green;
    private readonly Color DISC_Z_COLOR = Color.blue;
    
    //--------------------------------------------------------------------
    
    
    internal enum TransformMode
    {
        Move,
        Rotate,
        Scale
    }

    private struct VectorsAtBounds
    {
        public Vector3 Up;
        public Vector3 Down;
        public Vector3 Right;
        public Vector3 Left;
        public Vector3 Forward;
        public Vector3 Back;
    }
    
    protected PrimitiveType primitive;
    
    private ShapeCreatorOverlay overlay;
    
    private MeshRenderer selectedShape;
    
    
    
    internal TransformMode CurrentTransformMode;
    internal bool IsTransformLocal;
    
    

    public override void OnActivated()
    {
        overlay = new ShapeCreatorOverlay(this, primitive);
        
        if (IsGameObjectShape(Selection.activeGameObject))
        {
            overlay.BuildOverlayAndSetTransformButtonsVisibility(true);
            
            selectedShape = Selection.activeGameObject.GetComponent<MeshRenderer>();
        }
        else
        {
            overlay.BuildOverlayAndSetTransformButtonsVisibility(false);
        }
        
        Selection.selectionChanged += OnSelectionChanged;
        
    }

    public override void OnWillBeDeactivated()
    {
        SceneView.RemoveOverlayFromActiveView(overlay);
    }

    private void OnSelectionChanged()
    {
        if (!ToolManager.IsActiveTool(this))
        {
            return;
        }
        
        if (IsGameObjectShape(Selection.activeGameObject))
        {
            overlay.BuildOverlayAndSetTransformButtonsVisibility(true);
            overlay.destroyButton.SetEnabled(true);

            selectedShape = Selection.activeGameObject.GetComponent<MeshRenderer>();
        }
        else
        {
            overlay.BuildOverlayAndSetTransformButtonsVisibility(false);
            overlay.destroyButton.SetEnabled(false);

            selectedShape = null;
        }
    }

    internal bool IsGameObjectShape(GameObject gameObject)
    {
        if (gameObject is null)
        {
            return false;
        }

        GameObject objectToCheckGo = gameObject.GameObject();

        if (objectToCheckGo is null)
        {
            return false;
        }
        
        objectToCheckGo.TryGetComponent(out MeshFilter meshFilter);

        if (meshFilter is null)
        {
            return false;
        }

        if (!Enum.GetNames(typeof(PrimitiveType)).Contains(meshFilter.sharedMesh.name.Replace(" Instance","")))
        {
            return false;
        }

        return true;
    }

    public override void OnToolGUI(EditorWindow window)
    {

        if (window is not SceneView)
        {
            SceneView.RemoveOverlayFromActiveView(overlay);
            return;
        }


        if (!ToolManager.IsActiveTool(this))
        {
            SceneView.RemoveOverlayFromActiveView(overlay);
            return;
        }

        if (!selectedShape)
        {
            return;
        }
        
        
        if (CurrentTransformMode == TransformMode.Move)
        {
            ManageMoveShape(IsTransformLocal);
        }
        else if (CurrentTransformMode == TransformMode.Rotate)
        {
            ManageRotateShape(IsTransformLocal);
        }
        else if (CurrentTransformMode == TransformMode.Scale)
        {
            //The scale based on local transform is always true.
            //With this system, it does not make sense to do it based on global transform.
            ManageScaleShape(true);
        }
        
    }

    internal void CreateShape()
    {
        GameObject shape = GameObject.CreatePrimitive(primitive);
        
        SceneView sceneView = SceneView.lastActiveSceneView;

        if (sceneView is null)
        {
            return;
        }
        
        Vector3 viewPosition = sceneView.camera.transform.position;
        Vector3 viewDirection = sceneView.camera.transform.forward;
        
        Vector3 targetPosition = viewPosition + viewDirection * DISTANCE_FROM_EDITOR_VIEW;
        
        shape.transform.position = targetPosition;

        DestroyImmediate(shape.GetComponent<Collider>());

        Selection.activeObject = shape;
    }

    internal void DestroyShape()
    {
        DestroyImmediate(Selection.activeObject);
    }
    
    private VectorsAtBounds GetShapePositionsAtBounds(float offset,bool isLocal)
    {
        Vector3 center = isLocal ? selectedShape.transform.localPosition : selectedShape.transform.position;
        Bounds bounds = selectedShape.bounds;
        
        return new VectorsAtBounds
        {
            Up = center + (isLocal? selectedShape.transform.up : Vector3.up) * (bounds.extents.y + offset),
            
            Down = center + (isLocal? -selectedShape.transform.up : Vector3.down) * (bounds.extents.y + offset),
            
            Right = center + (isLocal? selectedShape.transform.right : Vector3.right) * (bounds.extents.x + offset),
            
            Left = center + (isLocal? -selectedShape.transform.right : Vector3.left) * (bounds.extents.x + offset),
            
            Forward = center + (isLocal? selectedShape.transform.forward : Vector3.forward) * (bounds.extents.z + offset),
            
            Back = center + (isLocal? -selectedShape.transform.forward : Vector3.back) * (bounds.extents.z + offset)
        };
    }

    private void ManageMoveShape(bool isLocal)
    {
        Color originalColor = Handles.color;
        
        VectorsAtBounds vectorsAtBounds = GetShapePositionsAtBounds(ARROW_POSITION_OFFSET,isLocal);
        
        EditorGUI.BeginChangeCheck();
            
        //--------------------------- X axis ---------------------------
        
        Handles.color = ARROW_X_COLOR;
        Vector3 newPositionX = Handles.Slider(vectorsAtBounds.Right, 
            isLocal? selectedShape.transform.right : Vector3.right, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Right) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f);

        
        //--------------------------- Y axis ---------------------------
        
        Handles.color = ARROW_Y_COLOR;
        Vector3 newPositionY = Handles.Slider(vectorsAtBounds.Up, 
            isLocal? selectedShape.transform.up : Vector3.up, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Up) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f);
        
        
        //--------------------------- Z axis ---------------------------
            
        Handles.color = ARROW_Z_COLOR;
        Vector3 newPositionZ = Handles.Slider(vectorsAtBounds.Forward, 
            isLocal? selectedShape.transform.forward : Vector3.forward, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Forward) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f);
            
            
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(selectedShape.transform, "Move Object");

            if (!Mathf.Approximately(vectorsAtBounds.Right.x, newPositionX.x))
            {
                if (isLocal)
                {
                    selectedShape.transform.localPosition = newPositionX;
                }
                else
                {
                    selectedShape.transform.position = newPositionX;
                }
                
            }
                
            else if (!Mathf.Approximately(vectorsAtBounds.Up.y, newPositionY.y))
            {
                selectedShape.transform.position = newPositionY;
            }
                
            else if (!Mathf.Approximately(vectorsAtBounds.Forward.z, newPositionZ.z))
            {
                selectedShape.transform.position = newPositionZ;
            }
        }
        
        Handles.color = originalColor;
    }
    
    private void ManageRotateShape(bool isLocal)
    {
        Color originalColor = Handles.color;
        
        VectorsAtBounds vectorsAtBounds = GetShapePositionsAtBounds(ARROW_POSITION_OFFSET,isLocal);
        
        Vector3 position = selectedShape.transform.position;
        Quaternion rotation = selectedShape.transform.rotation;
        
        EditorGUI.BeginChangeCheck();
        
        //--------------------------- X axis ---------------------------
        
        Handles.color = DISC_X_COLOR;

        Quaternion newRotationX = Handles.Disc(rotation, vectorsAtBounds.Right, 
            isLocal? selectedShape.transform.right : Vector3.right, 
            HandleUtility.GetHandleSize(position), false, 0f);
        
        
        //--------------------------- Y axis ---------------------------
        
        Handles.color = DISC_Y_COLOR;
        
        Quaternion newRotationY = Handles.Disc(rotation, vectorsAtBounds.Up, 
            isLocal? selectedShape.transform.up : Vector3.up, 
            HandleUtility.GetHandleSize(position), false, 0f);
        
        
        //--------------------------- Z axis ---------------------------
        
        Handles.color = DISC_Z_COLOR;
        
        Quaternion newRotationZ = Handles.Disc(rotation, vectorsAtBounds.Forward, 
            isLocal? selectedShape.transform.forward : Vector3.forward, 
            HandleUtility.GetHandleSize(position), false, 0f);



        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(selectedShape.transform, "Rotate Object");

            if (newRotationX != rotation)
            {
                selectedShape.transform.rotation = newRotationX;
            }
            else if (newRotationY != rotation)
            {
                selectedShape.transform.rotation = newRotationY;
            }
            else if(newRotationZ != rotation)
            {
                selectedShape.transform.rotation = newRotationZ;
            }
        }

        Handles.color = originalColor;
    }
    
    private void ManageScaleShape(bool isLocal)
    {
        Color originalColor = Handles.color;
        
        VectorsAtBounds vectorsAtBounds = GetShapePositionsAtBounds(ARROW_POSITION_OFFSET,isLocal);
        
        EditorGUI.BeginChangeCheck();
        
        //--------------------------- X axis ---------------------------
            
        Handles.color = ARROW_X_COLOR;
        
        float newScalePositiveX = Handles.Slider(vectorsAtBounds.Right, 
            isLocal? selectedShape.transform.right : Vector3.right, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Right) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f).x;
        
        float newScaleNegativeX = Handles.Slider(vectorsAtBounds.Left, 
            isLocal? -selectedShape.transform.right : Vector3.left,
            HandleUtility.GetHandleSize(vectorsAtBounds.Right) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f).x;

        
        //--------------------------- Y axis ---------------------------
        
        Handles.color = ARROW_Y_COLOR;
        
        float newScalePositiveY = Handles.Slider(vectorsAtBounds.Up, 
             isLocal? selectedShape.transform.up : Vector3.up, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Up) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f).y;
        
        float newScaleNegativeY = Handles.Slider(vectorsAtBounds.Down, 
             isLocal? -selectedShape.transform.up : Vector3.down, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Up) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f).y;
        
        
        //--------------------------- Z axis ---------------------------
        
        Handles.color = ARROW_Z_COLOR;
        
        float newScalePositiveZ = Handles.Slider(vectorsAtBounds.Forward, 
            isLocal? selectedShape.transform.forward : Vector3.forward, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Forward) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f).z;
        
        float newScaleNegativeZ = Handles.Slider(vectorsAtBounds.Back, 
            isLocal? -selectedShape.transform.forward : Vector3.back, 
            HandleUtility.GetHandleSize(vectorsAtBounds.Forward) * ARROW_SIZE, 
            Handles.ArrowHandleCap,0f).z;
            
            
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(selectedShape.transform, "Scale Object");

            if (!Mathf.Approximately(vectorsAtBounds.Right.x, newScalePositiveX))
            {
                selectedShape.transform.position += 
                    selectedShape.transform.right * (newScalePositiveX - vectorsAtBounds.Right.x) / 2;
                
                selectedShape.transform.localScale += 
                    Vector3.right * (newScalePositiveX - vectorsAtBounds.Right.x);
            }
            
            else if (!Mathf.Approximately(vectorsAtBounds.Left.x, newScaleNegativeX))
            {
                selectedShape.transform.position -= 
                    -selectedShape.transform.right * (newScaleNegativeX - vectorsAtBounds.Left.x) / 2;
                
                selectedShape.transform.localScale += 
                    Vector3.left * (newScaleNegativeX - vectorsAtBounds.Left.x);
            }
            
            else if (!Mathf.Approximately(vectorsAtBounds.Up.y, newScalePositiveY))
            {
                selectedShape.transform.position += 
                    selectedShape.transform.up * (newScalePositiveY - vectorsAtBounds.Up.y) / 2;
                
                selectedShape.transform.localScale += 
                    Vector3.up * (newScalePositiveY - vectorsAtBounds.Up.y);
            }
            
            else if (!Mathf.Approximately(vectorsAtBounds.Down.y, newScaleNegativeY))
            {
                selectedShape.transform.position -= 
                    -selectedShape.transform.up * (newScaleNegativeY - vectorsAtBounds.Down.y) / 2;
                
                selectedShape.transform.localScale += 
                    Vector3.down * (newScaleNegativeY - vectorsAtBounds.Down.y);
            }
            
            else if (!Mathf.Approximately(vectorsAtBounds.Forward.z, newScalePositiveZ))
            {
                selectedShape.transform.position += 
                    selectedShape.transform.forward * (newScalePositiveZ - vectorsAtBounds.Forward.z) / 2;
                
                selectedShape.transform.localScale += 
                    Vector3.forward * (newScalePositiveZ - vectorsAtBounds.Forward.z);
            }
            
            else if (!Mathf.Approximately(vectorsAtBounds.Back.z, newScaleNegativeZ))
            {
                selectedShape.transform.position -= 
                    -selectedShape.transform.forward * (newScaleNegativeZ - vectorsAtBounds.Back.z) / 2;
                
                selectedShape.transform.localScale += 
                    Vector3.back * (newScaleNegativeZ - vectorsAtBounds.Back.z);
            }
        }
        
        Handles.color = originalColor;
    }
    
}

