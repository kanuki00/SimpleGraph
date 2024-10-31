using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace SimpleGraph
{
    public class GraphEditorWindow : EditorWindow
    {
        //private EditorZoomer editorZoomer;
        private GameObject nodesParent;
        
        private bool isConnecting = false;
        private GraphNode startNode = null;

        private static GameObject tempNodesParent;

        
        // Zoom and pan variables
        
        private float zoomFactor = 1.0f; 
        private Vector2 panOffset = Vector2.zero; 
        private Vector2 scrollPos;
        

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
            HandleZoomAndPan();
            //editorZoomer.Begin();
            // Draw a red border around the entire window
            Color originalColor = GUI.color;
            GUI.color = Color.yellow;
            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none);
            GUI.color = originalColor;

            Matrix4x4 oldMatrix = GUI.matrix;

                        
            GUI.matrix = Matrix4x4.TRS(panOffset, Quaternion.identity, Vector3.one * zoomFactor);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
            
            float contentWidth = position.width / zoomFactor;
            float contentHeight = position.height / zoomFactor;
            
            //GUI.BeginGroup();
            

            
            BeginWindows();
            for (int i = 0; i < GraphUtility.nodes.Count; i++)
            {
                GraphUtility.nodes[i].windowRect = GUI.Window(i, GraphUtility.nodes[i].windowRect, GraphUtility.nodes[i].DrawNodeWindow, GraphUtility.nodes[i].nodeName);
            }
            EndWindows();
            EditorGUILayout.EndScrollView();

            DrawConnections();
            ProcessContextMenu(Event.current);
            if (isConnecting && startNode != null)
            {
                Vector3 startPos = new Vector3(startNode.windowRect.x + startNode.windowRect.width, startNode.windowRect.y + startNode.windowRect.height / 2, 0);
                Vector3 endPos = Event.current.mousePosition;
                Vector3 startTangent = startPos + Vector3.right * 50;
                Vector3 endTangent = endPos + Vector3.left * 50;

                Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.white, null, 3f);
                Repaint();
            }
            GUI.matrix = oldMatrix;
            //editorZoomer.End();
        }
        
        private void HandleZoomAndPan()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                float oldZoom = zoomFactor;
                zoomFactor -= Event.current.delta.y * 0.01f;
                zoomFactor = Mathf.Clamp(zoomFactor, 0.1f, 2.0f);

                Vector2 mousePos = Event.current.mousePosition;
                Vector2 zoomPos = (mousePos - panOffset) / oldZoom;
                panOffset -= zoomPos * (zoomFactor - oldZoom);
                Event.current.Use();
            }

            // Handle panning (middle mouse button drag)
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
            {
                panOffset += Event.current.delta; // Adjust panning speed
                Event.current.Use();
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