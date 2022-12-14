using HBStudio.Test.Mechanics.NetWork;
using Mirror;
using UnityEngine;

namespace HBStudio.Test.Mechanics.Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public abstract class BaseCharacter : NetworkBehaviour
    {
        [SerializeField] protected CharacterController _characterController;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected SkinnedMeshRenderer _meshRenderer;
        [SerializeField] protected Camera _camera;
        [SerializeField] protected Transform _cameraPosition;

        protected SceneObserver _sceneObserver;
    }
}