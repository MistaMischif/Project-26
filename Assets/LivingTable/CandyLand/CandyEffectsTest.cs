using UnityEngine;
namespace LivingTable.CandyLand
{
    public sealed partial class CandyTable
    {
        public void StartEffectsTest()
        {
            if(Phase!=CandyPhase.Setup||StorageBlocked)return;
            EffectsTestMode=true;Configure(1,false,false,false);StartGame(42);SessionId=null;
            DrawTestSpecial(0);
        }
        public void DrawTestSpecial(int character)
        {
            if(!EffectsTestMode||Paused||Phase!=CandyPhase.Ready||character<0||character>=pictureNames.Length)return;
            testCharacter=character;ReleasePieceGrabs(true);Cards.Clear();MoveMagic?.Clear();
            // Dedicated unsaved sandbox: normal sessions and their deck order are never edited.
            Game=new CandyGame(Data,2,42);BuildPawns(2);
            var card=new CandyCard(CandyColor.Picture,1,pictureNames[character]);Game.ObserveCard(card);
            pendingShortcutPassed=false;AnnounceCard(card);StartCoroutine(PresentAndMove(card));
            Message="Effects test: "+pictureNames[character]+". REPLAY or NEXT SPECIAL after landing.";
        }
    }
}
