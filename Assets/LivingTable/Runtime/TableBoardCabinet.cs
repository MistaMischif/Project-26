using UnityEngine;
namespace LivingTable
{
 // Follows the existing board dimensions and visibility, including physical-mode calibration.
 public sealed class TableBoardCabinet:MonoBehaviour
 {
  Vector3 dimensions;GameObject decoration;
  void OnEnable(){Refresh();}
  void LateUpdate(){if(transform.localScale!=dimensions)Refresh();}
  void Refresh(){dimensions=transform.localScale;if(dimensions.x<=0||dimensions.y<=0||dimensions.z<=0)return;GetComponent<Renderer>().enabled=false;if(decoration!=null){decoration.SetActive(false);Destroy(decoration);}decoration=new GameObject("Candy cabinet treatment");decoration.transform.SetParent(transform,false);decoration.transform.localPosition=new Vector3(0,.5f,0);decoration.transform.localScale=new Vector3(1/dimensions.x,1/dimensions.y,1/dimensions.z);decoration.AddComponent<TableCabinetVisual>().Board(dimensions.x,dimensions.z,true);}
 }
}
