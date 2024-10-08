using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameObject cellPrefab;
    public int numberOfCells = 15;
    public float timeInterval = 10f;

    private float lastGenerationTime;
    private bool isGenerating = false;

    private int generationCounter = 0;
    private int cellCounter = 0;
    private int roundCounter = 0;
    public Text destroyedCellsText;
    public Text roundsText;
    private int destroyedCellsCounter = 0;

    private List<GameObject> existingCells = new List<GameObject>();

    [System.Serializable]
    public class CellExperience
    {
        public Color color;
        public float size;
        public bool survived;
        public int generation;
        public int parentId;
    }

    private List<CellExperience> previousExperiences = new List<CellExperience>();
    public List<CellExperience> currentExperiences = new List<CellExperience>();
    private List<CellExperience> survivingExperiences = new List<CellExperience>();

    void Start()
    {
        lastGenerationTime = Time.time;
        GenerateCells();
        StartCoroutine(ManageGenerations());
        //destroyedCellsText.text = "Células Destruidas: ";
    }

    public void UpdateDestroyedCellCounter()
    {
        // Incrementar el contador
        destroyedCellsCounter++;

        // Actualizar el texto con la cantidad de células destruidas
        destroyedCellsText.text = "Destroyed Cells: " + destroyedCellsCounter;
        Debug.Log(destroyedCellsText.text);
    }

    IEnumerator ManageGenerations()
    {
        while (true)
        {
            // Esperar hasta el próximo intervalo
            yield return new WaitForSeconds(timeInterval);

            // Destruir células existentes
            foreach (GameObject cell in existingCells)
            {
                Destroy(cell);
            }
            existingCells.Clear();

            // Esperar un frame para asegurar que OnDestroy() se llama en las células
            yield return new WaitForEndOfFrame();

            // Preparar experiencias para la siguiente generación
            previousExperiences.Clear();
            previousExperiences.AddRange(currentExperiences);
            currentExperiences.Clear();

            // Filtrar experiencias para obtener solo las células que sobrevivieron
            survivingExperiences.Clear();
            foreach (CellExperience experience in previousExperiences)
            {
                if (experience.survived)
                {
                    survivingExperiences.Add(experience);
                }
            }

            // Generar nuevas células
            GenerateCells();
        }
    }

    void GenerateCells()
    {
        roundCounter++;
        roundsText.text = "Round: " + roundCounter;
        // Incrementar el contador de generaciones
        generationCounter++;

        // Verificar si hay células supervivientes
        if (survivingExperiences.Count == 0)
        {
            // Generar células aleatorias
            for (int i = 0; i < numberOfCells; i++)
            {
                // Generar atributos aleatorios
                float randomHue = Random.Range(0f, 1f);
                float randomSaturation = Random.Range(0.5f, 1f);
                float randomValue = Random.Range(0.5f, 1f);
                Color adjustedColor = Color.HSVToRGB(randomHue, randomSaturation, randomValue);

                float adjustedSize = Random.Range(0.5f, 1.5f);

                // Crear y configurar la nueva célula
                CreateNewCell(adjustedColor, adjustedSize, -1);
            }
        }
        else
        {
            // Generar nuevas células basadas en las células supervivientes
            int numSurvivors = survivingExperiences.Count;
            int survivorIndex = 0;

            for (int i = 0; i < numberOfCells; i++)
            {
                // Obtener la experiencia del superviviente actual
                CellExperience parentExperience = survivingExperiences[survivorIndex];

                // Crear y configurar la nueva célula
                CreateNewCell(parentExperience.color, parentExperience.size, previousExperiences.IndexOf(parentExperience));

                // Incrementar el índice de superviviente cíclicamente
                survivorIndex = (survivorIndex + 1) % numSurvivors;
            }
        }
    }

    // Método para crear y configurar una nueva célula
    void CreateNewCell(Color adjustedColor, float adjustedSize, int parentId)
    {
        // Instanciar la célula
        GameObject newCell = Instantiate(cellPrefab);

        // Asignar posición aleatoria
        float x = Random.Range(-8f, 8f);
        float y = Random.Range(-4f, 4f);
        newCell.transform.position = new Vector2(x, y);

        // Asignar tamaño y color
        newCell.transform.localScale = new Vector3(adjustedSize, adjustedSize, 1);
        newCell.GetComponent<SpriteRenderer>().color = adjustedColor;

        // Añadir a la lista de células existentes
        existingCells.Add(newCell);

        // Registrar la experiencia inicial
        CellExperience newExperience = new CellExperience();
        newExperience.color = adjustedColor;
        newExperience.size = adjustedSize;
        newExperience.survived = false; // Por defecto, asumimos que no sobrevivirá
        newExperience.generation = generationCounter;
        newExperience.parentId = parentId;
        currentExperiences.Add(newExperience);

        // Asignar el índice de experiencia a la célula
        Cell cellScript = newCell.GetComponent<Cell>();
        cellScript.experienceIndex = currentExperiences.Count - 1;

        // Asignar un identificador único a la célula
        cellScript.cellId = cellCounter;
        cellCounter++;
    }
}
