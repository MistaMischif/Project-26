using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  int[] observedProblem;ChessPosition problemTarget;Transform physicalGuides;
  readonly List<Mesh> guideMeshes=new List<Mesh>();readonly List<Renderer> pulseTiles=new List<Renderer>(),pulsePieces=new List<Renderer>();readonly List<Color> pulseColors=new List<Color>();
  Material guideFrom,guideTo,guideProblem,guideWhite,guideBlack;MaterialPropertyBlock pulseBlock;
  public int[] HighlightedProblemSquares=>observedProblem==null?Array.Empty<int>():Enumerable.Range(0,64).Where(s=>observedProblem[s]!=100&&observedProblem[s]!=problemTarget.Board[s]).ToArray();
  public void ShowPhysicalMismatch(int[] observed,ChessPosition target){if(observed==null||observed.Length!=64||target==null||!Playing)return;observedProblem=(int[])observed.Clone();problemTarget=target.Copy();DrawPhysicalGuides();}
  public void ClearPhysicalMismatch(){if(observedProblem==null)return;observedProblem=null;problemTarget=null;DrawPhysicalGuides();}
  public static float GuidancePulse(float time)=>.5f+.5f*Mathf.Sin(time*5.65f);
  void DrawPhysicalGuides(){
   foreach(var r in pulsePieces)if(r!=null)r.SetPropertyBlock(null);pulsePieces.Clear();pulseColors.Clear();pulseTiles.Clear();
   if(physicalGuides!=null){physicalGuides.gameObject.SetActive(false);Destroy(physicalGuides.gameObject);physicalGuides=null;}foreach(var mesh in guideMeshes)Destroy(mesh);guideMeshes.Clear();
   if(!Physical||!Playing||HomeScreen)return;
   physicalGuides=Node("Required physical moves");
   if(guideFrom==null){guideFrom=Mat(new Color(1,.6f,.05f),true);guideTo=Mat(new Color(.05f,1,.6f),true);guideProblem=Mat(new Color(1,.04f,.08f),true);foreach(var m in new[]{guideFrom,guideTo,guideProblem})m.SetFloat("_Square",1);guideWhite=new Material(ghostWhite);guideBlack=new Material(ghostBlack);materials.Add(guideWhite);materials.Add(guideBlack);}
   var marked=new HashSet<int>();
   void Tile(int square,bool destination,bool problem){if(square<0||square>63||!marked.Add(square))return;var tile=Shape(physicalGuides,"Pulse square "+ChessMove.Square(square),PrimitiveType.Quad,SquareLocal(square)+Vector3.up*.004f,new Vector3(Size/8*.92f,Size/8*.92f,1),problem?guideProblem:destination?guideTo:guideFrom);tile.localRotation=Quaternion.Euler(90,0,0);pulseTiles.Add(tile.GetComponent<Renderer>());}
   void Piece(Transform piece){if(piece==null)return;foreach(var renderer in piece.GetComponentsInChildren<Renderer>()){if(pulsePieces.Contains(renderer))continue;pulsePieces.Add(renderer);pulseColors.Add(renderer.sharedMaterial.color);}}
   void Source(int square,int piece){if(piece==0||Math.Abs(piece)>6)return;if(DisplayedPosition.Board[square]==piece&&figures[square]!=null){Piece(figures[square]);return;}var ghost=ChessPieceModel.Build(physicalGuides,piece,piece>0?guideWhite:guideBlack,piece>0?guideWhite:guideBlack,guideMeshes);ghost.name="Move from "+ChessMove.Square(square);ghost.localPosition=SquareLocal(square);ghost.localScale=PieceScale(piece);if(HumanSide<0)ghost.Rotate(0,180,0);Piece(ghost);}
   void Pair(int from,int to,int piece,bool problem){Tile(from,false,problem);Tile(to,true,problem);Source(from,piece);Piece(figures[to]);}
   if(observedProblem!=null){
    var used=new HashSet<int>();foreach(int to in HighlightedProblemSquares){Tile(to,observedProblem[to]!=99,observedProblem[to]!=99);Piece(figures[to]);int piece=problemTarget.Board[to];if(piece==0||observedProblem[to]==99)continue;var sources=Enumerable.Range(0,64).Where(s=>s!=to&&observedProblem[s]==piece&&problemTarget.Board[s]!=piece).ToArray();if(sources.Length==1){Pair(sources[0],to,piece,true);used.Add(sources[0]);}}
    foreach(int square in HighlightedProblemSquares)if(problemTarget.Board[square]==0&&observedProblem[square]!=99&&observedProblem[square]!=100&&!used.Contains(square)){Tile(square,false,true);Source(square,observedProblem[square]);}
   }else if(PendingAi.HasValue){
    var move=PendingAi.Value;int piece=Match.Position.Board[move.From];Pair(move.From,move.To,piece,false);
    if(Math.Abs(piece)==6&&Math.Abs(move.To-move.From)==2){int from=move.To>move.From?move.From+3:move.From-4,to=move.To>move.From?move.From+1:move.From-1;Pair(from,to,Match.Position.Board[from],false);}
    if(move.EnPassant){int victim=move.To-Match.Position.Side*8;Tile(victim,false,true);Source(victim,Match.Position.Board[victim]);var label=Label(physicalGuides,"REMOVE",SquareLocal(victim)+Vector3.up*.09f,.0018f);label.gameObject.AddComponent<TableFacingPanel>();}
   }
   UpdatePhysicalGuides();
  }
  void UpdatePhysicalGuides(){
   if(physicalGuides==null)return;if(pulseBlock==null)pulseBlock=new MaterialPropertyBlock();bool visible=Physical&&Aligned&&Playing&&!HomeScreen&&!relocating&&!SessionMenuOpen;physicalGuides.gameObject.SetActive(visible);float pulse=GuidancePulse(Time.unscaledTime);
   foreach(var tile in pulseTiles)if(tile!=null){pulseBlock.Clear();pulseBlock.SetFloat("_Strength",.2f+pulse*.65f);tile.SetPropertyBlock(pulseBlock);}
   for(int i=0;i<pulsePieces.Count;i++){var renderer=pulsePieces[i];if(renderer==null)continue;if(!visible){renderer.SetPropertyBlock(null);continue;}var color=pulseColors[i]*(1+pulse*.65f);color.a=.2f+pulse*.5f;pulseBlock.Clear();pulseBlock.SetColor("_BaseColor",color);renderer.SetPropertyBlock(pulseBlock);}
  }
 }
}
