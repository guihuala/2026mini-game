using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class MiniSceneManager : MonoBehaviour
{
   [SerializeField] private Transform oriPostion;
   [SerializeField] private Transform Player;

   private PlayerKeyInventory playerInventory;
   private List<GameObject> allKeys = new List<GameObject>();
   private List<GameObject> allDoors = new List<GameObject>();

   private void Start()
   {
      playerInventory = Player.GetComponent<PlayerKeyInventory>();

      foreach (var key in FindObjectsOfType<Key>(true))
         allKeys.Add(key.gameObject);

      foreach (var door in FindObjectsOfType<Door>(true))
         allDoors.Add(door.gameObject);
   }

   Coroutine resetCoroutine;
   [Button("ResetPlayer")]
   public void ResetPlayerPos()
   {
      if (resetCoroutine == null)
      {
         resetCoroutine = StartCoroutine(ResetPlayer());
      }
   }

   IEnumerator ResetPlayer()
   {
      var playerMove = Player.GetComponent<PlayerMove>();
      if (playerMove != null)
         playerMove.DisableInput(0.3f);

      yield return null;

      var rb = Player.GetComponent<Rigidbody2D>();
      if (rb != null)
      {
         rb.velocity = Vector2.zero;
         yield return null;
         rb.position = oriPostion.position;
         yield return null;
         rb.velocity = Vector2.zero;
      }
      else
      {
         Player.position = oriPostion.position;
      }

      // 重置钥匙和门
      if (playerInventory != null)
         playerInventory.ClearKeys();

      foreach (var key in allKeys)
         key.SetActive(true);

      foreach (var door in allDoors)
         door.SetActive(true);

      resetCoroutine = null;
   }
}
