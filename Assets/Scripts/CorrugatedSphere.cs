using UnityEngine;

public class CorrugatedSphere : MonoBehaviour
{
    // Number of vertices along each dimension of the sphere
    public int resolution = 50;

    // Initial radius of the sphere
    public float initialRadius = 1f;

    // Initial amplitude of the wave deformation
    public float initialWaveAmplitude = 0.5f;

    // Frequency of the wave deformation
    public float waveFrequency = 2f;

    // Material for rendering the sphere
    public Material sphereMaterial;

    // Speed at which the size of the sphere changes over time
    public float sizeChangeSpeed = 0.1f;

    // Current radius of the sphere (changing over time)
    private float currentRadius;

    // Current amplitude of the wave deformation (changing over time)
    private float currentWaveAmplitude;

    void Start()
    {
        // Initialize current radius and wave amplitude
        currentRadius = initialRadius;
        currentWaveAmplitude = initialWaveAmplitude;

        // Create the corrugated sphere
        CreateCorrugatedSphere();
    }

    void Update()
    {
        // Decrease the size of the sphere over time
       // currentRadius -= sizeChangeSpeed * Time.deltaTime;
        //currentRadius = Mathf.Max(currentRadius, 0.1f); // Ensure a minimum radius

        // Modify the wave amplitude to create a hat-like shape
        //currentWaveAmplitude = Mathf.Sin(Time.time) * initialWaveAmplitude;

        // Recreate the sphere with updated parameters
        //CreateCorrugatedSphere();
    }

    void CreateCorrugatedSphere()
    {
        // Access the MeshFilter component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        // Arrays to store vertex positions, UV coordinates, and normals
        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        // Loop through each vertex in the sphere
        for (int i = 0; i <= resolution; i++)
        {
            for (int j = 0; j <= resolution; j++)
            {
                // Calculate spherical coordinates
                float u = (float)j / resolution;
                float v = (float)i / resolution;
                float theta = 2f * Mathf.PI * u;
                float phi = Mathf.PI * v;

                // Compute Cartesian coordinates with wave deformation
                float x = currentRadius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = currentRadius * Mathf.Cos(phi) + currentWaveAmplitude * Mathf.Sin(waveFrequency * theta);
                float z = currentRadius * Mathf.Sin(phi) * Mathf.Sin(theta);

                int index = i * (resolution + 1) + j;

                // Store vertex position, UV coordinate, and normal
                vertices[index] = new Vector3(x, y, z);
                uv[index] = new Vector2(u, v);
                normals[index] = new Vector3(x, y, z).normalized;
            }
        }

        // Arrays to store triangle indices
        int[] triangles = new int[resolution * resolution * 6];
        int triangleIndex = 0;

        // Loop through each face of the sphere
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int vertexIndex = i * (resolution + 1) + j;

                // Define triangles for the current face
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                triangles[triangleIndex + 2] = vertexIndex + 1;

                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = vertexIndex + resolution + 1;
                triangles[triangleIndex + 5] = vertexIndex + resolution + 2;

                triangleIndex += 6;
            }
        }

        // Assign computed values to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;

        // Assign the material to the MeshRenderer component
        GetComponent<MeshRenderer>().material = sphereMaterial;
    }
}
