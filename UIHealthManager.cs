using UnityEngine;
using UnityEngine.UI;

public class UIHealthManager : MonoBehaviour
{
    [Header("Configuración de Círculos de Vida")]
    public Image[] healthCircles; 

    [Header("Colores de Estado")]
    public Color fullColor = Color.red; 
    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); 
    public Color lostColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);

    /// <summary>
    /// Actualiza la apariencia de los círculos de vida basados en la salud del jugador.
    /// </summary>
    /// <param name="currentHealth">La vida actual del jugador.</param>
    /// <param name="maxRecoverableHealth">La vida máxima a la que se puede curar.</param>
    public void UpdateHealthUI(int currentHealth, int maxRecoverableHealth)
    {
        for (int i = 0; i < healthCircles.Length; i++)
        {
            if (i < currentHealth)
            {
                healthCircles[i].color = fullColor;
                healthCircles[i].enabled = true;
            }

            else if (i < maxRecoverableHealth)
            {
                healthCircles[i].color = emptyColor;
                healthCircles[i].enabled = true;
            }

            else
            {
                healthCircles[i].color = lostColor;
            }
        }
    }
}
