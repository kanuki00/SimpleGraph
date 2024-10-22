using UnityEngine;

namespace SimpleGraph {
    public class StartNode : GraphNode  {
            public override void DrawNodeWindow(int id) {
                GUILayout.BeginVertical();
                //GUILayout.Label("Start Node Header", EditorStyles.boldLabel);
                GUILayout.EndVertical();

                base.DrawNodeWindow(id);
        }
    }
}