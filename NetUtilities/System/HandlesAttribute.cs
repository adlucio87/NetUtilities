﻿using NetUtilities;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HandlesAttribute : Attribute
    {
        /// <summary>
        /// The event metadata.
        /// </summary>
        public EventInfo EventInfo { get; }

        /// <summary>
        /// Creates an attribute that is used for metadata to gather the needed information for <see cref="EventManager{TSource}"/>
        /// </summary>
        /// <exception cref="EventNotFoundException">Thrown when the event can't be found.</exception>
        /// <param name="eventSourceType">The type of the source of events.</param>
        /// <param name="eventName">The name of the event, recommended to use nameof() operator to prevent <see cref="EventNotFoundException"/>.</param>
        /// <param name="flags">The flags used to search the event.</param>
        /// <param name="methodName">The name of the method. This is gathered from <see cref="CallerMemberNameAttribute"/> so any value you pass here is ommited.</param>
        public HandlesAttribute(Type eventSourceType, string eventName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance, [CallerMemberName]string methodName = "")
        {
            Ensure.NotNull(eventSourceType, nameof(eventSourceType));
            EventInfo = eventSourceType.GetEvent(eventName, flags);

            if (EventInfo is null)
                throw new EventNotFoundException(eventName, eventSourceType.Name, flags);
        }
    }
}
