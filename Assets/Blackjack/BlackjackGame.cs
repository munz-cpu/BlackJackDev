using System;
using System.Collections.Generic;

namespace Blackjack
{
    public enum RoundPhase { Ready, PlayerTurn, Settled }
    public enum RoundResult { None, Win, Lose, Push, Blackjack }

    public readonly struct Card
    {
        public readonly int Rank;
        public readonly int Suit;
        public Card(int rank, int suit) { Rank = rank; Suit = suit; }
    }

    // Rules only. The Canvas controller owns presentation and button input.
    public sealed class BlackjackGame
    {
        private readonly List<Card> deck = new List<Card>(52);
        private readonly List<Card> player = new List<Card>();
        private readonly List<Card> dealer = new List<Card>();
        private readonly Random random = new Random();
        private int nextCard;

        public IReadOnlyList<Card> Player => player;
        public IReadOnlyList<Card> Dealer => dealer;
        public RoundPhase Phase { get; private set; } = RoundPhase.Ready;
        public RoundResult Result { get; private set; } = RoundResult.None;
        public int PlayerScore => Score(player);
        public int DealerScore => Score(dealer);

        public void StartRound()
        {
            player.Clear();
            dealer.Clear();
            Result = RoundResult.None;
            Phase = RoundPhase.PlayerTurn;
            Shuffle();
            player.Add(Draw()); dealer.Add(Draw()); player.Add(Draw()); dealer.Add(Draw());
            if (PlayerScore == 21 || DealerScore == 21) Settle();
        }

        public void Hit()
        {
            if (Phase != RoundPhase.PlayerTurn) return;
            player.Add(Draw());
            if (PlayerScore > 21) Settle();
            else if (PlayerScore == 21) Stand();
        }

        public void Stand()
        {
            if (Phase != RoundPhase.PlayerTurn) return;
            while (DealerScore < 17) dealer.Add(Draw());
            Settle();
        }

        private void Settle()
        {
            bool playerBlackjack = player.Count == 2 && PlayerScore == 21;
            bool dealerBlackjack = dealer.Count == 2 && DealerScore == 21;
            if (PlayerScore > 21) Result = RoundResult.Lose;
            else if (playerBlackjack && !dealerBlackjack) Result = RoundResult.Blackjack;
            else if (dealerBlackjack && !playerBlackjack) Result = RoundResult.Lose;
            else if (DealerScore > 21 || PlayerScore > DealerScore) Result = RoundResult.Win;
            else if (PlayerScore < DealerScore) Result = RoundResult.Lose;
            else Result = RoundResult.Push;
            Phase = RoundPhase.Settled;
        }

        private void Shuffle()
        {
            deck.Clear();
            for (int suit = 0; suit < 4; suit++)
                for (int rank = 1; rank <= 13; rank++) deck.Add(new Card(rank, suit));
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
            nextCard = 0;
        }

        private Card Draw() => deck[nextCard++];

        public static int Score(IReadOnlyList<Card> cards)
        {
            int total = 0, aces = 0;
            foreach (Card card in cards)
            {
                if (card.Rank == 1) { total += 11; aces++; }
                else total += Math.Min(card.Rank, 10);
            }
            while (total > 21 && aces-- > 0) total -= 10;
            return total;
        }
    }
}
