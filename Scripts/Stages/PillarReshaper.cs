using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PillarReshaper : MonoBehaviour
{
    private Mesh mesh;
    private MeshCollider meshCollider;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        TryGetComponent(out meshCollider);
        
        foreach(var vertex in mesh.vertices)
        {
            print(vertex);
        }

        GenerateInitialShape();
    }

    private void GenerateInitialShape()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define vertices for each face separately to allow for independent texturing and normals
        Vector3[] vertices = new Vector3[24];

        float width = 1f;  // Adjust as needed
        float height = 1f; // Adjust as needed for a pillar
        float depth = 1f;  // Adjust as needed

        // Adjusting the vertex array to include vertices for the sides

        // Bottom face vertices
        vertices[3] = new Vector3(-width / 2, -height / 2, depth / 2);
        vertices[2] = new Vector3(width / 2, -height / 2, depth / 2);
        vertices[1] = new Vector3(width / 2, -height / 2, -depth / 2);
        vertices[0] = new Vector3(-width / 2, -height / 2, -depth / 2);

        // Top face vertices
        vertices[7] = new Vector3(-width / 2, height / 2, depth / 2);
        vertices[6] = new Vector3(width / 2, height / 2, depth / 2);
        vertices[5] = new Vector3(width / 2, height / 2, -depth / 2);
        vertices[4] = new Vector3(-width / 2, height / 2, -depth / 2);

        // Define side faces with separate vertices for each face
        // Front face
        vertices[8] = new Vector3(-width / 2, -height / 2, depth / 2);
        vertices[9] = new Vector3(width / 2, -height / 2, depth / 2);
        vertices[10] = new Vector3(width / 2, height / 2, depth / 2);
        vertices[11] = new Vector3(-width / 2, height / 2, depth / 2);

        // Back face
        vertices[15] = new Vector3(width / 2, -height / 2, -depth / 2);
        vertices[14] = new Vector3(-width / 2, -height / 2, -depth / 2);
        vertices[13] = new Vector3(-width / 2, height / 2, -depth / 2);
        vertices[12] = new Vector3(width / 2, height / 2, -depth / 2);

        // Left face -> +x 축 방향으로 바라보는 면
        vertices[16] = new Vector3(-width / 2, -height / 2, -depth / 2);
        vertices[17] = new Vector3(-width / 2, -height / 2, depth / 2);
        vertices[18] = new Vector3(-width / 2, height / 2, depth / 2);
        vertices[19] = new Vector3(-width / 2, height / 2, -depth / 2);

        // Right face
        vertices[23] = new Vector3(width / 2, -height / 2, depth / 2);
        vertices[22] = new Vector3(width / 2, -height / 2, -depth / 2);
        vertices[21] = new Vector3(width / 2, height / 2, -depth / 2);
        vertices[20] = new Vector3(width / 2, height / 2, depth / 2);

        // Define the triangles, ensuring correct winding order for visibility
        int[] triangles = new int[]
        {
    // Bottom
    0, 1, 2, 0, 2, 3,
    // Top
    4, 6, 5, 4, 7, 6,
    // Front
    8, 9, 10, 8, 10, 11,
    // Back
    12, 14, 13, 12, 15, 14,
    // Left
    16, 17, 18, 16, 18, 19,
    // Right
    20, 22, 21, 20, 23, 22
        };

        // Repeat for front, back, left, right faces, ensuring correct vertex indices and winding orders

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Define UVs for texturing if necessary

        mesh.RecalculateNormals(); // May not be necessary if you've manually set normals, but can be left for safety
    }

    public void ReshapePillar(Vector3 bottomCenter, Vector3 topCenter)
    {
        // Since width, depth, and height are always 1, we'll use 0.5f as half of these values
        float halfSize = 0.5f;
        topCenter.y = 0f;
        bottomCenter.y = 0f;

        // Define vertices for each face separately to ensure correct normals and independent texturing
        Vector3[] vertices = new Vector3[24];

        // Bottom face vertices (y doesn't change, only x and z are based on bottomCenter)
        vertices[0] = bottomCenter + new Vector3(-halfSize, -halfSize, halfSize);  // Front Left
        vertices[1] = bottomCenter + new Vector3(halfSize, -halfSize, halfSize);   // Front Right
        vertices[2] = bottomCenter + new Vector3(halfSize, -halfSize, -halfSize);  // Back Right
        vertices[3] = bottomCenter + new Vector3(-halfSize, -halfSize, -halfSize); // Back Left

        // Top face vertices (y doesn't change, only x and z are based on topCenter)
        vertices[4] = topCenter + new Vector3(-halfSize, halfSize, -halfSize); // Back Left
        vertices[5] = topCenter + new Vector3(halfSize, halfSize, -halfSize);  // Back Right
        vertices[6] = topCenter + new Vector3(halfSize, halfSize, halfSize);   // Front Right
        vertices[7] = topCenter + new Vector3(-halfSize, halfSize, halfSize);  // Front Left

        // Duplicate vertices for side faces to ensure correct normals and texture mapping
        // Front face
        vertices[8] = bottomCenter + new Vector3(-halfSize, -halfSize, halfSize);  // Bottom Left
        vertices[9] = bottomCenter + new Vector3(halfSize, -halfSize, halfSize);   // Bottom Right
        vertices[10] = topCenter + new Vector3(halfSize, halfSize, halfSize);      // Top Right
        vertices[11] = topCenter + new Vector3(-halfSize, halfSize, halfSize);     // Top Left

        // Back face
        vertices[15] = bottomCenter + new Vector3(halfSize, -halfSize, -halfSize); // Bottom Right
        vertices[14] = bottomCenter + new Vector3(-halfSize, -halfSize, -halfSize);// Bottom Left
        vertices[13] = topCenter + new Vector3(-halfSize, halfSize, -halfSize);    // Top Left
        vertices[12] = topCenter + new Vector3(halfSize, halfSize, -halfSize);     // Top Right

        // Left face
        vertices[16] = bottomCenter + new Vector3(-halfSize, -halfSize, -halfSize);// Bottom Back
        vertices[17] = bottomCenter + new Vector3(-halfSize, -halfSize, halfSize); // Bottom Front
        vertices[18] = topCenter + new Vector3(-halfSize, halfSize, halfSize);     // Top Front
        vertices[19] = topCenter + new Vector3(-halfSize, halfSize, -halfSize);    // Top Back

        // Right face
        vertices[23] = bottomCenter + new Vector3(halfSize, -halfSize, halfSize);  // Bottom Front
        vertices[22] = bottomCenter + new Vector3(halfSize, -halfSize, -halfSize); // Bottom Back
        vertices[21] = topCenter + new Vector3(halfSize, halfSize, -halfSize);     // Top Back
        vertices[20] = topCenter + new Vector3(halfSize, halfSize, halfSize);

        // Here you should define the triangles for each face as well, 
        // similar to how it was defined in your final working example.

        mesh.vertices = vertices;
        // Define triangles here, similar to your working example

        mesh.RecalculateNormals(); // This ensures normals are recalculated for proper lighting

        if (!ReferenceEquals(meshCollider, null))
        {
            meshCollider.sharedMesh = mesh;
        }
    }

}
