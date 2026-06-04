using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayMusic(Music.MainMenu);
    }
}
