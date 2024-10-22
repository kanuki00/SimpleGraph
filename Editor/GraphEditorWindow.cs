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
        private float zoomFactor = 1.0f; // Add zoom factor
        private Vector2 panOffset = Vector2.zero; // Add pan offset

        [MenuItem("Window/Graph Editor")]
        static void ShowEditor()
        {
            GraphEditorWindow editor = EditorWindow.GetWindow<GraphEditorWindow>();
            editor.titleContent = new GUIContent("Graph Editor");
        }

        private void OnEnable()
        {
            if (nodesParent == null)
            {
                nodesParent = new GameObject("NodesParent");
            }

            string path = Application.persistentDataPath + "/NodeGraph.json";
            if (File.Exists(path))
            {
                LoadGraph(path);
            }

            GraphNode.OnSetStartNode += SetStartNode;
            GraphNode.OnSetEndNode += SetEndNode;
        }

        private void OnDisable()
        {
            string path = Application.persistentDataPath + "/NodeGraph.json";
            SaveGraph(path);

            GraphNode.OnSetStartNode -= SetStartNode;
            GraphNode.OnSetEndNode -= SetEndNode;
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
                startNode.nextNodes.Add(node); // Add to nextNodes list
                node.previousNodes.Add(startNode); // Add to previousNodes list
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

            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Node Graph"))
            {
                string path = EditorUtility.SaveFilePanel("Save Node Graph", "", "NodeGraph.json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    SaveGraph(path);
                }
            }
            if (GUILayout.Button("Load Node Graph"))
            {
                string path = EditorUtility.OpenFilePanel("Load Node Graph", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    LoadGraph(path);
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
            BeginWindows();
            for (int i = 0; i < GraphUtility.nodes.Count; i++)
            {
                GraphUtility.nodes[i].windowRect = GUI.Window(i, GraphUtility.nodes[i].windowRect, GraphUtility.nodes[i].DrawNodeWindow, GraphUtility.nodes[i].nodeName);
            }
            EndWindows();
            EditorGUILayout.EndScrollView();

            // Draw connection points after the windows
            for (int i = 0; i < GraphUtility.nodes.Count; i++)
            {
                GraphUtility.nodes[i].DrawConnectionPoints();
            }

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
            Event e = Event.current;

            // Handle zoom
            if (e.type == EventType.ScrollWheel)
            {
                float zoomDelta = -e.delta.y * 0.1f;
                zoomFactor = Mathf.Clamp(zoomFactor + zoomDelta, 0.5f, 2.0f);
                e.Use();
            }

            // Handle pan
            if (e.type == EventType.MouseDrag && e.button == 2) // Middle mouse button
            {
                panOffset += e.delta;
                e.Use();
            }
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, position.height, 0f));
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(position.width, gridSpacing * j, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void SaveGraph(string path)
        {
            GraphData graphData = new GraphData();

            foreach (GraphNode node in GraphUtility.nodes)
            {
                GraphNodeData nodeData = new GraphNodeData
                {
                    nodeName = node.nodeName,
                    windowRect = node.windowRect
                };

                foreach (GraphNode connectedNode in node.connectedNodes)
                {
                    int index = GraphUtility.nodes.IndexOf(connectedNode);
                    if (index != -1)
                    {
                        nodeData.connectedNodeIndices.Add(index);
                    }
                }

                graphData.nodes.Add(nodeData);
            }

            string json = JsonUtility.ToJson(graphData, true);
            File.WriteAllText(path, json);
            Debug.Log("Graph saved to " + path);
        }

        private void LoadGraph(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("File not found: " + path);
                return;
            }

            string json = File.ReadAllText(path);
            GraphData graphData = JsonUtility.FromJson<GraphData>(json);

            GraphUtility.nodes.Clear();
            foreach (Transform child in nodesParent.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            foreach (GraphNodeData nodeData in graphData.nodes)
            {
                GraphNode node = CreateNode(nodeData.nodeName, nodeData.windowRect.position);
                node.windowRect = nodeData.windowRect;
            }

            for (int i = 0; i < graphData.nodes.Count; i++)
            {
                GraphNode node = GraphUtility.nodes[i];
                GraphNodeData nodeData = graphData.nodes[i];

                foreach (int connectedNodeIndex in nodeData.connectedNodeIndices)
                {
                    if (connectedNodeIndex >= 0 && connectedNodeIndex < GraphUtility.nodes.Count)
                    {
                        node.connectedNodes.Add(GraphUtility.nodes[connectedNodeIndex]);
                    }
                }
            }

            Debug.Log("Graph loaded from " + path);
        }

        private GraphNode CreateNode(string nodeName, Vector2 position)
        {
            GameObject nodeObject = new GameObject(nodeName);
            nodeObject.transform.SetParent(nodesParent.transform);
            GraphNode newNode = nodeObject.AddComponent<GraphNode>();
            newNode.windowRect = new Rect(position.x, position.y, 200, 100);
            newNode.nodeName = nodeName;
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
            Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
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
