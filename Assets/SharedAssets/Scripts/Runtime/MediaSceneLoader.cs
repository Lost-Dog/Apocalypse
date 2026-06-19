using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This class relegates signals dispatched from the camera flythrough timeline
/// </summary>
public class MediaSceneLoader : MonoBehaviour
{
    [SerializeField] private SceneLoader m_GardenSceneLoader;
    [SerializeField] private SceneLoader m_CockpitSceneLoader;
    [FormerlySerializedAs("m_OasisSceneLoader")]
    [SerializeField] private SceneLoader m_EnvironmentSceneLoader;

    private SceneLoader TerminalSceneLoader;

    void Start()
    {
        TerminalSceneLoader = GetComponent<SceneLoader>();
    }
    public void EnableGarden()
    {
        m_GardenSceneLoader.EnableScene();
    }

    public void DisableGarden()
    {
        m_GardenSceneLoader.DisableScene();
    }

    public void EnableCockpit()
    {
        m_CockpitSceneLoader.EnableScene();
    }

    public void DisableCockpit()
    {
        m_CockpitSceneLoader.DisableScene();
    }

    public void EnableTerminal()
    {
        TerminalSceneLoader.EnableScene();
    }

    public void EnableEnvironment()
    {
        m_EnvironmentSceneLoader.EnableScene();
    }

    public void DisableEnvironment()
    {
        m_EnvironmentSceneLoader.DisableScene();
    }

    public void Transition()
    {
        SceneTransitionManager.StartTransition(this);
    }

    public SceneLoader GetTerminalSceneLoader()
    {
        return TerminalSceneLoader;
    }
}