using System;

namespace Ratcow.Debugging.Server
{
    public class Ref
    {
        public virtual object RawValue { get; }
    }

    public sealed class Ref<T> : Ref
    {
        private Func<T> getter;
        private Action<T> setter;
        public Ref(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
        public T Value
        {
            get { return getter(); }
            set { setter(value); }
        }

        public override object RawValue { get { return Value; } }
    }
}
