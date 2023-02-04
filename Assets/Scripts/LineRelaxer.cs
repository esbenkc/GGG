using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LineRelaxer : MonoBehaviour
{
    private class GOHierarchy {
        public HashSet<GameObject> gos;

        public GOHierarchy() {
            gos = new HashSet<GameObject>();
        }

        public void Add(GameObject go) {
            if (go == null) return;
            if (gos.Contains(go)) return;
            gos.Add(go);
            var parent = go.transform.parent;
            if (parent != null)
                Add(parent.gameObject);
        }
    }

    [SerializeField]
    LineRenderer lineRenderer;

    [SerializeField]
    float relaxDuration = 2, stepSize = 0.1f;

    public void Relax() {
        Relax(lineRenderer, relaxDuration, stepSize);
    }

    public static void Relax(LineRenderer renderer, float relaxDuration, float stepSize) {
        List<Vector2> worldPoints = new List<Vector2>();

        for (int i = 0; i < renderer.positionCount; i++) {
            Vector2 pointPosition = renderer.GetPosition(i);
            Vector2 position =
                renderer.useWorldSpace ?
                pointPosition : renderer.transform.localToWorldMatrix * new Vector4(pointPosition.x, pointPosition.y, 0, 1);
            worldPoints.Add(position);
        }
        Relax(worldPoints, renderer, relaxDuration, stepSize);
    }

    public static void Relax(List<Vector2> worldPoints, LineRenderer renderer, float relaxDuration, float stepSize) {
        var scene = SceneManager.CreateScene("TempSim" + renderer.GetHashCode(), new CreateSceneParameters() { localPhysicsMode = LocalPhysicsMode.Physics2D });
        var physicsScene = scene.GetPhysicsScene2D();

        var goRoot = new GameObject("Root");
        SceneManager.MoveGameObjectToScene(goRoot, scene);

        // Duplicate scene colliders

        var cols = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        GOHierarchy hierarchy = new GOHierarchy();

        foreach (var col in cols) {
            var go = col.gameObject;
            hierarchy.Add(col.gameObject);
        }

        Dictionary<GameObject, GameObject> mapping = new Dictionary<GameObject, GameObject>();

        foreach(var go in hierarchy.gos) {
            var newGo = new GameObject(go.name);
            SceneManager.MoveGameObjectToScene(newGo, scene);
            var oldTransform = go.transform;
            var newTransform = newGo.transform;
            newTransform.localPosition = oldTransform.localPosition;
            newTransform.localRotation = oldTransform.localRotation;
            newTransform.localScale = oldTransform.localScale;
            mapping[go] = newGo;

            foreach (var goCol in go.GetComponents<Collider2D>()) {
                Collider2D col = null, newCol = null;
                if (goCol is PolygonCollider2D polyCol) {
                    col = polyCol;
                    var newColPoly = newGo.AddComponent<PolygonCollider2D>();
                    newCol = newColPoly;
                    newColPoly.points = polyCol.points;
                }
                if (goCol is CircleCollider2D circCol) {
                    col = circCol;
                    var newColCirc = newGo.AddComponent<CircleCollider2D>();
                    newCol = newColCirc;
                    newColCirc.radius = circCol.radius;
                }
                if (goCol is BoxCollider2D boxCol) {
                    col = boxCol;
                    var newColBox = newGo.AddComponent<BoxCollider2D>();
                    newCol = newColBox;
                    newColBox.size = boxCol.size;
                }
                if (col == null)
                    throw new System.Exception("Something went wrong, found no col!");
                if (newCol == null)
                    throw new System.Exception("Something went wrong, found no new col!");
                newCol.offset = col.offset;
                newCol.isTrigger = col.isTrigger;
                newCol.usedByComposite = col.usedByComposite;
                newCol.usedByEffector = col.usedByEffector;
            }
        }

        foreach(var (go, newGo) in mapping) {
            var parent = go.transform.parent;
            if (parent != null)
                newGo.transform.SetParent(mapping[parent.gameObject].transform, false);
        }

        // Create new physics objects to simulate trail
        GameObject[] objs = new GameObject[worldPoints.Count];
        Rigidbody2D[] rbs = new Rigidbody2D[objs.Length];
        for (int i = 0; i < objs.Length; i++) {
            var obj = objs[i] = new GameObject("Temp");
            SceneManager.MoveGameObjectToScene(obj, scene);
            var pointPosition = renderer.GetPosition(i);
            obj.transform.position = worldPoints[i];

            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.1f;

            var rb = rbs[i] = obj.AddComponent<Rigidbody2D>();
            rb.isKinematic = i == 0 || i == objs.Length - 1;
        }
        for (int i = 0; i < objs.Length - 1; i++) {
            var obj1 = objs[i];
            var obj2 = objs[i+1];

            var joint = obj1.AddComponent<DistanceJoint2D>();
            joint.connectedBody = rbs[i + 1];
            joint.distance = ((Vector2)obj1.transform.position - (Vector2)obj2.transform.position).magnitude;
        }

        int steps = Mathf.CeilToInt(relaxDuration / stepSize);

        for (int i = 0; i < steps; i++) {
            physicsScene.Simulate(stepSize);
        }
        renderer.positionCount = objs.Length;
        for (int i = 0; i < objs.Length; i++) {
            Vector2 tempPos = objs[i].transform.position;
            Vector2 position =
                renderer.useWorldSpace ?
                 tempPos : renderer.transform.worldToLocalMatrix * new Vector4(tempPos.x, tempPos.y, 0, 1);
            renderer.SetPosition(i, position);
        }
        SceneManager.UnloadSceneAsync(scene);
    }
}
