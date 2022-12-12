using UnityEngine;

namespace HBStudio.Test.Interface
{
    public interface IMoveble
    {
        void Move(Controls controls, bool isOn);
        void Rotate(Vector2 vector);
        void Bounce();
    }
}