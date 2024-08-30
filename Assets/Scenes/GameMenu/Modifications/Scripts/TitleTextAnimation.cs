using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RotateTextAnimation : MonoBehaviour
{
    public float rotationAngle = 15f; // Ángulo máximo de rotación
    public float rotationSpeed = 2f; // Velocidad de la rotación

    private TextMeshProUGUI tmpText;
    private float currentAngle;
    private bool rotatingRight = true;
    private float initialRotationZ;

    void Start()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        initialRotationZ = tmpText.rectTransform.eulerAngles.z; // Guarda la rotación Z inicial
    }

    void Update()
    {
        // Animar la rotación
        if (rotatingRight)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (currentAngle >= rotationAngle)
            {
                currentAngle = rotationAngle;
                rotatingRight = false;
            }
        }
        else
        {
            currentAngle -= rotationSpeed * Time.deltaTime;
            if (currentAngle <= -rotationAngle)
            {
                currentAngle = -rotationAngle;
                rotatingRight = true;
            }
        }

        // Aplicar la rotación al TMP considerando la rotación inicial
        tmpText.rectTransform.rotation = Quaternion.Euler(0, 0, initialRotationZ + currentAngle);
    }
}
