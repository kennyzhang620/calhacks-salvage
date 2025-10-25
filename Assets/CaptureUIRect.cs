using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class CaptureUIRect : MonoBehaviour
{
    [SerializeField] private RectTransform targetRectTransform;
    public string db;

    public IEnumerator CaptureAfterRender()
    {
        yield return new WaitForEndOfFrame();
        string base64 = CaptureToBase64();
        // do something with base64


    }

    public string CaptureToBase64()
    {
        string base64 = "";
        try
        {
            if (targetRectTransform == null)
            {
                Debug.LogError("No target RectTransform assigned!");
                return null;
            }

            Canvas canvas = targetRectTransform.GetComponentInParent<Canvas>();
            Camera cam = canvas != null ? canvas.worldCamera : Camera.main;

            Vector3[] corners = new Vector3[4];
            foreach (Vector2 v in corners)
            {
                if (v.x < 0 || v.y < 0 || v.x > Screen.width || v.y > Screen.height)
                    return null;
            }
            targetRectTransform.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
                corners[i] = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);

            float x = corners[0].x;
            float y = corners[0].y;
            float width = corners[2].x - corners[0].x;
            float height = corners[2].y - corners[0].y;

            // Flip y for ReadPixels (bottom-left origin) -- BAD GPT! NO FLIP!
            Rect rect = new Rect(x, y, width, height);

            Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            tex.ReadPixels(rect, 0, 0);
            tex.Apply();

            byte[] pngBytes = tex.EncodeToPNG();
            //System.IO.File.WriteAllBytes(Application.persistentDataPath + "/Snapshot" + "99.png", pngBytes);
            base64 = Convert.ToBase64String(pngBytes);
            db = base64;
            Destroy(tex);

            Debug.Log($"Captured UI region ({width}x{height}) to Base64 string length: {base64.Length}");
        }
        catch (Exception e)
        {
            db = "";
        }

        return base64;
    }

   
}
