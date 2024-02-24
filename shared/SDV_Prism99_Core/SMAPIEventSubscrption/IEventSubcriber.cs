using System;
using System.Collections.Generic;


namespace Prism99_Core.SMAPIEventSubscription
{
    internal interface IEventSubcriber:IEqualityComparer<IEventSubcriber>, IEquatable<IEventSubcriber>
    {
        public abstract void Hook();
        public abstract void UnHook();
        public abstract string GetDetails();
    }
}
