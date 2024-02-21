using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAgent : MonoBehaviour
{
    public Transform objetivo;  // Referencia al objeto Agente que la c�mara seguir�
    public Vector3 offset = new Vector3(0f, 2f, -30f);  // Ajuste de posici�n relativa de la c�mara respecto al objeto

    void Update()
    {
        if (objetivo != null)
        {
            // Actualizar la posici�n de la c�mara para seguir al objeto Agente con el desplazamiento especificado
            transform.position = objetivo.position + offset;
        }
    }
}

