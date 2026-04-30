using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotReel : MonoBehaviour
{
    [Header("Reel Settings")]
    public float spinSpeed = 2000f;
    public float symbolHeight = 150f;
    
    [Header("References")]
    public RectTransform[] symbolRects;
    public Image[] symbolImages;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stopSound;

    private bool isSpinning = false;
    private int finalSymbolId;
    private SlotGameManager gameManager;
    private float stopDelay;

    public void Initialize(SlotGameManager manager)
    {
        gameManager = manager;
        // Set random initial sprites
        foreach (var img in symbolImages)
        {
            SetRandomSymbol(img);
        }
    }

    public void StartSpin(float delayBeforeStop)
    {
        isSpinning = true;
        stopDelay = delayBeforeStop;
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        float timer = 0f;
        
        while (isSpinning)
        {
            timer += Time.deltaTime;
            
            // Move symbols down
            for (int i = 0; i < symbolRects.Length; i++)
            {
                var rect = symbolRects[i];
                rect.anchoredPosition += Vector2.down * spinSpeed * Time.deltaTime;

                // If symbol goes out of bounds at the bottom, move it to the top
                if (rect.anchoredPosition.y <= -symbolHeight * 1.5f)
                {
                    // Find the highest Y position among all symbols to place it exactly above
                    float highestY = float.MinValue;
                    foreach (var r in symbolRects)
                    {
                        if (r.anchoredPosition.y > highestY) highestY = r.anchoredPosition.y;
                    }
                    
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, highestY + symbolHeight);
                    SetRandomSymbol(symbolImages[i]);
                }
            }

            if (timer >= stopDelay)
            {
                StopSpin();
            }

            yield return null;
        }
    }

    private void SetRandomSymbol(Image img)
    {
        if (gameManager == null || gameManager.availableSymbols.Count == 0) return;
        
        SymbolData randomSymbol = gameManager.availableSymbols[Random.Range(0, gameManager.availableSymbols.Count)];
        img.sprite = randomSymbol.symbolSprite;
    }

    private void StopSpin()
    {
        isSpinning = false;
        
        // Randomize the final outcome
        int randomOutcomeIndex = Random.Range(0, gameManager.availableSymbols.Count);
        SymbolData finalSymbol = gameManager.availableSymbols[randomOutcomeIndex];
        
        StartCoroutine(SmoothStop(finalSymbol));
    }

    private IEnumerator SmoothStop(SymbolData finalSymbol)
    {
        float snapSpeed = 5f;
        float t = 0;
        
        // Sort rects by Y descending to identify top, middle, bottom
        List<RectTransform> sortedRects = new List<RectTransform>(symbolRects);
        sortedRects.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));
        
        Vector2[] targetPositions = new Vector2[symbolRects.Length];
        
        if (sortedRects.Count >= 3)
        {
            // Assuming 3 rects: [0]=Top, [1]=Center, [2]=Bottom
            for(int i = 0; i < sortedRects.Count; i++)
            {
                // Align to grid based on their vertical order. Center one becomes 0.
                float targetY = (1 - i) * symbolHeight; 
                int originalIndex = System.Array.IndexOf(symbolRects, sortedRects[i]);
                targetPositions[originalIndex] = new Vector2(symbolRects[originalIndex].anchoredPosition.x, targetY);
            }
            
            // Assign the exact outcome to the middle rect
            int centerOriginalIndex = System.Array.IndexOf(symbolRects, sortedRects[1]);
            symbolImages[centerOriginalIndex].sprite = finalSymbol.symbolSprite;
            finalSymbolId = finalSymbol.id;
        }
        else
        {
            // Fallback for different number of rects
            for(int i = 0; i < symbolRects.Length; i++) targetPositions[i] = symbolRects[i].anchoredPosition;
            symbolImages[0].sprite = finalSymbol.symbolSprite;
            finalSymbolId = finalSymbol.id;
        }

        Vector2[] startPositions = new Vector2[symbolRects.Length];
        for(int i=0; i<symbolRects.Length; i++) startPositions[i] = symbolRects[i].anchoredPosition;

        // Animate the snapping into place
        while (t < 1f)
        {
            t += Time.deltaTime * snapSpeed;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // easeOutCubic
            
            for (int i = 0; i < symbolRects.Length; i++)
            {
                symbolRects[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], easeT);
            }
            yield return null;
        }
        
        for (int i = 0; i < symbolRects.Length; i++)
        {
            symbolRects[i].anchoredPosition = targetPositions[i];
        }

        if (audioSource != null && stopSound != null)
        {
            audioSource.PlayOneShot(stopSound);
        }

        gameManager.ReelStopped(this);
    }

    public int GetFinalSymbolId()
    {
        return finalSymbolId;
    }
}
