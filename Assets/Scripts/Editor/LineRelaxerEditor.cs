using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(LineRelaxer))]
public class LineRelaxerEditor : Editor
{
    public override VisualElement CreateInspectorGUI() {
        VisualElement root = new VisualElement();

        Button button = new Button(() => {
            if(!Application.isPlaying) {
                Debug.Log("Can only relax in Play Mode");
                return;
            }
            ((LineRelaxer)serializedObject.targetObject).Relax();
        });
        button.text = "Relax";

        root.Add(button);

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        return root;
    }
}
