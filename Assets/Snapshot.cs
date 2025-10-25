using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(EventTrigger_C))]
public class Snapshot : MonoBehaviour
{
    EventTrigger_C _events;
    public EventTrigger_C _s;
    int imgid = 0;
    GameObject g;

    private void Start()
    {
        _events = GetComponent<EventTrigger_C>();
    }

    private void TrackR()
    {
        g = GameObject.Find("Trackables");
    }
    public void Capture()
    {
        var your_path = Application.persistentDataPath;

        var pn = DateTime.Now.ToString("h:mm:ss tt");
        TrackR();
        StartCoroutine(captureScreenshot());
    }

    IEnumerator captureScreenshot()
    {
        _events.TriggerTGB();
        g.SetActive(false);
        yield return new WaitForEndOfFrame();
        

        string path = Application.persistentDataPath + "/Snapshot"
                + "_" + imgid++ + "_" + Screen.width + "X" + Screen.height + "_" + Settings.SerializeData() + ".jpg";

        Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
        //Get Image from screen
        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenImage.Apply();
        //Convert to png
        byte[] imageBytes = screenImage.EncodeToJPG(50);

        //Save image to file
        System.IO.File.WriteAllBytes(path, imageBytes);
        yield return new WaitForEndOfFrame();
        g.SetActive(!false);
        _s.TriggerTGB();
        yield return new WaitForEndOfFrame();

        path = Application.persistentDataPath + "/Snapshot-Mask"
                + "_" + imgid++ + "_" + Screen.width + "X" + Screen.height + "_" + Settings.SerializeData() + ".jpg";

        screenImage = new Texture2D(Screen.width, Screen.height);
        //Get Image from screen
        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenImage.Apply();
        //Convert to png
        imageBytes = screenImage.EncodeToJPG(50);

        //Save image to file
        System.IO.File.WriteAllBytes(path, imageBytes);
        yield return new WaitForEndOfFrame();
        _s.TriggerUI();
        _events.TriggerUI();
        Settings.Clear();


    }
}
