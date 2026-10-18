
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatTutorialSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startAudio;
    [SerializeField] private AudioClip moveBoatAudio;
    [SerializeField] private AudioClip periscopeLeftAudio;
    [SerializeField] private AudioClip periscopeRightAudio;
    [SerializeField] private AudioClip shipLeftAudio;
    [SerializeField] private AudioClip shipRightAudio;
    [SerializeField] private AudioClip wellDoneAudio;
    [SerializeField] private AudioClip completedAudio;

    [Header("Settings")]
    [SerializeField, Min(0.1f)] private float repeatInterval = 5f;
    [SerializeField, Range(0.1f, 1f)] private float stickThreshold = 0.6f;
    [SerializeField, Range(0.1f, 1f)] private float triggerThreshold = 0.5f;

    private bool tutorialRunning;

    private Gamepad Pad => Gamepad.current;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.loop = false;
    }

    private void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        tutorialRunning = true;

        // Introduction
        bool start = false;

        yield return WaitForInput(
            "Would you like to start the tutorial?\n\n" +
            "X (Cross) - Yes        O (Circle) - No",
            startAudio,
            () =>
            {
                if (Pad == null)
                    return false;

                if (Pad.buttonSouth.wasPressedThisFrame)
                {
                    start = true;
                    return true;
                }

                if (Pad.buttonEast.wasPressedThisFrame)
                {
                    start = false;
                    return true;
                }

                return false;
            }
        );

        if (!start)
        {
            HideTutorial();
            yield break;
        }

        // Step 1: Move the boat
        yield return WaitForInput(
            "Step 1\nPress R2 to move the boat.",
            moveBoatAudio,
            () => Pad != null &&
                  Pad.rightTrigger.ReadValue() > triggerThreshold
        );

        yield return SuccessAndContinue();

        // Step 2: Control the periscope
        yield return WaitForInput(
            "Step 2\nPress L1 to turn the periscope left.",
            periscopeLeftAudio,
            () => Pad != null &&
                  Pad.leftShoulder.wasPressedThisFrame
        );

        yield return WaitForInput(
            "Step 2\nPress R1 to turn the periscope right.",
            periscopeRightAudio,
            () => Pad != null &&
                  Pad.rightShoulder.wasPressedThisFrame
        );

        yield return SuccessAndContinue();

        // Step 3: Turn the ship
        yield return WaitForInput(
            "Step 3\nMove the left joystick to the left to turn the ship left.",
            shipLeftAudio,
            () => Pad != null &&
                  Pad.leftStick.x.ReadValue() < -stickThreshold
        );

        yield return WaitForInput(
            "Step 3\nMove the left joystick to the right to turn the ship right.",
            shipRightAudio,
            () => Pad != null &&
                  Pad.leftStick.x.ReadValue() > stickThreshold
        );

        //yield return SuccessAndContinue();

        // Completion
        yield return WaitForInput(
            "Tutorial completed!\nPress X (Cross) to finish.",
            completedAudio,
            () => Pad != null &&
                  Pad.buttonSouth.wasPressedThisFrame
        );

        HideTutorial();
    }

    private IEnumerator WaitForInput(
        string message,
        AudioClip clip,
        Func<bool> inputDetected)
    {
        if (dialogueText != null)
            dialogueText.text = message;

        while (true)
        {
            PlayAudio(clip);

            // Let the audio finish while monitoring player input.
            while (audioSource != null && audioSource.isPlaying)
            {
                if (inputDetected())
                {
                    StopAudio();
                    yield break;
                }

                yield return null;
            }

            // Wait before repeating the prompt.
            float elapsed = 0f;

            while (elapsed < repeatInterval)
            {
                if (inputDetected())
                {
                    StopAudio();
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (inputDetected())
            {
                StopAudio();
                yield break;
            }
        }
    }

    private IEnumerator SuccessAndContinue()
    {
        yield return WaitForInput(
            "Well done!\nPress X (Cross) to continue.",
            wellDoneAudio,
            () => Pad != null &&
                  Pad.buttonSouth.wasPressedThisFrame
        );
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void HideTutorial()
    {
        StopAudio();
        tutorialRunning = false;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void OnDisable()
    {
        StopAudio();
        StopAllCoroutines();
        tutorialRunning = false;
    }
}
