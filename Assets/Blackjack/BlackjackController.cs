using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Blackjack
{
    public sealed class BlackjackController : MonoBehaviour
    {
        [Header("Coins (editable in the Inspector)")]
        [SerializeField, Min(0)] private int startingCoins = 10;
        [SerializeField, Min(0)] private int coinsPerWin = 1;
        [SerializeField, Min(0)] private int coinsPerLoss = 1;
        [Header("Canvas references")]
        [SerializeField] private Text statusText, dealerScoreText, playerScoreText;
        [SerializeField] private Text coinsText, outcomeText, coinChangeText, popupBalanceText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private RectTransform dealerCards, playerCards, deckCard;
        [SerializeField] private Button startButton, hitButton, standButton, restartButton;
        [Header("Card art: suit order matches BlackjackGame")]
        [SerializeField] private Sprite[] anaCards, crealCards, paraCards, tapieCards;
        [SerializeField] private Sprite cardBack;
        [Header("Deal animation")]
        [SerializeField, Min(0.05f)] private float travelSeconds = 0.32f;
        [SerializeField, Min(0.05f)] private float flipSeconds = 0.16f;
        [Header("Card sound")]
        [SerializeField] private AudioSource cardAudioSource;
        [SerializeField] private AudioClip cardSound;
        [SerializeField, Range(0f, 1f)] private float cardSoundVolume = 0.7f;
        [Header("Result sounds")]
        [SerializeField] private AudioClip successSound, failureSound;
        [SerializeField, Range(0f, 1f)] private float resultSoundVolume = 0.7f;

        private readonly List<Image> dealerImages = new List<Image>();
        private readonly List<Image> playerImages = new List<Image>();
        private BlackjackGame game;
        private RectTransform canvasRect;
        private bool animating;
        private int coins;
        private int lastCoinChange;
        private bool resultApplied;

        public void Configure(Text status, Text dealerScore, Text playerScore, Text coinCount,
            GameObject popup, Text outcome, Text coinChange, Text popupBalance,
            RectTransform dealerHand, RectTransform playerHand, RectTransform deck,
            Button start, Button hit, Button stand, Button restart, Sprite[] ana,
            Sprite[] creal, Sprite[] para, Sprite[] tapie, Sprite back,
            AudioSource audioSource, AudioClip sound, AudioClip success, AudioClip failure)
        {
            statusText = status; dealerScoreText = dealerScore; playerScoreText = playerScore;
            coinsText = coinCount; resultPanel = popup; outcomeText = outcome;
            coinChangeText = coinChange; popupBalanceText = popupBalance;
            dealerCards = dealerHand; playerCards = playerHand; deckCard = deck;
            startButton = start; hitButton = hit; standButton = stand; restartButton = restart;
            anaCards = ana; crealCards = creal; paraCards = para; tapieCards = tapie; cardBack = back;
            cardAudioSource = audioSource; cardSound = sound;
            successSound = success; failureSound = failure;
        }

        private void Awake()
        {
            canvasRect = GetComponent<RectTransform>();
            game = new BlackjackGame();
            coins = startingCoins;
            startButton.onClick.AddListener(StartRound);
            hitButton.onClick.AddListener(Hit);
            standButton.onClick.AddListener(Stand);
            restartButton.onClick.AddListener(StartRound);
            Refresh();
        }

        private void StartRound()
        {
            if (animating) return;
            resultApplied = false;
            lastCoinChange = 0;
            game.StartRound();
            Refresh();
            var targets = new List<Image>(4) { playerImages[0], dealerImages[0], playerImages[1], dealerImages[1] };
            StartCoroutine(DealSequence(targets, game.Phase == RoundPhase.Settled));
        }

        private void Hit()
        {
            if (animating || game.Phase != RoundPhase.PlayerTurn) return;
            game.Hit();
            Refresh();
            StartCoroutine(DealSequence(new List<Image> { playerImages[playerImages.Count - 1] }, game.Phase == RoundPhase.Settled));
        }

        private void Stand()
        {
            if (animating || game.Phase != RoundPhase.PlayerTurn) return;
            int oldDealerCount = game.Dealer.Count;
            game.Stand();
            Refresh();
            var targets = new List<Image>();
            for (int i = oldDealerCount; i < dealerImages.Count; i++) targets.Add(dealerImages[i]);
            StartCoroutine(DealSequence(targets, true));
        }

        private IEnumerator DealSequence(List<Image> targets, bool revealHole)
        {
            animating = true;
            SetControls(false);
            statusText.text = "Dealing...";
            if (revealHole && dealerImages.Count > 1) dealerImages[1].sprite = cardBack;
            foreach (Image target in targets) target.color = new Color(1, 1, 1, 0);

            foreach (Image target in targets)
            {
                yield return FlyFromDeck(target);
                PlayCardSound();
                target.color = Color.white;
                if (target != dealerImages[1])
                {
                    Sprite face = target.sprite;
                    target.sprite = cardBack;
                    yield return Flip(target, face);
                }
            }
            if (revealHole && dealerImages.Count > 1)
            {
                PlayCardSound();
                yield return Flip(dealerImages[1], GetFace(game.Dealer[1]));
            }
            animating = false;
            if (game.Phase == RoundPhase.Settled) ApplyResult();
            Refresh();
            if (game.Phase == RoundPhase.Settled) PlayResultSound();
        }

        private void ApplyResult()
        {
            if (resultApplied) return;
            int previousCoins = coins;
            if (game.Result == RoundResult.Win || game.Result == RoundResult.Blackjack)
                coins += coinsPerWin;
            else if (game.Result == RoundResult.Lose)
                coins = Mathf.Max(0, coins - coinsPerLoss);
            lastCoinChange = coins - previousCoins;
            resultApplied = true;
        }

        private void PlayCardSound()
        {
            if (cardAudioSource != null && cardSound != null)
                cardAudioSource.PlayOneShot(cardSound, cardSoundVolume);
        }

        private void PlayResultSound()
        {
            AudioClip resultSound = game.Result == RoundResult.Win || game.Result == RoundResult.Blackjack
                ? successSound : game.Result == RoundResult.Lose ? failureSound : null;
            if (cardAudioSource != null && resultSound != null)
                cardAudioSource.PlayOneShot(resultSound, resultSoundVolume);
        }

        private IEnumerator FlyFromDeck(Image target)
        {
            GameObject flyingObject = new GameObject("Flying Card", typeof(RectTransform), typeof(Image));
            RectTransform flying = flyingObject.GetComponent<RectTransform>();
            flying.SetParent(canvasRect, false);
            flying.SetAsLastSibling();
            flying.sizeDelta = target.rectTransform.sizeDelta;
            Image image = flyingObject.GetComponent<Image>();
            image.sprite = cardBack;
            image.preserveAspect = true;
            image.raycastTarget = false;
            AddCardShadow(image);
            Vector2 start = canvasRect.InverseTransformPoint(deckCard.position);
            Vector2 end = canvasRect.InverseTransformPoint(target.rectTransform.position);
            float elapsed = 0;
            while (elapsed < travelSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / travelSeconds);
                float eased = 1 - Mathf.Pow(1 - t, 3);
                flying.anchoredPosition = Vector2.LerpUnclamped(start, end, eased) + Vector2.up * (Mathf.Sin(t * Mathf.PI) * 25);
                flying.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-10, 0, eased));
                flying.localScale = Vector3.one * Mathf.Lerp(0.88f, 1, eased);
                yield return null;
            }
            Destroy(flyingObject);
        }

        private IEnumerator Flip(Image image, Sprite face)
        {
            RectTransform rect = image.rectTransform;
            float half = flipSeconds * 0.5f;
            float elapsed = 0;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                rect.localScale = new Vector3(1 - Mathf.Clamp01(elapsed / half), 1, 1);
                yield return null;
            }
            image.sprite = face;
            elapsed = 0;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                rect.localScale = new Vector3(Mathf.Clamp01(elapsed / half), 1, 1);
                yield return null;
            }
            rect.localScale = Vector3.one;
        }

        private void Refresh()
        {
            bool playing = game.Phase == RoundPhase.PlayerTurn;
            coinsText.text = $"COINS  {coins}";
            dealerScoreText.text = game.Dealer.Count == 0 ? "상대방" : playing ?
                $"상대방  {BlackjackGame.Score(new[] { game.Dealer[0] })} + ?" : $"상대방  {game.DealerScore}";
            playerScoreText.text = game.Player.Count == 0 ? "당신" : $"당신  {game.PlayerScore}";
            statusText.text = game.Phase == RoundPhase.Ready ? "시작 버튼을 눌러 시작" : playing ? "추가 or 쫄튀" : "라운드 끝!";
            outcomeText.text = game.Result == RoundResult.Blackjack ? "BLACKJACK!" :
                game.Result == RoundResult.Win ? "승리" :
                game.Result == RoundResult.Push ? "무승부" : "패배ㅋ";
            coinChangeText.text = lastCoinChange > 0 ? $"+{lastCoinChange} COIN" :
                lastCoinChange < 0 ? $"{lastCoinChange} COIN" : "COINS UNCHANGED";
            popupBalanceText.text = $"TOTAL COINS  {coins}";
            DrawHand(dealerCards, game.Dealer, playing, dealerImages);
            DrawHand(playerCards, game.Player, false, playerImages);
            startButton.gameObject.SetActive(game.Phase == RoundPhase.Ready);
            hitButton.gameObject.SetActive(playing);
            standButton.gameObject.SetActive(playing);
            resultPanel.SetActive(game.Phase == RoundPhase.Settled && resultApplied && !animating);
        }

        private void SetControls(bool active)
        {
            startButton.gameObject.SetActive(active && game.Phase == RoundPhase.Ready);
            hitButton.gameObject.SetActive(active && game.Phase == RoundPhase.PlayerTurn);
            standButton.gameObject.SetActive(active && game.Phase == RoundPhase.PlayerTurn);
            resultPanel.SetActive(active && game.Phase == RoundPhase.Settled && resultApplied);
        }

        private void DrawHand(RectTransform container, IReadOnlyList<Card> cards, bool hideSecond, List<Image> views)
        {
            views.Clear();
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Transform old = container.GetChild(i);
                old.SetParent(null, false);
                Destroy(old.gameObject);
            }
            for (int i = 0; i < cards.Count; i++)
            {
                GameObject view = new GameObject($"Card {i + 1}", typeof(RectTransform), typeof(Image));
                view.transform.SetParent(container, false);
                RectTransform rect = (RectTransform)view.transform;
                rect.sizeDelta = new Vector2(112, 153);
                Image image = view.GetComponent<Image>();
                image.sprite = hideSecond && i == 1 ? cardBack : GetFace(cards[i]);
                image.preserveAspect = true;
                image.raycastTarget = false;
                AddCardShadow(image);
                views.Add(image);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }

        private static void AddCardShadow(Image image)
        {
            Shadow shadow = image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.2f);
            shadow.effectDistance = new Vector2(3, -4);
            shadow.useGraphicAlpha = true;
        }

        private Sprite GetFace(Card card)
        {
            Sprite[] suit = card.Suit == 0 ? anaCards : card.Suit == 1 ? crealCards : card.Suit == 2 ? paraCards : tapieCards;
            return suit[card.Rank - 1];
        }
    }
}
