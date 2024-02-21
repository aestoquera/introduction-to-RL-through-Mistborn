using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 7;
    [SerializeField] float moveSpeedTowardsMetal = 20;
    [SerializeField] float sprintSpeed = 10;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] Transform groundCheck; // MUST SET BY HAND
    public bool isGrounded;
    [SerializeField] LayerMask groundLayer; // MUST SET BY HAND
    [SerializeField] float g = -10;
    public Vector3 velocity;

    private PlayerInputManager input;
    private CharacterController controller;

    [SerializeField] public Transform head; // MUST SET BY HAND
    public float rotationSpeed = 270f;
    [SerializeField] GameObject headPointer; // MUST SET BY HAND
    [SerializeField] Material whiteMaterial; // MUST SET BY HAND
    [SerializeField] Material blueMaterial; // MUST SET BY HAND
    [SerializeField] Material greenMaterial; // MUST SET BY HAND
    [SerializeField] Material purpleMaterial; // MUST SET BY HAND

    [SerializeField] float awarenessRadius = 25f;
    [SerializeField] LayerMask metalLayer; // MUST SET BY HAND
    public bool movingTowardsMetal = false;
    public bool movingTowardsMetalInverse = false;
    private Vector3 metalTargetPosition;

    [SerializeField] float raycastDistance = 500f;

    // Start is called before the first frame update
    public void Start()
    {
        moveSpeed = 7;
        moveSpeedTowardsMetal = 20;
        sprintSpeed = 10;
        jumpHeight = 5f;
        isGrounded = false;
        g = -10;
        input = GetComponent<PlayerInputManager>();
        controller = GetComponent<CharacterController>();
        rotationSpeed = 270f;
        awarenessRadius = 25f;
        movingTowardsMetal = false;
        movingTowardsMetalInverse = false;
        raycastDistance = 500f;

        // Obtener la referencia al objeto Head al comienzo del juego
        if (transform.Find("Head") != null)
        {
            head = transform.Find("Head");
        }
        else
        {
            Debug.LogError("No se encontró el objeto 'Head' como hijo de este GameObject.");
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (movingTowardsMetal)
        {
            MoveTowardsMetal();
        }
        else if (movingTowardsMetalInverse)
        {
            MoveTowardsMetalInverse();
        }
        else
        {
            Look();
            Move();
            JumpAndGravity();
            CheckMetalObjects();

            // Cast a ray only if not currently moving towards a metal object
            CastRay();
        }
    }

    public void Look(int agentAct = 0)
    {
        //Debug.Log("Rotate: " + input.rotHead.x);
        if(agentAct == 0) // HUMAN INPUT
        {
            //Debug.Log("Human Input Look");
            // Calculamos la rotación en función de la dirección y la velocidad de rotación
            float rotationAmount = -input.rotHead.x * rotationSpeed * Time.deltaTime;

            // Rotamos el objeto en el eje Z (puedes ajustar según tus necesidades)
            head.Rotate(Vector3.forward, rotationAmount);
        }
        else // AGENT INPUT
        {
            //Debug.Log("Agent Look: " + agentAct);
            // Calculamos la rotación en función de la dirección y la velocidad de rotación
            float rotationAmount = -agentAct * rotationSpeed * Time.deltaTime;

            // Rotamos el objeto en el eje Z (puedes ajustar según tus necesidades)
            head.Rotate(Vector3.forward, rotationAmount);
        }

        
    }

    public void Move(int agentAct = 0, int agentSprint = -1)
    {
        float speed = 0;
        Vector3 inputDir = new Vector3(0, 0, 0);
        //Debug.Log("Move: " + input.move.x);
        if (agentAct == 0)
        {
            //Debug.Log("Human Input Move");
            inputDir = new Vector3(input.move.x, 0, 0);  // Asegúrate de que la componente z sea 0
            if (input.move != Vector2.zero)
            {
                speed = input.sprint ? sprintSpeed : moveSpeed;
            }
        }
        else
        {
            //Debug.Log("Agent Move: " + agentAct + "/" + agentSprint);
            inputDir = new Vector3(agentAct, 0, 0);
            if (agentSprint != -1)
            {
                speed = agentSprint == 1 ? sprintSpeed : moveSpeed;
            }
        }


        // Invertir la dirección del movimiento en el eje Z
        controller.Move(inputDir * speed * Time.deltaTime);
    }

    public void JumpAndGravity(bool agentAct = false)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, .1f, groundLayer);
        //Debug.Log(isGrounded);
        //Debug.Log("Agent Jump: " + agentAct);

        if (isGrounded)
        {
            if (input.jump || agentAct)
            {
                //Debug.Log("Jumping");
                velocity.y = Mathf.Sqrt(jumpHeight * -g * 2);
                input.jump = false;
                agentAct = false;
            }
        }
        else
        {
            velocity.y += g * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    public List<Transform> GetMetalObjects()
    {
        List<Transform> metalObjects = new List<Transform>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, awarenessRadius, metalLayer);

        foreach (var collider in colliders)
        {
            metalObjects.Add(collider.transform);
        }

        return metalObjects;
    }

    public void CheckMetalObjects()
    {
        List<Transform> metalObjects = GetMetalObjects();

        if (metalObjects.Count > 0)
        {
            //Debug.Log("Metal objects nearby:");

            foreach (var metalObject in metalObjects)
            {
                Debug.Log(metalObject.name);
            }
        }
    }

    public void CastRay(int agentAct = 0)
    {
        Ray ray = new Ray(head.position, head.up);
        RaycastHit hit;
        //Debug.Log("Agent CastRay: " + agentAct);

        int metalLayerMask = 1 << LayerMask.NameToLayer("Metal"); // Assuming "Metal" is the name of your metal layer

        if (Physics.Raycast(ray, out hit, raycastDistance, metalLayerMask))
        {
            // Change the material to blue
            Renderer rend = headPointer.GetComponent<Renderer>();
            rend.material = blueMaterial;

            //Debug.Log("Ray hit a metal object!");
            // Check if the left mouse button is pressed
            if (Input.GetMouseButtonDown(0) || agentAct == 1)
            {
                // Start moving towards the metal object
                rend.material = greenMaterial;
                StartMovingTowardsMetal(hit.point);
            }
            else if (Input.GetMouseButtonDown(1) || agentAct == -1)
            {
                // Start moving in the inverse direction
                rend.material = purpleMaterial;
                StartMovingTowardsMetalInverse(hit.point);
            }
        }
        else
        {
            // Change the material to white
            Renderer rend = headPointer.GetComponent<Renderer>();
            rend.material = whiteMaterial;
        }
    }

    public void StartMovingTowardsMetal(Vector3 targetPosition)
    {
        movingTowardsMetal = true;
        metalTargetPosition = targetPosition;
    }

    public void MoveTowardsMetal()
    {
        // Calculate the direction towards the metal target position
        Vector3 direction = (metalTargetPosition - transform.position).normalized;

        // Move the player towards the metal target position
        controller.Move(direction * moveSpeedTowardsMetal * Time.deltaTime);

        // Check if the left mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            // Stop moving towards the metal object
            movingTowardsMetal = false;
        }
    }

    public void StartMovingTowardsMetalInverse(Vector3 targetPosition)
    {
        movingTowardsMetalInverse = true;
        metalTargetPosition = targetPosition;
    }

    public void MoveTowardsMetalInverse()
    {
        // Calculate the direction away from the metal target position
        Vector3 direction = (transform.position - metalTargetPosition).normalized;

        // Move the player away from the metal target position
        controller.Move(direction * moveSpeedTowardsMetal * Time.deltaTime);

        // Check if the right mouse button is released or if the metal object is out of awareness radius
        if (!Input.GetMouseButton(1) || Vector3.Distance(transform.position, metalTargetPosition) > awarenessRadius)
        {
            // Stop moving away from the metal object
            movingTowardsMetalInverse = false;
        }
    }
}