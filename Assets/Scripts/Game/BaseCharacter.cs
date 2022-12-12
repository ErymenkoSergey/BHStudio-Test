using Cinemachine;
using HBStudio.Test.Mechanics.NetWork;
using Mirror;
using UnityEngine;

namespace HBStudio.Test.Mechanics.Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    public abstract class BaseCharacter : NetworkBehaviour
    {
        [SerializeField] protected CharacterController _characterController;
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected SkinnedMeshRenderer _meshRenderer;

        protected CinemachineFreeLook _cinemachine;
        protected SceneObserver _sceneObserver;
    }
}