using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum TurnState { white, black }
    public static TurnState currentTurn;

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            if (currentTurn == TurnState.white)
            {
                Debug.Log("white");
                yield return StartCoroutine(whiteTurn());
            }
            else if (currentTurn == TurnState.black)
            {
                Debug.Log("black");
                yield return StartCoroutine(blackTurn());
            }
        }
    }

    private IEnumerator whiteTurn()
    {
        // Perform white turn logic here
        Debug.Log("White turn in progress...");
        
        // Example delay to simulate work being done
        yield return new WaitForSeconds(30f);

        // Set the next turn
        currentTurn = TurnState.black;
    }

    private IEnumerator blackTurn()
    {
        // Perform black turn logic here
        Debug.Log("Black turn in progress...");

        // Example delay to simulate work being done
        yield return new WaitForSeconds(30f);

        // Set the next turn
        currentTurn = TurnState.white;
    }

    public static void NextState()
    {
        // Advance to the next state
        currentTurn = (TurnState)(((int)currentTurn + 1) % System.Enum.GetValues(typeof(TurnState)).Length);
    }
}
