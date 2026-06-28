using UnityEngine;
using FMODUnity;

public class SceneButton : MonoBehaviour
{
    [SerializeField] private SceneTransition transitionType;
    [SerializeField] private bool loadingIcon;
    [SerializeField] private EventReference sfx;

    public void LoadScene(string LevelName)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayOneShot(sfx);

        GameSceneManager.Instance.LoadScene(LevelName, transitionType, loadingIcon);
    }
}
