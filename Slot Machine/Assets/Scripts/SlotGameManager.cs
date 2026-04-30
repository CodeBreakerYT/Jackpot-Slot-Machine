using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SlotGameManager : MonoBehaviour
{
    [Header("Game Data")]
    public List<SymbolData> availableSymbols = new List<SymbolData>();
    
    [Header("Reel References")]
    public SlotReel[] reels;
    
    [Header("UI References")]
    public Button spinButton;
    public GameObject spinningImage;
    public TextMeshProUGUI resultText; 
    public GameObject winPopup;

    [Header("Betting UI")]
    public TextMeshProUGUI moneyText;
    public Button bet500Button;
    public Button bet1000Button;
    public Button bet2000Button;

    [Header("Betting Data")]
    public int currentMoney = 1000;
    public int currentBet = 500;
    private int previousMoney;

    [Header("Win Popup Animation")]
    public float winPopupEndScale = 1.0f;
    public float winPopupAnimationDuration = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip spinSound;

    private int reelsSpinning = 0;

    void Start()
    {
        previousMoney = currentMoney;
        
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(OnSpinClicked);
        }
        
        if (winPopup != null) 
        {
            winPopup.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "Welcome! Press Spin.";
        }

        if (bet500Button != null) bet500Button.onClick.AddListener(() => SetBet(500));
        if (bet1000Button != null) bet1000Button.onClick.AddListener(() => SetBet(1000));
        if (bet2000Button != null) bet2000Button.onClick.AddListener(() => SetBet(2000));

        if (spinningImage != null) spinningImage.SetActive(false);

        UpdateMoneyUI();

        foreach (var reel in reels)
        {
            reel.Initialize(this);
        }
    }

    void Update()
    {
        if (currentMoney != previousMoney)
        {
            previousMoney = currentMoney;
            UpdateMoneyUI();
        }
    }

    private void SetBet(int amount)
    {
        currentBet = amount;
        if (resultText != null) resultText.text = "Bet set to " + amount + ". Press Spin.";
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: " + currentMoney;
        }
    }

    public void OnSpinClicked()
    {
        if (reelsSpinning > 0) return; // Already spinning

        if (currentMoney < currentBet)
        {
            if (resultText != null) resultText.text = "Not enough money!";
            return;
        }

        currentMoney -= currentBet;
        UpdateMoneyUI();

        if (spinButton != null) spinButton.gameObject.SetActive(false);
        if (spinningImage != null) spinningImage.SetActive(true);
        if (bet500Button != null) bet500Button.gameObject.SetActive(false);
        if (bet1000Button != null) bet1000Button.gameObject.SetActive(false);
        if (bet2000Button != null) bet2000Button.gameObject.SetActive(false);

        if (winPopup != null) winPopup.SetActive(false);
        if (resultText != null) resultText.text = "Spinning...";
        
        reelsSpinning = reels.Length;

        if (audioSource != null && spinSound != null)
        {
            audioSource.clip = spinSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Start spinning with slightly different stop delays to create suspense
        float baseDelay = 1.0f;
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StartSpin(baseDelay + (i * 0.5f)); // Stop sequentially
        }
    }

    public void ReelStopped(SlotReel reel)
    {
        reelsSpinning--;

        if (reelsSpinning <= 0)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            CheckResults();
        }
    }

    private void CheckResults()
    {
        if (reels.Length == 0) return;

        int firstSymbolId = reels[0].GetFinalSymbolId();
        bool allMatch = true;

        for (int i = 1; i < reels.Length; i++)
        {
            if (reels[i].GetFinalSymbolId() != firstSymbolId)
            {
                allMatch = false;
                break;
            }
        }

        if (allMatch)
        {
            int winnings = currentBet * 10;
            currentMoney += winnings;
            UpdateMoneyUI();

            if (resultText != null) resultText.text = "JACKPOT! YOU WIN " + winnings + "!";
            if (winPopup != null) StartCoroutine(AnimateWinPopup());
            
            if (audioSource != null && winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }

            StartCoroutine(ReloadSceneAfterDelay(5f));
            
            if (spinButton != null) { spinButton.gameObject.SetActive(true); spinButton.interactable = false; }
            if (bet500Button != null) { bet500Button.gameObject.SetActive(true); bet500Button.interactable = false; }
            if (bet1000Button != null) { bet1000Button.gameObject.SetActive(true); bet1000Button.interactable = false; }
            if (bet2000Button != null) { bet2000Button.gameObject.SetActive(true); bet2000Button.interactable = false; }
            if (spinningImage != null) spinningImage.SetActive(false);
        }
        else
        {
            if (resultText != null) resultText.text = "Try Again!";
            
            if (spinButton != null) spinButton.gameObject.SetActive(true);
            if (bet500Button != null) bet500Button.gameObject.SetActive(true);
            if (bet1000Button != null) bet1000Button.gameObject.SetActive(true);
            if (bet2000Button != null) bet2000Button.gameObject.SetActive(true);
            if (spinningImage != null) spinningImage.SetActive(false);
        }
    }

    private IEnumerator AnimateWinPopup()
    {
        if (winPopup == null) yield break;

        winPopup.SetActive(true);
        winPopup.transform.localScale = Vector3.one * 0.1f;
        
        CanvasGroup canvasGroup = winPopup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = winPopup.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < winPopupAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / winPopupAnimationDuration;
            
            winPopup.transform.localScale = Vector3.Lerp(Vector3.one * 0.1f, Vector3.one * winPopupEndScale, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            yield return null;
        }

        winPopup.transform.localScale = Vector3.one * winPopupEndScale;
        canvasGroup.alpha = 1f;
    }

    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
