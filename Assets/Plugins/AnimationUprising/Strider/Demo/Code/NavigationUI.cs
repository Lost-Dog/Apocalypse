using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StriderDemo
{
    public class NavigationUI : MonoBehaviour
    {
        public void LoadMenuScene()
        {
            SceneManager.LoadScene(0);
        }

        public void LoadComparisonScene()
        {
            SceneManager.LoadScene(1);
        }

        public void LoadGameplayScene()
        {
            SceneManager.LoadScene(2);
        }

        public void LoadSizeMattersScene()
        {
            SceneManager.LoadScene(3);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}