using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum TurnState { white, black }
    public static TurnState currentTurn;


    void Start()
    {

    }


    public static void NextState()
    {
        TimeoutPenalty.timeElapsed = 0f;
        if(currentTurn == TurnState.white) {
            currentTurn = TurnState.black;
        } else {
            currentTurn = TurnState.white;
        }
        //Debug.Log(currentTurn);
    }
}
