using UnityEngine;

public class TestHpBoost : MonoBehaviour
{
    [SerializeField] float hpIncrease = 10f;

    void Start()
    {
        print("AAAAAAA");
    }

    private void OnTriggerEnter(Collider other)
    {
        GameDataManager.Instance.IncreaseMaxHP(hpIncrease);
        Debug.Log($"[TestHpBoost] MaxHp aumentado en {hpIncrease}. Nuevo valor: {GameDataManager.Instance.PlayerMaxHp}");
        gameObject.SetActive(false);
    }
}
