using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class PressTheButtonMinigame : MonoBehaviour, IMiniGame
{
    public Button[] buttons; // Array of UI Buttons in the mini-game
    public GameObject miniGameCanvas; // Reference to the mini-game Canvas or parent GameObject to deactivate
    public Color flashColor = Color.yellow; // Color to flash the buttons
    public float flashDuration = 0.5f; // Duration each button flashes
    public float delayBetweenFlashes = 0.5f; // Delay between each button flash
    public float initialDelay = 0.5f; // Initial delay before starting the flashing sequence
    public int maxSequenceLength = 5; // Length of how long the sequence generated will be

    private Color[] originalColors; // Store the original colors of the buttons
    private int[] correctSequence; // Randomly generated sequence of button indices
    private int currentStep = 0; // Track the player's progress in the sequence
    private bool playerCanInteract = false; // Allow player interaction only after flashing sequence
    private bool isGameCompleted = false; // Track if the game has already been completed

    void Start()
    {
        originalColors = new Color[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            originalColors[i] = buttons[i].GetComponent<Image>().color;
        }

        GenerateRandomSequence();

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonPressed(index));
        }

        StartCoroutine(FlashSequence());
    }

    void GenerateRandomSequence()
    {
        int sequenceLength = Mathf.Min(buttons.Length, maxSequenceLength); // Change how long the sequence is
        correctSequence = Enumerable.Range(0, buttons.Length)
                                    .OrderBy(x => Random.value)
                                    .Take(sequenceLength)
                                    .ToArray();
    }

    IEnumerator FlashSequence()
    {
        if (isGameCompleted) yield break;

        playerCanInteract = false;
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < correctSequence.Length; i++)
        {
            int buttonIndex = correctSequence[i];
            buttons[buttonIndex].GetComponent<Image>().color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            buttons[buttonIndex].GetComponent<Image>().color = originalColors[buttonIndex];
            yield return new WaitForSeconds(delayBetweenFlashes);
        }

        playerCanInteract = true;
        currentStep = 0;
    }

    public void OnButtonPressed(int buttonIndex)
    {
        if (!playerCanInteract || isGameCompleted) return;

        if (buttonIndex == correctSequence[currentStep])
        {
            currentStep++;

            if (currentStep >= correctSequence.Length)
            {
                Debug.Log("Correct sequence completed!");
                isGameCompleted = true;
                CompleteMiniGame();
                CloseMiniGame();
                miniGameCanvas.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Wrong sequence!");
            playerCanInteract = false;
            currentStep = 0;
            StartCoroutine(FlashSequence());
        }
    }

    private void CloseMiniGame()
    {
        playerCanInteract = false;
        miniGameCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void TryOpenMiniGame()
    {
        if (isGameCompleted)
        {
            Debug.Log("Mini-game has already been completed and cannot be reopened.");
            return;
        }

        miniGameCanvas.SetActive(true);
        StartCoroutine(FlashSequence());
    }

    public void CompleteMiniGame()
    {
        MinigameManager.instance.MiniGameCompleted();
    }
}
