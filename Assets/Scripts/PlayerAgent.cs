using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    [SerializeField] private PlayerController _playerController;

    public float timeReset = 60.0f;
    public float timeRemaining = 60.0f; // Tiempo total en segundos , cambiarlo en el UPDATE
    private bool timerIsRunning = true;

    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject goal;
    [SerializeField] private Transform spawn;

    public float starting_dist;

    public override void Initialize()
    {
        //ResetChar();
        starting_dist = Vector3.Distance(goal.transform.position, obj.transform.position);
        timeRemaining = timeReset;
        timerIsRunning = true;
    }
    private void Update()
    {
        // Reward by distance
        float dist_covered_towards_goal = starting_dist - Vector3.Distance(goal.transform.position, obj.transform.position);
        GiveReward(dist_covered_towards_goal*5);

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                // Resta el tiempo del temporizador en cada frame
                GiveReward(-(5 * (timeReset - timeRemaining)));
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                EndEpisode();
            }
        }

    }

    public void ResetChar()
    {


        obj.transform.position = spawn.position;

        //RESETEA PROPIEDADES DEL Agente
        _playerController.velocity = new Vector3(0,0,0);
        _playerController.Start(); //Resetear el Agente


    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Begin Episode...");
        timeRemaining = timeReset;
        ResetChar();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // MoveAgent(actions.DiscreteActions);
        MoveAgent(actions.DiscreteActions);
    }

    public void MoveAgent(ActionSegment<int> vectorAction)
    {
        Debug.Log(vectorAction);
        //Girar la Cabeza: Numero 0

        int direction = (int)vectorAction[0];
        if (direction == 0)
        {
            // No rotar cabeza
        }
        else if (direction == 1)
        {
            _playerController.Look(1); // Rotar DERECHA
        }
        else if (direction == 2)
        {
            _playerController.Look(-1); // Rotar IZQUIERDA
        }

        //Moverse izq y drch con o sin Sprint: Numero 1

        int moveSide = (int)vectorAction[1];
        if (moveSide == 0)
        {
            // No Moverse
        }
        else if (moveSide == 1)
        {
            Debug.Log("Andando");
            _playerController.Move(1, 0); // Andar Derecha
        }
        else if (moveSide == 2)
        {
            Debug.Log("Andando");
            _playerController.Move(-1, 0); // Andar Izquierda
        }
        else if (moveSide == 3)
        {
            Debug.Log("Corriendo");
            _playerController.Move(1, 1); // Correr Derecha
        }
        else if (moveSide == 4)
        {
            Debug.Log("Corriendo");
            _playerController.Move(-1, 1); // Correr Izquierda
        }

        //Saltar: Numero 2
        int jump = (int)vectorAction[2];
        if (jump == 0)
        {
            // No Saltar
            //Debug.Log("No Salto");
        }
        else if (jump == 1)
        {
            GiveReward(-1000f);
            _playerController.JumpAndGravity(true); // Saltar
        }

        //Tirar y Empujar Metal: Numero 3
        int pull_push = (int)vectorAction[3];
        if (pull_push == 0)
        {
            //Debug.Log("NOT Metal PULL or PUSH");
            _playerController.CastRay(0); // No Tirar ni empujar del Metal
        }
        else if (pull_push == 1)
        {
            //Debug.Log(" ---> PULL");
            //GiveReward(-1000f);
            _playerController.CastRay(1); // Tirar del Metal
        }
        else if (pull_push == 2)
        {
            //Debug.Log(" <--- PUSH");
            //GiveReward(-1000f);
            _playerController.CastRay(-1); // Empujar del Metal
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_playerController.velocity);
        sensor.AddObservation(_playerController.isGrounded);
        sensor.AddObservation(_playerController.movingTowardsMetal);
        sensor.AddObservation(_playerController.movingTowardsMetalInverse);

        sensor.AddObservation(Vector3.Distance(goal.transform.position, obj.transform.position));
        sensor.AddObservation(obj.transform.position);
        sensor.AddObservation(_playerController.head.position);
        sensor.AddObservation(_playerController.head.rotation);
    }
    public void ReachedGoal()
    {
        GiveReward(1000000f);

        // By marking an agent as done AgentReset() will be called automatically.
        //_checkpointManager.ResetCheckpoints();
        EndEpisode();
        Debug.Log("GOAL REACHED...");
    }

    public void GiveReward(float rewardGiven)
    {
        // We use a reward of 5.
        //Debug.Log("Giving Reward of: " + rewardGiven);
        AddReward(rewardGiven);

    }

    public void Stop()
    {
        //RESETEA PROPIEDADES DEL COCHE
        Debug.Log("STOOOOOOOP...");
        _playerController.velocity = new Vector3(0, 0, 0);
        timeRemaining = timeReset;
        _playerController.Start(); //NOS AYUDA A RESETAR EL COCHE
    }
}
