using System;
using UnityEngine;

namespace MRBoardMVP
{
    [Serializable] public class PieceDefinition
    {
        public string id;
        public string label;
        public int x;
        public int y;
        public string shape = "Cylinder";
        public string color = "#35B9FF";
    }

    [Serializable] public class GameDefinition
    {
        public string id;
        public string title;
        public int columns = 8;
        public int rows = 8;
        public float cellSize = 0.045f;
        public PieceDefinition[] pieces;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Game ID is required.");
            if (columns < 1 || columns > 32 || rows < 1 || rows > 32)
                throw new ArgumentException("Board dimensions must be between 1 and 32.");
            if (float.IsNaN(cellSize) || float.IsInfinity(cellSize) || cellSize < .01f || cellSize > .25f)
                throw new ArgumentException("Cell size must be between .01 and .25 meters.");
            var ids = new System.Collections.Generic.HashSet<string>();
            var cells = new System.Collections.Generic.HashSet<Vector2Int>();
            foreach (var p in pieces ?? Array.Empty<PieceDefinition>())
            {
                if (p == null || string.IsNullOrWhiteSpace(p.id) || !ids.Add(p.id))
                    throw new ArgumentException("Each piece needs a unique nonempty ID.");
                if (p.x < 0 || p.x >= columns || p.y < 0 || p.y >= rows)
                    throw new ArgumentException("Piece lies outside the board: " + p.id);
                if (!cells.Add(new Vector2Int(p.x, p.y)))
                    throw new ArgumentException("Two pieces occupy the same starting cell.");
            }
        }
    }
}
