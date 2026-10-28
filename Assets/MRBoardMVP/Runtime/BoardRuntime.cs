using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MRBoardMVP
{
    public sealed class CellView : MonoBehaviour { public Vector2Int cell; }
    public sealed class PieceView : MonoBehaviour
    {
        public string id;
        public Vector2Int cell;
        public bool moving;
        public Renderer visual;
        public Color baseColor;
    }

    public sealed class BoardRuntime : MonoBehaviour
    {
        public Material materialTemplate;
        public event Action<string, Vector2Int, Vector2Int> PieceMoved;
        public GameDefinition Definition { get; private set; }
        public string SelectedId => selected != null ? selected.id : null;
        public bool IsAnimating => animating > 0;
        public int MoveCount { get; private set; }
        public string Feedback { get; private set; }
        public Func<string, Vector2Int, Vector2Int, bool> MoveValidator;
        readonly Dictionary<string, PieceView> pieces = new Dictionary<string, PieceView>();
        readonly Dictionary<Vector2Int, PieceView> occupancy = new Dictionary<Vector2Int, PieceView>();
        readonly List<Material> materials = new List<Material>();
        Transform content;
        PieceView selected;
        int animating;

        Material Material(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = materialTemplate != null ? new Material(materialTemplate) : new Material(shader);
            mat.color = color;
            materials.Add(mat);
            return mat;
        }

        public void Build(GameDefinition definition)
        {
            definition.Validate();
            StopAllCoroutines();
            animating = 0;
            MoveCount = 0;
            Feedback = "Select a piece, then an empty cell.";
            if (content != null) { content.gameObject.SetActive(false); Destroy(content.gameObject); }
            foreach (var mat in materials) Destroy(mat);
            materials.Clear(); pieces.Clear(); occupancy.Clear(); selected = null;
            Definition = definition;
            content = new GameObject("Generated Board").transform;
            content.SetParent(transform, false);
            var dark = Material(new Color(.10f, .13f, .17f));
            var light = Material(new Color(.72f, .78f, .83f));
            for (int y = 0; y < definition.rows; y++)
            for (int x = 0; x < definition.columns; x++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Cell {x},{y}";
                tile.transform.SetParent(content, false);
                tile.transform.localPosition = CellPosition(new Vector2Int(x, y), -.004f);
                tile.transform.localScale = new Vector3(definition.cellSize * .97f, .008f, definition.cellSize * .97f);
                tile.GetComponent<Renderer>().sharedMaterial = (x + y) % 2 == 0 ? light : dark;
                tile.AddComponent<CellView>().cell = new Vector2Int(x, y);
            }
            foreach (var p in definition.pieces ?? Array.Empty<PieceDefinition>())
            {
                PrimitiveType shape = p.shape == "Sphere" ? PrimitiveType.Sphere :
                    p.shape == "Cube" ? PrimitiveType.Cube : PrimitiveType.Cylinder;
                var go = GameObject.CreatePrimitive(shape);
                go.name = string.IsNullOrEmpty(p.label) ? p.id : p.label;
                go.transform.SetParent(content, false);
                var view = go.AddComponent<PieceView>();
                view.id = p.id; view.cell = new Vector2Int(p.x, p.y);
                go.transform.localPosition = CellPosition(view.cell, PieceHeight);
                float size = definition.cellSize;
                go.transform.localScale = shape == PrimitiveType.Cylinder
                    ? new Vector3(size * .60f, size * .24f, size * .60f)
                    : Vector3.one * size * .55f;
                Color color;
                if (!ColorUtility.TryParseHtmlString(p.color, out color)) color = Color.cyan;
                view.baseColor = color;
                view.visual = go.GetComponent<Renderer>();
                view.visual.sharedMaterial = Material(color);
                pieces.Add(p.id, view); occupancy.Add(view.cell, view);
            }
        }

        float PieceHeight => Definition.cellSize * .30f;
        Vector3 CellPosition(Vector2Int cell, float height) => new Vector3(
            (cell.x - (Definition.columns - 1) * .5f) * Definition.cellSize,
            height, (cell.y - (Definition.rows - 1) * .5f) * Definition.cellSize);

        public void Select(string id)
        {
            if (IsAnimating || !pieces.TryGetValue(id, out var piece)) return;
            if (selected != null) selected.visual.sharedMaterial.color = selected.baseColor;
            selected = piece;
            selected.visual.sharedMaterial.color = Color.Lerp(selected.baseColor, Color.white, .65f);
            Feedback = "Selected " + piece.gameObject.name + ". Choose an empty cell.";
        }

        public void InteractCell(Vector2Int cell)
        {
            if (IsAnimating) return;
            if (occupancy.TryGetValue(cell, out var piece)) Select(piece.id);
            else if (selected != null) TryMove(selected.id, cell);
            else Feedback = "Select a piece first.";
        }

        public bool TryGetPieceCell(string id, out Vector2Int cell)
        {
            if (pieces.TryGetValue(id, out var piece)) { cell = piece.cell; return true; }
            cell = default;
            return false;
        }

        public void Interact(Ray ray)
        {
            if (IsAnimating || !Physics.Raycast(ray, out var hit, 20f)) return;
            var piece = hit.collider.GetComponent<PieceView>();
            if (piece != null && piece.transform.IsChildOf(transform)) { Select(piece.id); return; }
            var tile = hit.collider.GetComponent<CellView>();
            if (tile != null && tile.transform.IsChildOf(transform) && selected != null)
                TryMove(selected.id, tile.cell);
        }

        public bool TryMove(string id, Vector2Int target)
        {
            if (Definition == null || IsAnimating || !pieces.TryGetValue(id, out var piece)) return false;
            if (target.x < 0 || target.y < 0 || target.x >= Definition.columns || target.y >= Definition.rows
                || occupancy.ContainsKey(target)) { Feedback = "Choose an empty cell inside the board."; return false; }
            if (MoveValidator != null && !MoveValidator(id, piece.cell, target)) { Feedback = "That move is not allowed."; return false; }
            Vector2Int previous = piece.cell;
            occupancy.Remove(previous); occupancy.Add(target, piece); piece.cell = target;
            StartCoroutine(AnimateMove(piece, previous, target));
            return true;
        }

        IEnumerator AnimateMove(PieceView piece, Vector2Int from, Vector2Int to)
        {
            animating++; piece.moving = true;
            Vector3 start = piece.transform.localPosition;
            Vector3 end = CellPosition(to, PieceHeight);
            float elapsed = 0;
            while (elapsed < .35f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / .35f);
                piece.transform.localPosition = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t))
                    + Vector3.up * Mathf.Sin(t * Mathf.PI) * Definition.cellSize;
                yield return null;
            }
            piece.transform.localPosition = end;
            piece.moving = false; animating--;
            MoveCount++;
            Feedback = $"{piece.gameObject.name} moved to {to.x + 1}, {to.y + 1}.";
            StartCoroutine(Pulse(end));
            PieceMoved?.Invoke(piece.id, from, to);
        }

        IEnumerator Pulse(Vector3 position)
        {
            var effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "Move Pulse";
            effect.transform.SetParent(content, false);
            effect.transform.localPosition = new Vector3(position.x, .004f, position.z);
            var collider = effect.GetComponent<Collider>();
            collider.enabled = false; Destroy(collider);
            effect.GetComponent<Renderer>().sharedMaterial = Material(new Color(.1f, 1f, .7f));
            float elapsed = 0;
            while (elapsed < .3f)
            {
                elapsed += Time.deltaTime;
                float size = Mathf.Sin(Mathf.Clamp01(elapsed / .3f) * Mathf.PI) * Definition.cellSize;
                effect.transform.localScale = new Vector3(size, .001f, size);
                yield return null;
            }
            // Remove the per-effect material rather than accumulating it over a session.
            var mat = effect.GetComponent<Renderer>().sharedMaterial;
            materials.Remove(mat); Destroy(mat); Destroy(effect);
        }

        void OnDestroy() { foreach (var mat in materials) if (mat != null) Destroy(mat); }
    }
}
