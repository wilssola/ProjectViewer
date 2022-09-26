using UnityEngine;

public class AddMeshCollider : MonoBehaviour
{
    private void Awake() {
        foreach (var child in transform.GetComponentsInChildren<Transform>())
        {
            var meshFilter = child.gameObject.GetComponent<MeshFilter>();

            if (meshFilter == null) continue;
            
            var mesh = meshFilter.mesh;

            if (mesh == null) continue;
            
            var meshCollider = child.gameObject.AddComponent<MeshCollider>();
            
            meshCollider.sharedMesh = mesh;
        }
    }
}
