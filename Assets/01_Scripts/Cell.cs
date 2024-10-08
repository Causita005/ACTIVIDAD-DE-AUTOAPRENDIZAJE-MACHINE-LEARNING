using UnityEngine;

public class Cell : MonoBehaviour
{
    public int experienceIndex;
    public int cellId;
    private GameController gameController;
    private bool destroyedByPlayer = false;

    void Start()
    {
        // Obtener referencia al GameController
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    void OnMouseDown()
    {
        // Marcar que fue destruida por el jugador
        destroyedByPlayer = true;

        // Mostrar información en la consola
        //Debug.Log($"Cell {cellId} eliminated by the player.");

        // Destruir la célula
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Actualizar el estado de supervivencia
        if (gameController != null && experienceIndex >= 0 && experienceIndex < gameController.currentExperiences.Count)
        {
            if (destroyedByPlayer)
            {
                gameController.currentExperiences[experienceIndex].survived = false;
            }
            else
            {
                gameController.currentExperiences[experienceIndex].survived = true;
                //Debug.Log($"Cell {cellId} survived until the end of its life.");
            }
        }
        gameController.UpdateDestroyedCellCounter();
    }
}
