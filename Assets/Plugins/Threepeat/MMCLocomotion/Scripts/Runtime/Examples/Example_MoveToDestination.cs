using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class Example_MoveToDestination : SimpleKeyProcessor
    {
        public Transform target;

        public override void HandleKeyDown()
        {
            if (character.AI.moveTo_active)
            {
                character.AI.CancelMoveToDestination();
            }
            else
            {
                character.AI.MoveToDestination(target, OnComplete);
            }
        }

        protected void OnComplete()
        {
            Debug.Log("Arrived at destination!");
        }
    }
}