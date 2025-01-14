using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum TurnState { white, black }
    public static TurnState currentTurn;


    void Start()
    {

    }


    /*private IEnumerator whiteTurn()
    {
        // Perform white turn logic here
        Debug.Log("White turn in progress...");

        // Example delay to simulate work being done (e.g., waiting for player input)
        yield return new WaitForSeconds(2f);  // Simulate some work done

        // Set the next turn
        NextState();  // Calling NextState resets the timer
    }

    private IEnumerator blackTurn()
    {
        // Perform black turn logic here
        Debug.Log("Black turn in progress...");

        // Example delay to simulate work being done (e.g., waiting for player input)
        yield return new WaitForSeconds(2f);  // Simulate some work done

        // Set the next turn
        NextState();  // Calling NextState resets the timer
    }*/

    public static void NextState()
    {
        TimeoutPenalty.timeElapsed = 0f;
        if(currentTurn == TurnState.white) {
            currentTurn = TurnState.black;
        } else {
            currentTurn = TurnState.white;
        }
    }
}
