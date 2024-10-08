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
        //destroyedCellsText.text = "C�lulas Destruidas: ";
    }

    public void UpdateDestroyedCellCounter()
    {
        // Incrementar el contador
        destroyedCellsCounter++;

        // Actualizar el texto con la cantidad de c�lulas destruidas
        destroyedCellsText.text = "Destroyed Cells: " + destroyedCellsCounter;
        Debug.Log(destroyedCellsText.text);
    }

    IEnumerator ManageGenerations()
    {
        while (true)
        {
            // Esperar hasta el pr�ximo intervalo
            yield return new WaitForSeconds(timeInterval);

            // Destruir c�lulas existentes
            foreach (GameObject cell in existingCells)
            {
                Destroy(cell);
            }
            existingCells.Clear();

            // Esperar un frame para asegurar que OnDestroy() se llama en las c�lulas
            yield return new WaitForEndOfFrame();

            // Preparar experiencias para la siguiente generaci�n
            previousExperiences.Clear();
            previousExperiences.AddRange(currentExperiences);
            currentExperiences.Clear();

            // Filtrar experiencias para obtener solo las c�lulas que sobrevivieron
            survivingExperiences.Clear();
            foreach (CellExperience experience in previousExperiences)
            {
                if (experience.survived)
                {
                    survivingExperiences.Add(experience);
                }
            }

            // Generar nuevas c�lulas
            GenerateCells();
        }
    }

    void GenerateCells()
    {
        roundCounter++;
        roundsText.text = "Round: " + roundCounter;
        // Incrementar el contador de generaciones
        generationCounter++;

        // Verificar si hay c�lulas supervivientes
        if (survivingExperiences.Count == 0)
        {
            // Generar c�lulas aleatorias
            for (int i = 0; i < numberOfCells; i++)
            {
                // Generar atributos aleatorios
                float randomHue = Random.Range(0f, 1f);
                float randomSaturation = Random.Range(0.5f, 1f);
                float randomValue = Random.Range(0.5f, 1f);
                Color adjustedColor = Color.HSVToRGB(randomHue, randomSaturation, randomValue);

                float adjustedSize = Random.Range(0.5f, 1.5f);

                // Crear y configurar la nueva c�lula
                CreateNewCell(adjustedColor, adjustedSize, -1);
            }
        }
        else
        {
            // Generar nuevas c�lulas basadas en las c�lulas supervivientes
            int numSurvivors = survivingExperiences.Count;
            int survivorIndex = 0;

            for (int i = 0; i < numberOfCells; i++)
            {
                // Obtener la experiencia del superviviente actual
                CellExperience parentExperience = survivingExperiences[survivorIndex];

                // Crear y configurar la nueva c�lula
                CreateNewCell(parentExperience.color, parentExperience.size, previousExperiences.IndexOf(parentExperience));

                // Incrementar el �ndice de superviviente c�clicamente
                survivorIndex = (survivorIndex + 1) % numSurvivors;
            }
        }
    }

    // M�todo para crear y configurar una nueva c�lula
    void CreateNewCell(Color adjustedColor, float adjustedSize, int parentId)
    {
        // Instanciar la c�lula
        GameObject newCell = Instantiate(cellPrefab);

        // Asignar posici�n aleatoria
        float x = Random.Range(-8f, 8f);
        float y = Random.Range(-4f, 4f);
        newCell.transform.position = new Vector2(x, y);

        // Asignar tama�o y color
        newCell.transform.localScale = new Vector3(adjustedSize, adjustedSize, 1);
        newCell.GetComponent<SpriteRenderer>().color = adjustedColor;

        // A�adir a la lista de c�lulas existentes
        existingCells.Add(newCell);

        // Registrar la experiencia inicial
        CellExperience newExperience = new CellExperience();
        newExperience.color = adjustedColor;
        newExperience.size = adjustedSize;
        newExperience.survived = false; // Por defecto, asumimos que no sobrevivir�
        newExperience.generation = generationCounter;
        newExperience.parentId = parentId;
        currentExperiences.Add(newExperience);

        // Asignar el �ndice de experiencia a la c�lula
        Cell cellScript = newCell.GetComponent<Cell>();
        cellScript.experienceIndex = currentExperiences.Count - 1;

        // Asignar un identificador �nico a la c�lula
        cellScript.cellId = cellCounter;
        cellCounter++;
    }
}
