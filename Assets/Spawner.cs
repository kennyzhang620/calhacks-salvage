using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] Objects;
    public Text OverlayText;

    Dictionary<string, int> categoryIds = new Dictionary<string, int>
    {
        { "REUSE", 0 },
        { "RECYCLE", 1 },
        { "REDUCE", 2 },
        { "COMPOST", 3 },
        { "DISCARD", 4 }
    };


    string[] ignore = { "person", "man", "woman", "boy", "girl", "human body", "human", "face" };

    float _t = 0;

    async void getType(string label, Vector3 objectHit)
    {
        string reply = await OpenAIClient.GetChatCompletion("I'll provide an item. Classify as REUSE, RECYCLE, REDUCE, COMPOST, or DISCARD. Example input: book Example output: REUSE   Follow only the structure specified by the example." + label);
        print(reply);
        if (reply != null && categoryIds.ContainsKey(reply))
        {
            print("ac");
            var index = categoryIds[reply];
            var _spawned = Instantiate(Objects[index], objectHit, Quaternion.identity);
            print("ad");
            if (_spawned)
            {
                _spawned.name = _spawned.transform.position.GetHashCode().ToString();
                GameManager.Objects.Add(_spawned.name, _spawned.gameObject);
                // Do something with the object that was hit by the raycast.

            }

        }
    }

    async void voiceType(string label)
    {
        string reply = await GeminiClient.GetChatCompletion("Write out a roasting message in two lines warning on the potential amount of items to be reused, recycled, reduced or composed, emphasizing based on how frequent of the type exists:" + label  + "Only generate the message, nothing else. No astricks or markdown, just natural speech. ");
        print(reply);
        if (reply != null) GameManager.SaySpeak = reply;
    }

    void SpawnObject(Vector3 objectHit)
    {
        if (!OverlayText) return;

        foreach (string i in ignore)
        {
            if (OverlayText.text.ToLower().Contains(i)) return;
        }

        getType(OverlayText.text.ToLower(), objectHit);

        GameManager.detectedThings.Add(OverlayText.text.ToLower());

        if (GameManager.detectedThings.Count % 3 == 0)
        {
            voiceType(string.Join(", ", GameManager.detectedThings));
        }


        return;
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

            if ((Camera.main.transform.position - objectHit).magnitude > 0.008f && (Camera.main.transform.position - objectHit).magnitude < 1.5f )
            {

                var objectDetect = GameManager.CheckDuplicate(objectHit, 0.4f);
                if (objectDetect)
                {
                  //  objectDetect.transform.position = objectHit;
                    yield return null;
                }
                else
                {
                    print("ab");

                    SpawnObject(objectHit);
                    
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
