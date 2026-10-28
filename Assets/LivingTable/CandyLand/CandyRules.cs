using System;
using System.Collections.Generic;
using UnityEngine;

namespace LivingTable.CandyLand
{
    public enum CandyColor { Red, Purple, Yellow, Blue, Orange, Green, Picture }
    [Serializable] public sealed class CandySpace
    {
        public float x, y;
        public CandyColor color;
        public string picture;
        public int shortcut = -1;
        public int releaseColor = -1;
    }
    [Serializable] public sealed class CandyBoardData
    {
        public string source, status;
        public CandySpace[] spaces;
        public int[] singles, doubles;
        public void Validate()
        {
            if (spaces == null || spaces.Length < 2) throw new ArgumentException("Missing board path.");
            var pictures = new HashSet<string>();
            foreach (var s in spaces)
            {
                if (s == null || (int)s.color < 0 || (int)s.color > 6) throw new ArgumentException("Invalid board space.");
                if (s.x < 0 || s.x > 1 || s.y < 0 || s.y > 1 || float.IsNaN(s.x) || float.IsNaN(s.y)) throw new ArgumentException("Invalid space coordinates.");
                if (s.shortcut < -1 || s.shortcut >= spaces.Length || s.releaseColor < -1 || s.releaseColor > 5) throw new ArgumentException("Invalid special space.");
                if (s.color == CandyColor.Picture && (string.IsNullOrEmpty(s.picture) || !pictures.Add(s.picture))) throw new ArgumentException("Picture spaces must be unique.");
            }
            if (pictures.Count != 6 || singles == null || doubles == null || singles.Length != 6 || doubles.Length != 6) throw new ArgumentException("Incomplete deck definition.");
            int count = 6;
            for (int c = 0; c < 6; c++) { if (singles[c] < 0 || doubles[c] < 0) throw new ArgumentException("Negative deck count."); count += singles[c] + doubles[c]; }
            if (count != 64) throw new ArgumentException("Expected a 64-card deck.");
        }
    }
    public readonly struct CandyCard
    {
        public readonly CandyColor Color;
        public readonly int Count;
        public readonly string Picture;
        public CandyCard(CandyColor color, int count = 1, string picture = null) { Color = color; Count = count; Picture = picture; }
        public string Label => Color == CandyColor.Picture ? Picture : (Count == 2 ? "Double " : "") + Color;
    }
    public readonly struct CandyMove
    {
        public readonly int From, To, Via;
        public readonly bool Stuck, Won;
        public CandyMove(int from, int to, int via, bool stuck, bool won) { From = from; To = to; Via = via; Stuck = stuck; Won = won; }
    }

    // Pure turn rules: the narrator and recognition providers cannot override a legal result.
    public sealed class CandyGame
    {
        public readonly CandyBoardData Board;
        readonly System.Random random;readonly int seed;int randomCalls;
        readonly List<CandyCard> draw = new List<CandyCard>();
        readonly List<CandyCard> discard = new List<CandyCard>();
        readonly int[] positions;
        public int CurrentPlayer { get; private set; }
        public int Winner { get; private set; } = -1;
        public int Turn { get; private set; }
        public int Players => positions.Length;
        public int DrawCount => draw.Count;
        public int DiscardCount => discard.Count;
        public bool HasPending => pending.HasValue;
        public CandyCard? PendingCard => pending;
        CandyCard? pending;
        public int Position(int player) => positions[player];
        public CandyGame(CandyBoardData board, int players, int seed)
        {
            board.Validate();
            if (players < 2 || players > 4) throw new ArgumentOutOfRangeException(nameof(players));
            Board = board; this.seed=seed;random = new System.Random(seed); positions = new int[players];
            for (int i = 0; i < players; i++) positions[i] = -1;
            for (int c = 0; c < 6; c++)
            {
                for (int n = 0; n < board.singles[c]; n++) draw.Add(new CandyCard((CandyColor)c));
                for (int n = 0; n < board.doubles[c]; n++) draw.Add(new CandyCard((CandyColor)c, 2));
            }
            foreach (var s in board.spaces) if (s.color == CandyColor.Picture) draw.Add(new CandyCard(CandyColor.Picture, 1, s.picture));
            Shuffle();
        }
        [Serializable] public sealed class SavedCard {public int color,count;public string picture;public CandyCard Card=>new CandyCard((CandyColor)color,count,picture);public SavedCard(CandyCard c){color=(int)c.Color;count=c.Count;picture=c.Picture;}}
        [Serializable] public sealed class State {public bool hasPending;public int seed,randomCalls,current,winner,turn;public int[] positions;public SavedCard[] draw,discard;public SavedCard pending;}
        int Next(int max){randomCalls++;return random.Next(max);}
        public State Capture()=>new State{seed=seed,randomCalls=randomCalls,current=CurrentPlayer,winner=Winner,turn=Turn,positions=(int[])positions.Clone(),draw=draw.ConvertAll(c=>new SavedCard(c)).ToArray(),discard=discard.ConvertAll(c=>new SavedCard(c)).ToArray(),hasPending=pending.HasValue,pending=pending.HasValue?new SavedCard(pending.Value):null};
        public static CandyGame Restore(CandyBoardData board,State state){if(state==null||state.positions==null||state.positions.Length<2||state.positions.Length>4||state.draw==null||state.discard==null||state.current<0||state.current>=state.positions.Length||state.winner< -1||state.winner>=state.positions.Length||state.turn<0||state.randomCalls<63||state.randomCalls>1000000||Array.Exists(state.positions,p=>p< -1||p>=board.spaces.Length))throw new ArgumentException("Invalid saved Candy Land state");var game=new CandyGame(board,state.positions.Length,state.seed);var expected=game.draw.ConvertAll(c=>c.Label);var actual=new List<string>();foreach(var c in state.draw)actual.Add(c.Card.Label);foreach(var c in state.discard)actual.Add(c.Card.Label);if(state.hasPending)actual.Add(state.pending.Card.Label);expected.Sort();actual.Sort();if(expected.Count!=actual.Count||!System.Linq.Enumerable.SequenceEqual(expected,actual))throw new ArgumentException("Saved deck does not conserve its cards");while(game.randomCalls<state.randomCalls)game.Next(1);game.draw.Clear();game.discard.Clear();foreach(var c in state.draw){game.ValidateCard(c.Card);game.draw.Add(c.Card);}foreach(var c in state.discard){game.ValidateCard(c.Card);game.discard.Add(c.Card);}if(state.hasPending){game.ValidateCard(state.pending.Card);game.pending=state.pending.Card;}Array.Copy(state.positions,game.positions,state.positions.Length);game.CurrentPlayer=state.current;game.Winner=state.winner;game.Turn=state.turn;return game;}
        void Shuffle() { for (int i = draw.Count - 1; i > 0; i--) { int j = Next(i + 1); var t = draw[i]; draw[i] = draw[j]; draw[j] = t; } }
        public CandyCard Draw()
        {
            EnsureReady();
            if (draw.Count == 0) { draw.AddRange(discard); discard.Clear(); Shuffle(); }
            var card = draw[draw.Count - 1]; draw.RemoveAt(draw.Count - 1); pending = card; return card;
        }
        // Track physical draws in the same remaining deck so changing surfaces cannot reroll spent cards.
        public void ObserveCard(CandyCard card)
        {
            EnsureReady(); ValidateCard(card);if(draw.Count==0){draw.AddRange(discard);discard.Clear();Shuffle();}int index=draw.FindIndex(c=>c.Color==card.Color&&c.Count==card.Count&&c.Picture==card.Picture);if(index<0)throw new InvalidOperationException("That card has already been used. Keep the same deck until it is exhausted.");draw.RemoveAt(index);pending = card;
        }
        void EnsureReady() { if (Winner >= 0 || pending.HasValue) throw new InvalidOperationException("Finish the pending turn first."); }
        void ValidateCard(CandyCard card)
        {
            if (card.Color == CandyColor.Picture)
            {
                if (Array.FindIndex(Board.spaces, s => s.picture == card.Picture && s.color == CandyColor.Picture) < 0) throw new ArgumentException("Unknown picture card.");
            }
            else if ((int)card.Color < 0 || (int)card.Color > 5 || (card.Count != 1 && card.Count != 2)) throw new ArgumentException("Invalid color card.");
        }
        public CandyMove Resolve(int from, CandyCard card)
        {
            ValidateCard(card);
            if (from < -1 || from >= Board.spaces.Length) throw new ArgumentOutOfRangeException(nameof(from));
            if (from >= 0 && Board.spaces[from].releaseColor >= 0 && (int)card.Color != Board.spaces[from].releaseColor)
                return new CandyMove(from, from, from, true, false);
            int destination = from;
            if (card.Color == CandyColor.Picture) destination = Array.FindIndex(Board.spaces, s => s.picture == card.Picture && s.color == CandyColor.Picture);
            else
            {
                int remaining = card.Count;
                while (++destination < Board.spaces.Length) if (Board.spaces[destination].color == card.Color && --remaining == 0) break;
            }
            destination = Math.Min(destination, Board.spaces.Length - 1);
            int via = destination;
            if (Board.spaces[destination].shortcut >= 0) destination = Board.spaces[destination].shortcut;
            return new CandyMove(from, destination, via, false, destination == Board.spaces.Length - 1);
        }
        public CandyMove Preview() { if (!pending.HasValue) throw new InvalidOperationException("Draw or confirm a card."); return Resolve(positions[CurrentPlayer], pending.Value); }
        public CandyMove Commit(int confirmedDestination)
        {
            var move = Preview();
            if (confirmedDestination != move.To) throw new InvalidOperationException("That space does not match this card.");
            positions[CurrentPlayer] = move.To;
            discard.Add(pending.Value); pending = null; Turn++;
            if (move.Won) Winner = CurrentPlayer;
            else CurrentPlayer = (CurrentPlayer + 1) % Players;
            return move;
        }
    }
}
