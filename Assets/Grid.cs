using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

	public int width, height;

	private Mesh mesh;
	private Vector3[] vertices;
    public FFTOcean fft;

    private void Start () {
        StartCoroutine(Banana());
	}

    private IEnumerator Banana()
    {
        yield return new WaitForSeconds(1);
        InstantiateElement("OceanPlane", width, height, fft.renderingShader_Material);
        yield return null;
    }

	private void Generate (int xSize, int ySize) {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		vertices = new Vector3[(xSize + 1) * (ySize + 1)];
		for (int i = 0, y = 0; y <= ySize; y++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, 0, y);
			}
		}
		mesh.vertices = vertices;

		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
	}

    Element InstantiateElement(string name, int xSize, int ySize, Material mat)
    {
        Generate(xSize, ySize);
        transform.gameObject.name = name;
        transform.gameObject.transform.SetParent(transform);
        transform.gameObject.transform.localPosition = Vector3.zero;
        MeshRenderer meshRenderer = transform.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mat;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = true;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
        meshRenderer.allowOcclusionWhenDynamic = false;
        return new Element(transform.gameObject.transform, meshRenderer);
    }
}