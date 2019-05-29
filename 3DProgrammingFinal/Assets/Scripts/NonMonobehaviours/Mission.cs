using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MissionEvent : UnityEvent<Mission> {}

[System.Serializable]
public class Mission 
{
    public UnityEvent Accomplished      { get; protected set; }
    public UnityEvent Failed            { get; protected set; }
    public UnityEvent DeadlineMissed    { get; protected set; }
    PlayerShip _handler =           null;
    public PlayerShip handler
    {
        get { return _handler; }
        set { _handler = value; }
    }
    int _successVal =                1;
    public int successVal
    {
        get { return _successVal; }
        set { _successVal = value; }
    }
    int _penaltyVal =                2;
    public int penaltyVal
    {
        get { return _penaltyVal; }
        set { _penaltyVal = value; }
    }
    int _deadlinePenaltyVal =        3;
    public int deadlinePenaltyVal
    {
        get { return _deadlinePenaltyVal; }
        set { _deadlinePenaltyVal = value; }
    }
    public float baseTimeLimit          { get; protected set; }
    public float timeRemaining          { get; protected set; } // in seconds until it is failed
    public Planet destination =     null;

    public Mission(float timeLimit, Planet destination)
    {
        Accomplished =              new UnityEvent();
        Failed =                    new UnityEvent();
        DeadlineMissed =            new UnityEvent();
        this.baseTimeLimit =        timeLimit;
        this.timeRemaining =        timeLimit;
        this.destination =          destination;
    }

    public void Update()
    {
        CountDownToDeadline();
    }

    void CountDownToDeadline()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -=        Time.deltaTime;
            //Debug.Log("Mission ticking down.");
            
            if (timeRemaining <= 0)
            {
                handler.statusEvents.MissedDeadline.Invoke();
                DeadlineMissed.Invoke();
            }
        }
    }
	
    
}
