using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LivingTable
{
    // Append-only local checkpoints. Hashes detect corruption; they are not a trusted anti-cheat server.
    public static class GameSessionStore
    {
        [Serializable] public sealed class Entry
        {
            public int schema=1, revision;
            public string id, game, utc, reason, previous, payload, hash;
        }
        public static string TestRoot;
        public static bool Enabled => !Application.isEditor || TestRoot!=null;
        public static string Root => TestRoot ?? Path.Combine(Application.persistentDataPath,"sessions");
        public static string NewId()=>Guid.NewGuid().ToString("N");
        static string Folder(string id)
        {
            if(id==null||id.Length!=32||!id.All(Uri.IsHexDigit))throw new ArgumentException("Invalid session ID");
            return Path.Combine(Root,id);
        }
        static string Hash(Entry e)
        {
            using(var sha=SHA256.Create())return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(e.schema+"\n"+e.id+"\n"+e.game+"\n"+e.revision+"\n"+e.utc+"\n"+e.reason+"\n"+e.previous+"\n"+e.payload)));
        }
        public static Entry Load(string id)
        {
            Entry last=null;string folder=Folder(id);
            foreach(var file in Directory.GetFiles(folder,"*.json").OrderBy(p=>p,StringComparer.Ordinal))
            {
                var e=JsonUtility.FromJson<Entry>(File.ReadAllText(file));
                if(e==null||e.schema!=1||e.id!=id||e.revision!=(last?.revision??0)+1||e.previous!=(last?.hash??"")||e.hash!=Hash(e)||last!=null&&last.game!=e.game)
                    throw new InvalidDataException("Session history is damaged; it has not been reset.");
                last=e;
            }
            return last??throw new InvalidDataException("No complete checkpoint.");
        }
        public static void Save(string id,string game,string reason,string payload)
        {
            if(!Enabled)return;
            string folder=Folder(id);Directory.CreateDirectory(folder);
            var last=Directory.GetFiles(folder,"*.json").Length==0?null:Load(id);
            if(last!=null&&last.game!=game)throw new InvalidDataException("Wrong game for session");
            var entry=new Entry{id=id,game=game,revision=(last?.revision??0)+1,utc=DateTime.UtcNow.ToString("O"),reason=reason,previous=last?.hash??"",payload=payload};
            entry.hash=Hash(entry);string target=Path.Combine(folder,entry.revision.ToString("D8")+".json"),temp=target+".tmp";
            byte[] bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(entry,true));
            using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
            File.Move(temp,target);
        }
        public static Entry[] List(string game)
        {
            if(!Enabled||!Directory.Exists(Root))return Array.Empty<Entry>();
            return Directory.GetDirectories(Root).Select(path=>{
                try {return Load(Path.GetFileName(path));}
                catch(Exception ex){Debug.LogWarning("[Session] Cannot open "+Path.GetFileName(path)+": "+ex.Message);return null;}
            }).Where(e=>e!=null&&e.game==game).OrderByDescending(e=>e.utc,StringComparer.Ordinal).ToArray();
        }
    }
}
