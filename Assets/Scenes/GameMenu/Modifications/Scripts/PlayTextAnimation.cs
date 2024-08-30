using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayTextAnimation : MonoBehaviour
{
    public TextMeshProUGUI tmpText; // Referencia al componente TextMeshPro
    public float colorChangeSpeed = 1f; // Velocidad de cambio de color

    void Start()
    {
        if (tmpText == null)
        {
            tmpText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        // Animar el primer color del gradiente con un efecto de arcoiris
        AnimateRainbowColor();
    }

    void AnimateRainbowColor()
    {
        // Calcula el color arcoiris basado en el tiempo
        float t = Mathf.PingPong(Time.time * colorChangeSpeed, 1f);
        Color rainbowColor = Color.HSVToRGB(t, 1f, 1f);

        // Obtener el gradiente actual
        VertexGradient gradient = tmpText.colorGradient;

        // Asignar el color arcoiris al primer color del gradiente
        gradient.topLeft = rainbowColor;
        gradient.topRight = rainbowColor;

        // Mantener el segundo color en blanco
        gradient.bottomLeft = Color.white;
        gradient.bottomRight = Color.white;

        // Aplicar el gradiente modificado al TMP
        tmpText.colorGradient = gradient;
    }
}
