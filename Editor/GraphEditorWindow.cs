using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SimpleGraph
{
    public class GraphEditorWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private GameObject nodesParent;
        
        private bool isConnecting = false;
        private GraphNode startNode = null;
        private float zoomFactor = 1.0f; 
        private Vector2 panOffset = Vector2.zero; 

        [MenuItem("Window/Graph Editor")]
        static void ShowEditor()
        {
            GraphEditorWindow editor = EditorWindow.GetWindow<GraphEditorWindow>();
            editor.titleContent = new GUIContent("Graph Editor");
        }

        private void OnEnable()
        {
            nodesParent = GameObject.Find("SimpleGraphNodes");
            if (nodesParent == null)
            {
                nodesParent = new GameObject("SimpleGraphNodes");
            }
            LoadGraph();
            

            GraphNode.OnSetStartNode += SetStartNode;
            GraphNode.OnSetEndNode += SetEndNode;
        }

        private void SetStartNode(GraphNode node)
        {
            Debug.Log("SetStartNode called with node: " + node.nodeName);
            startNode = node;
            isConnecting = true;
        }

        private void SetEndNode(GraphNode node)
        {
            Debug.Log("SetEndNode called with node: " + node.nodeName);
            if (isConnecting && startNode != null)
            {
                startNode.nextNodes.Add(node); 
                node.previousNodes.Add(startNode); 
                isConnecting = false;
                startNode = null;
            }
        }

        private void OnGUI()
        {
            HandleZoomAndPan(); // Capture scroll wheel input for zooming and mouse drag for panning

            // Apply zoom and pan transformations
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(panOffset, Quaternion.identity, Vector3.one * zoomFactor);

            // Draw the grid
            //DrawGrid(20 * zoomFactor, 0.2f, Color.green);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
            BeginWindows();
            for (int i = 0; i < GraphUtility.nodes.Count; i++)
            {
                GraphUtility.nodes[i].windowRect = GUI.Window(i, GraphUtility.nodes[i].windowRect, GraphUtility.nodes[i].DrawNodeWindow, GraphUtility.nodes[i].nodeName);
            }
            EndWindows();
            EditorGUILayout.EndScrollView();

            DrawConnections();
            ProcessContextMenu(Event.current);

            // Draw the connection line following the cursor
            if (isConnecting && startNode != null)
            {
                Vector3 startPos = new Vector3(startNode.windowRect.x + startNode.windowRect.width, startNode.windowRect.y + startNode.windowRect.height / 2, 0);
                Vector3 endPos = Event.current.mousePosition;
                Vector3 startTangent = startPos + Vector3.right * 50;
                Vector3 endTangent = endPos + Vector3.left * 50;

                Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.white, null, 3f);
                Repaint();
            }

            // Restore the original GUI matrix
            GUI.matrix = oldMatrix;
        }

         private void HandleZoomAndPan()
    {
        // Handle zooming (mouse wheel)
        if (Event.current.type == EventType.ScrollWheel)
        {
            float oldZoom = zoomFactor;
            zoomFactor -= Event.current.delta.y * 0.01f;
            zoomFactor = Mathf.Clamp(zoomFactor, 0.1f, 2.0f);

            Vector2 mousePos = Event.current.mousePosition;
            Vector2 zoomPos = (mousePos - panOffset) / oldZoom;
            panOffset -= zoomPos * (zoomFactor - oldZoom);

            //DrawGrid(100 * zoomFactor, 0.5f, Color.white);

            Event.current.Use();
        }

        // Handle panning (middle mouse button drag)
        if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
        {
            panOffset += Event.current.delta; // Adjust panning speed
            Event.current.Use();
        }

    }
    
    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        // Calculate number of divisions based on window size and grid spacing
        /*
        int widthDivs = Mathf.CeilToInt((position.width * zoomFactor + 2 * gridSpacing) / gridSpacing);
        int heightDivs = Mathf.CeilToInt((position.height * zoomFactor + 2 * gridSpacing) / gridSpacing);
        */

        int widthDivs = Mathf.CeilToInt((position.width * gridSpacing) / gridSpacing);
        int heightDivs = Mathf.CeilToInt((position.height * gridSpacing) / gridSpacing);

        // Begin GUI drawing
        Handles.BeginGUI();

        // Set the grid color with specified opacity
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        // Calculate offset within grid cell to ensure grid "snaps" as you pan
        Vector3 newOffset = new Vector3(panOffset.x % gridSpacing, panOffset.y % gridSpacing, 0);

        // Draw vertical grid lines, extending beyond the view (left and right)
        for (int i = -1; i < widthDivs + 1; i++)
        {
            // The gridSpacing * i + newOffset creates the moving grid based on panOffset
            Handles.DrawLine(
                new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, 
                new Vector3(gridSpacing * i, position.height * zoomFactor + gridSpacing, 0f) + newOffset
            );
        }

        // Draw horizontal grid lines, extending beyond the view (top and bottom)
        for (int j = -1; j < heightDivs + 1; j++)
        {
            Handles.DrawLine(
                new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, 
                new Vector3(position.width * zoomFactor + gridSpacing, gridSpacing * j, 0f) + newOffset
            );
        }

        // Reset the Handles color back to white
        Handles.color = Color.white;

        // End GUI drawing
        Handles.EndGUI();
    }
    

        
    private void LoadGraph()
    {
        Debug.Log("Loading graph from SimpleGraphNodes parent.");

        // Find the SimpleGraphNodes parent GameObject
        GameObject simpleGraphNodesParent = GameObject.Find("SimpleGraphNodes");
        if (simpleGraphNodesParent == null)
        {
            Debug.LogError("SimpleGraphNodes parent GameObject not found.");
            return;
        }

        // Clear the existing nodes
        GraphUtility.nodes.Clear();

        // Iterate through the children of SimpleGraphNodes parent
        foreach (Transform child in simpleGraphNodesParent.transform)
        {
            GraphNode graphNode = child.GetComponent<GraphNode>();            

            if (graphNode != null)
            {
                graphNode.windowRect = new Rect(graphNode.windowRect.x, graphNode.windowRect.y, 200, 150);
                GraphUtility.nodes.Add(graphNode);
            }
        }

        // Refresh the editor window
        Repaint();
    }

        private GraphNode CreateNode(string nodeName, Vector2 position)
        {
            // Create a new GameObject for the node
            GameObject nodeObject = new GameObject(nodeName);
            nodeObject.transform.SetParent(nodesParent.transform);
            nodeObject.transform.position = position;

            // Add the GraphNode component to the GameObject
            GraphNode newNode = nodeObject.AddComponent<GraphNode>();
            newNode.windowRect = new Rect(position.x, position.y, 200, 150);
            newNode.nodeName = nodeName;

            // Add the new node to the GraphUtility nodes list
            GraphUtility.nodes.Add(newNode);

            return newNode;
        }

        private void DrawConnections()
        {
            foreach (GraphNode node in GraphUtility.nodes)
            {
                foreach (GraphNode nextNode in node.nextNodes)
                {
                    DrawNodeCurve(node.windowRect, nextNode.windowRect);
                }
            }
        }

        private void DrawNodeCurve(Rect start, Rect end)
        {
            Vector3 startPos = new Vector3(start.x + start.width, start.y + (start.height / 2), 0); // Start from the right center
            Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0); // End at the left center
            Vector3 startTangent = startPos + Vector3.right * 50;
            Vector3 endTangent = endPos + Vector3.left * 50;

            Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.white, null, 3f);
        }

        private void ProcessContextMenu(Event e)
        {
            if (e.button == 1 && e.type == EventType.MouseDown)
            {
                GraphNode clickedNode = null;
                foreach (GraphNode node in GraphUtility.nodes)
                {
                    if (node.windowRect.Contains(e.mousePosition))
                    {
                        clickedNode = node;
                        break;
                    }
                }

                GenericMenu menu = new GenericMenu();

                if (clickedNode != null)
                {
                    menu.AddItem(new GUIContent("Remove Node"), false, () => GraphUtility.RemoveNode(clickedNode));
                }
                else
                {
                    menu.AddItem(new GUIContent("Add Start Node"), false, () => CreateNode("StartNode", e.mousePosition));
                    menu.AddItem(new GUIContent("Add Inverter Node"), false, () => CreateNode("InverterNode", e.mousePosition));
                    menu.AddItem(new GUIContent("Add Task Node"), false, () => CreateNode("TaskNode", e.mousePosition));
                    menu.AddItem(new GUIContent("Add End Node"), false, () => CreateNode("EndNode", e.mousePosition));
                }

                menu.ShowAsContext();
                e.Use();
            }
        }
    }
}