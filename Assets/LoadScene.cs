using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{
    public void LoadLvl(int L)
    {
        SceneManager.LoadScene(L);
    }
}
