using UnityEngine;
using UnityEngine.InputSystem;

public class SceneButton : MonoBehaviour
{
    [SerializeField] private SceneTransition transitionType;
    [SerializeField] private bool loadingIcon;

    public void LoadScene(string LevelName)
    {
        GameSceneManager.Instance.LoadScene(LevelName, transitionType, loadingIcon);
    }
}
