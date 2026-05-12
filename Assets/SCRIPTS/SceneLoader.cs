using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public enum Scene
    {
        LoadingScene,
        LobbyScene,
        MultiplayerScene
    }

    private static Scene newScene;

    public static void Load(Scene newScene)
    {
        SceneLoader.newScene = newScene;

        SceneManager.LoadSceneAsync(Scene.LoadingScene.ToString());
    }
    public static void LoadNetwork(Scene newScene)
    {
        SceneLoader.newScene = newScene;

        NetworkManager.Singleton.SceneManager.LoadScene(newScene.ToString(), LoadSceneMode.Single);
    }
    public static void SceneLoaderCallback() 
    {
        SceneManager.LoadSceneAsync(newScene.ToString());  
    }
}
