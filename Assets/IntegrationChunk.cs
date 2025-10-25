using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IntegrationChunk : MonoBehaviour
{
    public MeshRenderer mesh;
    public float RectSize = 1.0f; // Size in m
    public float L, W = 0;
    float dist = 4;
    float _t = 0.05f;
    float its = 2;
    bool _inA = false;

    MeshFilter _m;

    void Start()
    {
        if (Settings.Freeze)
        {
            Destroy(gameObject);
            return;
        }

        mesh = GetComponent<MeshRenderer>();
        _m = GetComponent<MeshFilter>();
        if (Settings.MeshCount - 1 <= 0) Destroy(gameObject);

        if (Settings.cmesh == null)
        {
            Settings.cmesh = new Mesh();
            Settings.cmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Large mesh support
        }
    }

    private void Update()
    {
        if (mesh)
            mesh.enabled = (transform.position - Camera.main.transform.position).magnitude < dist;

        _t += Time.deltaTime;
        if ((_m.sharedMesh.triangles.Count() > 1000 || _t > 5) && !_inA)
        {
            StartCoroutine(MergeAsync());
            _inA = true;
        }
    }
    IEnumerator MergeAsync()
    {
        yield return new WaitForSeconds(0.5f);

        MergeIntoCentralMesh();
    }

    void MergeIntoCentralMesh()
    {
        MeshFilter mf = _m;
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("No mesh to merge on " + gameObject.name);
            return;
        }

        // Convert the current fragment to a CombineInstance
        CombineInstance ci = new CombineInstance
        {
            mesh = mf.sharedMesh,
            transform = transform.localToWorldMatrix
        };

        // Copy existing mesh from Settings.cmesh
        Mesh existing = Settings.cmesh;
        List<CombineInstance> combineList = new List<CombineInstance>();

        if (existing.vertexCount > 0)
        {
            CombineInstance existingCI = new CombineInstance
            {
                mesh = existing,
                transform = Matrix4x4.identity
            };
            combineList.Add(existingCI);
        }

        combineList.Add(ci);

        // Merge and assign back to Settings.cmesh
        Mesh combined = new Mesh();
        combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combined.CombineMeshes(combineList.ToArray(), true, true);
        Settings.cmesh = combined;

        Debug.Log($"Merged {gameObject.name} into Settings.cmesh — now has {combined.vertexCount} vertices.");
        Settings.MeshCount--;

   
    }

    private void OnDestroy()
    {
        
        Settings.MeshCount++;
    }
}

