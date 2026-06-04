using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [SerializeField] private SceneTransition transitionType;
    [SerializeField] private bool loadingIcon;
    [SerializeField] private SFX sfx;

    public void LoadScene(string LevelName)
    {
        SoundManager.Instance.PlaySFX(sfx);
        GameSceneManager.Instance.LoadScene(LevelName, transitionType, loadingIcon);
    }
}
