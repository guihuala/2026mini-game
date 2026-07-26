using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class MiniSceneManager : MonoBehaviour
{
   [SerializeField] private Transform oriPostion;
   [SerializeField] private Transform Player;

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

      resetCoroutine = null;
   }
}
