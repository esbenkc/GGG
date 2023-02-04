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

        Button animateButton = new Button(() => {
            if(!Application.isPlaying) {
                Debug.Log("Can only animate trail in Play Mode");
                return;
            }
            ((RootTrail)serializedObject.targetObject).StartAnimating();
        });
        animateButton.text = "Animate";

        root.Add(animateButton);

        Button revertButton = new Button(() => {
            if (!Application.isPlaying) {
                Debug.Log("Can only animate reverting trail in Play Mode");
                return;
            }
            ((RootTrail)serializedObject.targetObject).StartReverting();
        });
        revertButton.text = "Revert";

        root.Add(revertButton);

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        return root;
    }
}
