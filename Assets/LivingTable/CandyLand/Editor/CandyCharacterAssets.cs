#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace LivingTable.Editor
{
 public static class CandyCharacterAssets
 {
  public static void Bake()
  {
   const string folder="Assets/LivingTable/CandyLand/Resources/Characters";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
   var palette=new Dictionary<Color,Material>();
   Material Mat(Color c){if(palette.TryGetValue(c,out var m))return m;string path=folder+"/plastic-"+ColorUtility.ToHtmlStringRGB(c)+".mat";m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Resources.Load<Shader>("CandyPiecePlastic"));m.color=c;AssetDatabase.CreateAsset(m,path);}palette[c]=m;return m;}
   for(int id=0;id<CandyLand.CandyCharacterModel.Ids.Length;id++)
   {
    string name=CandyLand.CandyCharacterModel.Ids[id],path=folder+"/"+name+".prefab";
    if(id>=0)
    {
     string modelFolder=new[]{"Plumpy","MrMint","Jolly","GrammaNutt","PrincessLolly","QueenFrostine","KingKandy","LordLicorice","Gloppy"}[id];
     var imported=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LivingTable/CandyLand/Models/"+modelFolder+"/"+name+"-sculpt.fbx");
     if(imported==null)throw new Exception(name+" sculpture import missing.");
     var root=new GameObject(CandyLand.CandyCharacterModel.Names[id]+" full sculpture");var model=UnityEngine.Object.Instantiate(imported,root.transform);model.name="Sculpted "+CandyLand.CandyCharacterModel.Names[id];model.transform.localRotation=Quaternion.Euler(0,180,0);
     foreach(var renderer in model.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>Mat(m.color)).ToArray();
     var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
     model.transform.localScale*=.96f/bounds.size.y;model.transform.localPosition=Vector3.up*(-bounds.min.y*.96f/bounds.size.y);
     root.AddComponent<CandyLand.CandyCharacterActor>().CharacterId=id;PrefabUtility.SaveAsPrefabAsset(root,path);UnityEngine.Object.DestroyImmediate(root);continue;
    }
    if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)continue;
    var source=new GameObject("Source");CandyLand.CandyCharacterModel.Build(source.transform,id,Mat);
    var baked=new GameObject(CandyLand.CandyCharacterModel.Names[id]);
    int group=0;
    foreach(var parts in source.GetComponentsInChildren<MeshRenderer>().GroupBy(r=>r.sharedMaterial))
    {
     var combine=parts.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=r.transform.localToWorldMatrix}).ToArray();
     var mesh=new Mesh{name=name+" surface "+group,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(combine,true,true);
     string meshPath=folder+"/"+name+"-"+group+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(existing!=null){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,meshPath);var g=new GameObject(parts.Key.name);g.transform.SetParent(baked.transform,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=parts.Key;group++;
    }
    baked.AddComponent<CandyLand.CandyCharacterActor>().CharacterId=id;PrefabUtility.SaveAsPrefabAsset(baked,path);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(baked);
   }
   foreach(string id in CandyLand.CandyCharacterModel.Ids){string path=folder+"/"+id+".png";var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer!=null){importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.maxTextureSize=1024;importer.SaveAndReimport();}}
   AssetDatabase.SaveAssets();
  }
 }
}
#endif
