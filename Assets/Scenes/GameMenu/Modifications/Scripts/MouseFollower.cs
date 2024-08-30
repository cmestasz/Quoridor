using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    void Update()
    {
        // Obtén la posición del ratón en el mundo
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Asegúrate de que la posición z sea 0 para un entorno 2D
        mousePosition.z = 0;

        // Actualiza la posición del objeto para que siga al ratón
        transform.position = mousePosition;
    }
}
