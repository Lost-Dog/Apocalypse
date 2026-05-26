using UnityEngine;
using UnityEngine.SceneManagement;

public class bl_MMSceneLoad : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene("MiniMaps (Additive)", LoadSceneMode.Additive);
    }
}
