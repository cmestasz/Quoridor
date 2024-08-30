using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    // Jugar:
    public void Jugar()
    {
        Debug.Log("Jugar() fue llamado.");

        // Obtener el índice de la escena actual
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("Índice de la escena actual: " + currentSceneIndex);

        // Calcular el índice de la siguiente escena
        int nextSceneIndex = currentSceneIndex + 1;
        Debug.Log("Intentando cargar la escena con índice: " + nextSceneIndex);

        // Verificar si el índice de la siguiente escena es válido
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Cargar la siguiente escena
            Debug.Log("Cargando la siguiente escena...");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Si el índice no es válido, mostrar un mensaje de error
            Debug.LogError("El índice de la siguiente escena es inválido. Asegúrate de que la escena esté en los Build Settings.");
        }
    }

    // Salir:
    public void Salir()
    {
        Debug.Log("Salir() fue llamado. Cerrando la aplicación...");

        Application.Quit();
    }
}
