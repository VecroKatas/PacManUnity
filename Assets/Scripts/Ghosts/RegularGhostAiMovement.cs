using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Random = System.Random;

namespace Ghosts
{
    public enum GhostChaseType
    {
        Regular,
        Random,
        Related,
        Shy,
        Predictive,
        Patrol,
        Circling,
        Follower
    }
    public class RegularGhostAiMovement : GhostAiMovement
    {
        [Header("Ghost chase settings")]

        public GhostChaseType chaseType;
        public GameObject relatedGhost;
        public Vector3 center;

        private Queue<Vector2> path;
        private Vector2 currentPathTarget;

        private PlayerController playerController;

        private bool _startedPath = false;

        new void Start()
        {
            path = new Queue<Vector2>();

            path.Enqueue(chaseModeTarget.transform.position);

            playerController = chaseModeTarget.GetComponent<PlayerController>();

            if (chaseType == GhostChaseType.Patrol)
                currentPathTarget = GetRandomGum().transform.position;

            base.Start();
        }

        private new void FixedUpdate()
        {
            path.Enqueue(chaseModeTarget.transform.position);

            base.FixedUpdate();
        }

        protected override void Chase()
        {
            switch (chaseType)
            {
                case GhostChaseType.Regular:
                    RegularChase();
                    break;
                case GhostChaseType.Random:
                    RandomChase();
                    break;
                case GhostChaseType.Related:
                    RelatedChase();
                    break;
                case GhostChaseType.Shy:
                    ShyChase();
                    break;
                case GhostChaseType.Predictive:
                    PredictiveChase();
                    break;
                case GhostChaseType.Patrol:
                    PatrolChase();
                    break;
                case GhostChaseType.Circling:
                    CirclingChase();
                    break;
                case GhostChaseType.Follower:
                    FollowerChase();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RegularChase()
        {
            ChaseTarget(chaseModeTarget, runSpeed);
        }

        private void RandomChase()
        {
            bool centered = TryMoveToTileCenter(NextTileDestination, runSpeed);

            if (!centered)
                return;

            var possibleDirections = GetPossibleDirections();

            NextTileDestination = (Vector2)transform.position + possibleDirections[new System.Random().Next(possibleDirections.Count)];

            UpdateAnimation();
        }

        private void RelatedChase()
        {
            var targetPos = chaseModeTarget.transform.position;
            Vector2 directionVector = targetPos - relatedGhost.transform.position;
            ChaseTarget((Vector2)targetPos + directionVector / 4, runSpeed);
        }

        private void ShyChase()
        {
            ChaseTarget(
                Vector2.Distance(transform.position, chaseModeTarget.transform.position) >= 8f
                    ? chaseModeTarget
                    : scatterModeTarget, runSpeed);
        }

        private void PredictiveChase()
        {
            Vector2 playerDirection = (playerController.NextDestination - (Vector2)playerController.transform.position).normalized;
            Vector2 predictedPosition = (Vector2)chaseModeTarget.transform.position + (playerDirection * 3);

            ChaseTarget(predictedPosition, runSpeed);
        }

        private void FollowerChase()
        {
            if (Vector2.Distance(currentPathTarget, transform.position) < 0.5f)
            {
                currentPathTarget = path.Dequeue();
                _startedPath = true;
            }

            if (!_startedPath)
            {
                ChaseTarget(currentPathTarget, runSpeed);
            }
            else
            {
                MoveGhost(transform.position, runSpeed);

                NextTileDestination = currentPathTarget;
            }

            UpdateAnimation();
        }

        protected override void LeavingHouse()
        {
            _startedPath = false;
            path.Clear();
            base.LeavingHouse();
        }

        private void PatrolChase()
        {
            if (Vector2.Distance(currentPathTarget, transform.position) < 0.5f)
            {
                var gum = GetRandomGum();

                if (!gum)
                {
                    currentPathTarget = chaseModeTarget.transform.position;
                }
                else
                {
                    currentPathTarget = gum.transform.position;
                }
            }

            ChaseTarget(currentPathTarget, runSpeed);
        }

        private GameObject GetRandomGum()
        {
            var list = GameHandler.GameHandler.Instance.pacGumList;

            if (list.Count < 20) return null;

            return list[new Random().Next(list.Count)];
        }

        private void CirclingChase()
        {
            var playerCenterMiddle = (chaseModeTarget.transform.position + center) / 2f;
            var centerVector = (playerCenterMiddle - transform.position).normalized;
            var ghostVector = (chaseModeTarget.transform.position - transform.position).normalized;
            var vector = centerVector + ghostVector;

            ChaseTarget(transform.position + vector, runSpeed);
        }
    }
}
