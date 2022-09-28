using UnityEngine;

[ExecuteAlways]
public class ClippingPlane : MonoBehaviour {
	public Material mat;
	
	private readonly int _plane = Shader.PropertyToID("_Plane");

	private void Update () {
		var plane = new Plane(transform.up, transform.position);
		var planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
		mat.SetVector(_plane, planeRepresentation);
	}
}

