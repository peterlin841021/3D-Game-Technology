using UnityEngine;

public class RenderInfo : MonoBehaviour
{	
	[Range(0, 1)]
	public float lambertCoefficient = 1f;
	[Range(0, 1)]
	public float reflectiveCoefficient = 0f;//反射率
	[Range(0, 1)]
	public float transparentCoefficient = 0f;//透明度

	// Phong variables. 
	public float phongCoefficient = 1f;
	public float phongPower = 2f;

	// Blinn variables.
	public float blinnPhongCoefficient = 0f;
	public float blinnPhongPower = 2f;
}
