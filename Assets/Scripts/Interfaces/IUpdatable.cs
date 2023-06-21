using Core;

namespace Interfaces
{
    public interface IUpdatable : IActor
    {
        public void OnUpdate(float deltaTime);
    }
}