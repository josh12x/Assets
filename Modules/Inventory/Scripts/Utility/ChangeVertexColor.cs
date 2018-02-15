using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeVertexColor : MonoBehaviour
{
    public Color colToChangeTo;
    public Mesh meshTochange;

    // Use this for initialization
    void Start()
    {
        if (GetComponent<SkinnedMeshRenderer>() != null)
        {
            Mesh mesh = Instantiate(GetComponent<SkinnedMeshRenderer>().sharedMesh);
            Color[] colors = mesh.colors;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = colToChangeTo;
            }

            mesh.colors = colors;
            GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
        }

        else if (GetComponent<MeshFilter>() != null)
        {
            Mesh mesh = Instantiate(GetComponent<MeshFilter>().sharedMesh);
            Color[] colors = mesh.colors;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = colToChangeTo;
            }

            mesh.colors = colors;
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }


    // Update is called once per frame
    void Update ()
    {
		
	}
}
