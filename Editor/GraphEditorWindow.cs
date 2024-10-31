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
        private Rect _zoomArea;
        private const float zoomAreaScale = 10.0f;
        private Vector2 panOffset = Vector2.zero; 
        private static GameObject tempNodesParent;

        

        public static void ShowEditor(GameObject nodesParent)
        {
            tempNodesParent = nodesParent;
            GraphEditorWindow editor = EditorWindow.GetWindow<GraphEditorWindow>();
            editor.titleContent = new GUIContent(nodesParent.name);
        }

        private void OnEnable()
        {   
            nodesParent = tempNodesParent;
            tempNodesParent = null;

            LoadGraph(nodesParent);
            GraphNode.OnSetStartNode += SetStartNode;
            GraphNode.OnSetEndNode += SetEndNode;
        }

        private void SetStartNode(GraphNode node)
        {
            startNode = node;
            if (isConnecting) {
                isConnecting = false;
            } else {
                isConnecting = true;
            }   
        }

        private void SetEndNode(GraphNode node)
        {
            if (isConnecting && startNode != null)
            {
                startNode.nextNodes.Add(node); 
                node.previousNodes.Add(startNode); 
                isConnecting = false;
                startNode = null;
            }
            else {
                if (node.previousNodes.Count > 0)
                {
                    GraphNode startNode = node.previousNodes[0];
                    startNode.nextNodes.Remove(node);
                    node.previousNodes.Remove(startNode);
                }
                Repaint();
            }
        }

        private void OnGUI()
        {
        // Handle zooming and panning
        HandleZoomAndPan();

        // Save the current GUI matrix
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.ScaleAroundPivot(Vector2.one * zoomFactor, new Vector2(position.width / 2f, position.height / 2f));
        GUI.matrix = Matrix4x4.TRS(panOffset, Quaternion.identity, Vector3.one) * GUI.matrix;

        BeginWindows();
        for (int i = 0; i < GraphUtility.nodes.Count; i++)
        {
            GraphUtility.nodes[i].windowRect = GUI.Window(
                i,
                GraphUtility.nodes[i].windowRect,
                GraphUtility.nodes[i].DrawNodeWindow,
                GraphUtility.nodes[i].nodeName
            );
        }
        EndWindows();

        DrawConnections();

        // Restore the original GUI matrix
        GUI.matrix = oldMatrix;

        // Process events
        ProcessContextMenu(Event.current);
        }

        private void HandleZoomAndPan()
        {
            Event e = Event.current;

            // Handle zooming with the scroll wheel
            if (e.type == EventType.ScrollWheel)
            {
                Vector2 mousePosition = e.mousePosition;
                float zoomDelta = -e.delta.y * 0.01f;
                float prevZoom = zoomFactor;
                zoomFactor = Mathf.Clamp(zoomFactor + zoomDelta, 0.5f, 2f);

                // Adjust panOffset to zoom around the mouse position
                Vector2 offset = mousePosition - new Vector2(position.width / 2f, position.height / 2f);
                panOffset -= offset * (zoomFactor - prevZoom) / zoomFactor;

                e.Use();
            }

            // Handle panning with middle mouse button drag
            if (e.type == EventType.MouseDrag && e.button == 2)
            {
                panOffset += e.delta;
                e.Use();
            }
        }
        private void LoadGraph(GameObject nodesParent)
        {
            GraphUtility.nodes.Clear();
            if (nodesParent == null)
            {
                return;
            }

            foreach (Transform child in nodesParent.transform)
            {
                GraphNode graphNode = child.GetComponent<GraphNode>();            

                if (graphNode != null)
                {
                    graphNode.windowRect = new Rect(graphNode.windowRect.x, graphNode.windowRect.y, 200, 150);
                    GraphUtility.nodes.Add(graphNode);
                }
            }
            
            Repaint();
        }

        private GraphNode CreateNode(string nodeName, Vector2 position)
        {
            // Create a new GameObject
            GameObject nodeObject = new GameObject(nodeName);
            nodeObject.transform.SetParent(nodesParent.transform);
            nodeObject.transform.position = position;

            // Add the GraphNode component
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