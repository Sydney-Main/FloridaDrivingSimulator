using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StyleUIManager : MonoBehaviour
{
    public Slider styleBar;
    public TextMeshProUGUI styleRankText;

    private CarController carController;

    void Start()
    {
        carController = FindObjectOfType<CarController>();
    }

    void Update()
    {
        if (carController == null) return;

        styleBar.value = carController.GetCurrentStylePoints();
        styleRankText.text = carController.GetStyleRank();
    }
}
