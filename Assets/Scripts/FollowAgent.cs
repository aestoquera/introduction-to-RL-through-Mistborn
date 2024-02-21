using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAgent : MonoBehaviour
{
    public Transform objetivo;  // Referencia al objeto Agente que la cámara seguirá
    public Vector3 offset = new Vector3(0f, 2f, -30f);  // Ajuste de posición relativa de la cámara respecto al objeto

    void Update()
    {
        if (objetivo != null)
        {
            // Actualizar la posición de la cámara para seguir al objeto Agente con el desplazamiento especificado
            transform.position = objetivo.position + offset;
        }
    }
}

