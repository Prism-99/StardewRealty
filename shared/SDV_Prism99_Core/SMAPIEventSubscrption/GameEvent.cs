
namespace Prism99_Core.SMAPIEventSubscription
{
    internal abstract class GameEvent : IEventSubcriber
    {
        public bool Equals(IEventSubcriber x, IEventSubcriber y)
        {
            return x.GetType().Name == y.GetType().Name;
        }

        public bool Equals(IEventSubcriber other)
        {
            return Equals(this, other);
        }

        public int GetHashCode(IEventSubcriber obj)
        {
            return obj.GetType().Name.GetHashCode();
        }

        public abstract void Hook();
        public abstract void UnHook();
        public abstract string GetDetails();
    }
}
