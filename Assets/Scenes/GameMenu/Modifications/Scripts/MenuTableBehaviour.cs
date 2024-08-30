using UnityEngine;

public class DragObject2D : MonoBehaviour
{
    public float dragForce = 10f; // Fuerza aplicada al objeto mientras se arrastra
    public float decelerationRate = 0.95f; // Tasa de desaceleración cuando se deja de arrastrar

    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("El objeto debe tener un Rigidbody2D para que el arrastre funcione.");
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 force = (mousePosition - transform.position) * dragForce;
            rb.AddForce(force);
        }
        else
        {
            // Aplicar la desaceleración cuando no se está arrastrando
            rb.velocity *= decelerationRate;
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseUp()
    {
        isDragging = false;
        // Opcional: Puedes ajustar la velocidad inicial o aplicar una pequeña fuerza de fricción aquí si es necesario
    }
}
