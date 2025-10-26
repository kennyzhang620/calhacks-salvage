using UnityEngine;
using UnityEngine.UI;

public class Interactables : MonoBehaviour
{
    GameObject menu;
    public Text text1;
    public string SetText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void UpdateInfo(Text text)
    {
        text.transform.parent.gameObject.SetActive(true);
        text.text = SetText;
    }

    RaycastHit ray;

    // Update is called once per frame
    void Update()
    {

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out ray))
        {
            if (ray.transform.gameObject.tag == "Interact")
            {
                ray.transform.SendMessage("UpdateInfo", text1);
            }
        }
    }
}
