using UnityEngine;
using System.Collections;

public class ExplosionMenu : MonoBehaviour
{
    public float explosionRadius = 5f; // Radio del área de explosión
    public float explosionForce = 10f; // Fuerza de la explosión
    public float upwardsModifier = 1f; // Modificador de fuerza hacia arriba
    public float explosionInterval = 3f; // Intervalo en segundos entre explosiones

    void Start()
    {
        StartCoroutine(ExplosionRoutine()); // Inicia la coroutine para las explosiones periódicas
    }

    IEnumerator ExplosionRoutine()
    {
        while (true)
        {
            SimulateExplosion(); // Simula una explosión
            yield return new WaitForSeconds(explosionInterval); // Espera el intervalo especificado antes de la próxima explosión
        }
    }

    private void SimulateExplosion()
    {
        // Obtiene todos los colliders dentro del radio de la explosión
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            // Obtiene el Rigidbody2D del objeto
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // Calcula la dirección y fuerza de la explosión
                Vector2 direction = collider.transform.position - transform.position;
                float distance = direction.magnitude;
                direction.Normalize();

                // Calcula la fuerza a aplicar basada en la distancia
                float force = Mathf.Clamp01((explosionRadius - distance) / explosionRadius) * explosionForce;

                // Aplica la fuerza al Rigidbody2D
                rb.AddForce(direction * force + Vector2.up * upwardsModifier, ForceMode2D.Impulse);
            }
        }
    }
}
