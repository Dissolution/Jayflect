
/*
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Jay.Reflection
{
    public static class TemporaryEventHandle
    {
        public sealed class NP : INotifyPropertyChanged
        {
            private int _id;

            public int Id
            {
                get { return _id; }
                set
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        static TemporaryEventHandle()
        {
            var np = new NP();
            Attach(ref np, thing => thing.PropertyChanged, 
        }

        public static IDisposable Attach<T, THandle>(ref T instance, Expression<Func<T, THandle>> selectEventHandlerName, THandle handler)
            where T : class
            where THandle : Delegate
        {
            if (instance is null) throw new ArgumentNullException(nameof(instance));
            var eventInfo = selectEventHandlerName.GetMember<EventInfo>();
            if (eventInfo is null)
            {
                var constEx = selectEventHandlerName.EnumerateExpressions()
                                                    .OfType<ConstantExpression>()
                                                    .FirstOrDefault();
                string? eventName = constEx?.Value?.ToString();
                if (string.IsNullOrEmpty(eventName))
                {
                    // We can't find it!
                    throw new ArgumentException("The given expression doesn't contain enough information to find an Event", nameof(selectEventHandlerName));
                }
                eventInfo = typeof(T).GetEvent(eventName, Reflect.InstanceFlags);
                if (eventInfo is null)
                {
                    // Can't find it
                    throw new ArgumentException($"{typeof(T).Name} does not have an instance event named '{eventName}'",
                                                nameof(selectEventHandlerName));
                }
            }

            //Cast the delegate to the correct type
            var castHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType!, handler.Target, handler.Method);
            //Add it to the event handler
            eventInfo.AddEventHandler(instance, castHandler);
            //Create our remove action
            void Remove() => eventInfo.RemoveEventHandler(instance, castHandler);
            //Return it in a dispose action
            return Disposable.Action(Remove);
        }
    }
}*/