﻿using NetUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    /// <summary>
    /// This class is a handy wrapper for automatic event wrapping.
    /// You shouldn't use this class to dynamically add/remove handlers frequently
    /// as this class relys on <see cref="Reflection"/> which may negatively affect
    /// the performance of your application.
    /// </summary>
    /// <typeparam name="TSource">The source of the events</typeparam>
    public class EventManager<TSource> where TSource : notnull
    {
        /// <summary>
        /// The source of the events
        /// </summary>
        public TSource Source { get; }

        private readonly Dictionary<object, List<(EventInfo, Delegate)>> _handlers =
            new Dictionary<object, List<(EventInfo, Delegate)>>();

        /// <summary>
        /// Creates an instance of <see cref="EventManager{TSource}"/> with the instance of the source provided.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <param name="source">The source of the events</param>
        public EventManager(TSource source)
            => Source = Ensure.NotNull(source, nameof(source));


        /// <summary>
        /// Adds all the handlers to the methods that have <see cref="HandlesAttribute"/>
        /// </summary>
        /// <param name="target">The instance of object that will listen to the events</param>
        /// <param name="flags">The flags used for <see cref="Reflection"/> to search the methods that will listen to the events</param>
        public virtual void AddHandlers(object target, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            var targetType = target.GetType();
            var metadatas = targetType.GetMethods(flags)
                .Select(method => new
                {
                    Method = method,
                    Events = method.GetCustomAttributes<HandlesAttribute>().Select(attribute => attribute.EventInfo).ToArray()
                })
                .Where(metadata => metadata.Events.Length > 0);

            foreach (var metadata in metadatas)
            {
                var method = metadata.Method;

                foreach (var eventInfo in metadata.Events)
                {
                    var handler = method.CreateDelegate(eventInfo.EventHandlerType, target);
                    eventInfo.AddEventHandler(Source, handler);

                    if (_handlers.TryGetValue(target, out var list))
                        list.Add((eventInfo, handler));
                    else
                        _handlers.Add(target, new List<(EventInfo, Delegate)>() { (eventInfo, handler) });
                }
            }
        }

        /// <summary>
        /// Removes all handlers to the methods with <see cref="HandlesAttribute"/>
        /// </summary>
        /// <param name="target">The instance of object that is currently listening to the events</param>
        public virtual void RemoveHandlers(object target)
        {
            if (!_handlers.TryGetValue(target, out var list)) return;

            foreach (var (eventInfo, handler) in list)
                eventInfo.RemoveEventHandler(target, handler);
        }
    }
}
