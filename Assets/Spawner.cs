using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] Objects;
    public Text OverlayText;
    public CaptureUIRect cap;

    float _t = 0;

    int SpawnObject()
    {
        if (!OverlayText) return -1;
        /*
            if (OverlayText.text.ToLower().Contains("flower") || OverlayText.text.ToLower().Contains("plant") || !true)
            {
                return GameManager.ObjectIndex;
            }

            if (OverlayText.text.ToLower().Contains("flower") || OverlayText.text.ToLower().Contains("plant") || !true)
            {
                return Random.Range(0, Objects.Length);
            }
        */
        return 0;
    }
    void OnEnable()
    {

        StartCoroutine(Spawn());
        //     transform.parent.gameObject.SetActive(false);
    }


    IEnumerator Spawn()
    {

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(transform.position);

        if (Physics.Raycast(ray, out hit) && GameManager.ObjectsActive < GameManager.SpawnLimit)
        {
            Vector3 objectHit = hit.point;
            print("a");

            if ((Camera.main.transform.position - objectHit).magnitude > 0.078f && (Camera.main.transform.position - objectHit).magnitude < 1.5f )
            {

                var objectDetect = GameManager.CheckDuplicate(objectHit, 0.8f);
                if (objectDetect)
                {
                    yield return null;
                }
                else
                {
                    print("ab");

                    var index = SpawnObject();

                    if (index != -1)
                    {
                        print("ac");

                        var _spawned = Instantiate(Objects[index], objectHit, Quaternion.identity);
                        StartCoroutine(cap.CaptureAfterRender());
                        print("ad");
                        if (_spawned)
                        {
                            _spawned.name = _spawned.transform.position.GetHashCode().ToString();
                            GameManager.Objects.Add(_spawned.name, _spawned.gameObject);
                            // Do something with the object that was hit by the raycast.

                        }

                    }
                }

            }

            _t = 0.05f;
            yield return null;

        }

    }
    private void FixedUpdate()
    {
        if (_t > 0) _t -= Time.deltaTime;
    }
}
