using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(RootTrail))]
public class RootTrailEditor : Editor
{
    public override VisualElement CreateInspectorGUI() {
        VisualElement root = new VisualElement();

        Button button = new Button(() => {
            if(!Application.isPlaying) {
                Debug.Log("Can only animate trail in Play Mode");
                return;
            }
            ((RootTrail)serializedObject.targetObject).StartAnimating();
        });
        button.text = "Animate";

        root.Add(button);

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        return root;
    }
}
