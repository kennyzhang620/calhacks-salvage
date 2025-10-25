using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntegralDisplay : MonoBehaviour
{
    public Text textObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textObj = GetComponent<Text>();
    }

    float t = 0;
    float v = 0;
    float _ava, _avl, _avw = 0;

    IEnumerator Calc()
    {
        yield return new WaitForSeconds(1);
        Settings.CalculateMesh2();
    }

    public void Freezer(bool f)
    {
        Settings.Freeze = f;
    }
    
    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Calc());
        if (Settings.Area != 0 && Settings.Length != 0 && Settings.Width != 0)
            if (t < 10)
            {
                _ava += Settings.Area;
                _avl += Settings.Length;
                _avw += Settings.Width;
                t++;
            }
        else
            {
                var a = ((_ava / t) + (_avl / t) * (_avw / t)) / 2;
                textObj.text = "CombArea: " + (a).ToString() + " sq.ft\nLength: " + (_avl/t) + " ft\n" + "Width: " + (_avw / t) + " ft\n LxW: " + (_avl / t) * (_avw / t) + "\nScanned Area: " + (_ava/t) +'\n' ;
                _ava = 0; _avl = 0; _avw = 0;
                t = 0;
            }

    }
}
