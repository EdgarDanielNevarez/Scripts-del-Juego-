using UnityEngine;
using UnityEngine.UI;

public class UISharpnessManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public Image sharpnessBarFill; 

    [Header("Configuración de Color")]
    public Gradient sharpnessGradient;

    /// <summary>
    /// Actualiza la barra de filo (el llenado y el color).
    /// </summary>
    /// <param name="currentValue">El valor actual del filo.</param>
    /// <param name="maxValue">El valor máximo del filo.</param>
    public void UpdateSharpnessUI(float currentValue, float maxValue)
    {
        if (sharpnessBarFill == null) return;

        float fillAmount = currentValue / maxValue;

        sharpnessBarFill.fillAmount = fillAmount;

        sharpnessBarFill.color = sharpnessGradient.Evaluate(fillAmount);
    }
}
