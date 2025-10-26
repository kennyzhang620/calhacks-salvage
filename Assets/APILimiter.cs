using UnityEngine;

public class APILimiter : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < GameManager.APILimiter.Length; i++)
        {
            if (GameManager.APILimiter[i] > 0)
                GameManager.APILimiter[i] -= Time.deltaTime;
        }
    }
}
