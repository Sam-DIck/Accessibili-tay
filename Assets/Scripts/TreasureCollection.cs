
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TreasureCollection : MonoBehaviour
{
    [Header("Treasure Detection")]
    [SerializeField] private string treasureTag = "Treasure";
    [SerializeField] private float collectionDistance = 3f;
    [SerializeField] private int targetScore = 3;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text promptText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip closeTreasureClip;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip missionCompleteClip;

    private SubController subController;
    private GameObject nearestTreasure;
    private GameObject promptedTreasure;

    private int score;
    private bool hasPlayedClosePrompt;
    private bool missionComplete;

    private void Awake()
    {
        subController = GetComponent<SubController>();

        if (subController == null)
            subController = FindAnyObjectByType<SubController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateScoreUI();

        if (promptText != null)
            promptText.text = "Find a treasure!";
    }

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;

        // Handle input after the mission has been completed.
        if (missionComplete)
        {
            HandleMissionCompleteInput(gamepad);
            return;
        }

        if (subController == null)
            return;

        FindNearestTreasure();

        if (nearestTreasure == null)
        {
            promptedTreasure = null;
            hasPlayedClosePrompt = false;

            if (promptText != null)
                promptText.text = "Find a treasure!";

            return;
        }

        float distance = Vector2.Distance(
            subController.position,
            nearestTreasure.transform.position
        );

        if (distance <= collectionDistance)
        {
            HandleTreasureNearby(gamepad);
        }
        else
        {
            promptedTreasure = null;
            hasPlayedClosePrompt = false;

            if (promptText != null)
                promptText.text = "Move closer to the treasure!";
        }
    }

    private void FindNearestTreasure()
    {
        GameObject[] treasures =
            GameObject.FindGameObjectsWithTag(treasureTag);

        nearestTreasure = null;
        float closestDistanceSquared = float.PositiveInfinity;

        Vector2 playerPosition = subController.position;

        foreach (GameObject treasure in treasures)
        {
            if (treasure == null)
                continue;

            float distanceSquared =
                ((Vector2)treasure.transform.position - playerPosition)
                .sqrMagnitude;

            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                nearestTreasure = treasure;
            }
        }
    }

    private void HandleTreasureNearby(Gamepad gamepad)
    {
        if (promptText != null)
        {
            promptText.text =
                "You are close to the treasure!\n" +
                "Press X to collect the treasure.";
        }

        if (promptedTreasure != nearestTreasure)
        {
            promptedTreasure = nearestTreasure;
            hasPlayedClosePrompt = false;
        }

        // Play the nearby prompt once per approach.
        if (!hasPlayedClosePrompt)
        {
            hasPlayedClosePrompt = true;

            if (audioSource != null && closeTreasureClip != null)
                audioSource.PlayOneShot(closeTreasureClip);
        }

        // X / Cross button.
        if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame)
        {
            CollectTreasure();
        }
    }

    private void CollectTreasure()
    {
        if (nearestTreasure == null || missionComplete)
            return;

        Destroy(nearestTreasure);

        score++;
        UpdateScoreUI();

        nearestTreasure = null;
        promptedTreasure = null;
        hasPlayedClosePrompt = false;

        // Finish the mission when the target score is reached.
        if (score >= targetScore)
        {
            CompleteMission();
            return;
        }

        if (audioSource != null && successClip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(successClip);
        }

        if (promptText != null)
        {
            promptText.text =
                "Successfully collected the treasure! Well done!";
        }
    }

    private void CompleteMission()
    {
        missionComplete = true;

        if (audioSource != null)
        {
            audioSource.Stop();

            if (missionCompleteClip != null)
                audioSource.PlayOneShot(missionCompleteClip);
        }

        if (promptText != null)
        {
            promptText.text =
                "MISSION COMPLETE!\n" +
                "Well done!\n\n" +
                "Press X to replay\n" +
                "Press O to quit";
        }
    }

    private void HandleMissionCompleteInput(Gamepad gamepad)
    {
        if (gamepad == null)
            return;

        // X / Cross: restart the current scene.
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
        // O / Circle: quit the game.
        else if (gamepad.buttonEast.wasPressedThisFrame)
        {
            QuitGame();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
