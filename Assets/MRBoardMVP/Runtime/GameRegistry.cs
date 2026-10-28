using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRBoardMVP
{
    public sealed class GameRegistry
    {
        readonly Dictionary<string, GameDefinition> games = new Dictionary<string, GameDefinition>();
        public IEnumerable<GameDefinition> Games => games.Values;
        public GameDefinition Register(string json)
        {
            var definition = JsonUtility.FromJson<GameDefinition>(json);
            if (definition == null) throw new ArgumentException("Invalid game JSON.");
            definition.Validate();
            if (games.ContainsKey(definition.id)) throw new ArgumentException("Game already registered: " + definition.id);
            games.Add(definition.id, definition);
            return definition;
        }
        public GameDefinition Get(string id) => games.TryGetValue(id, out var game)
            ? game : throw new KeyNotFoundException(id);
    }
}
