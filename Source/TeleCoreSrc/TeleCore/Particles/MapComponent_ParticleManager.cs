using System;
using System.Collections.Generic;
using Verse;
using TeleCore.Defs;

namespace TeleCore.Particles
{
    public class MapComponent_ParticleManager : MapComponent
    {
        private List<ParticleSystem> activeSystems = new List<ParticleSystem>();
        private Dictionary<Type, ParticleSystem> systemsByType = new Dictionary<Type, ParticleSystem>();

        public MapComponent_ParticleManager(Map map) : base(map)
        {
        }

        public T GetSystem<T>() where T : ParticleSystem
        {
            if (systemsByType.TryGetValue(typeof(T), out var system))
            {
                return (T)system;
            }

            T newSystem = (T)Activator.CreateInstance(typeof(T));
            activeSystems.Add(newSystem);
            systemsByType[typeof(T)] = newSystem;
            return newSystem;
        }

        public ParticleSystem GetSystem(Type type)
        {
            if (systemsByType.TryGetValue(type, out var system))
            {
                return system;
            }

            if (typeof(ParticleSystem).IsAssignableFrom(type))
            {
                ParticleSystem newSystem = (ParticleSystem)Activator.CreateInstance(type);
                activeSystems.Add(newSystem);
                systemsByType[type] = newSystem;
                return newSystem;
            }
            return null;
        }

        public override void MapComponentTick()
        {
            for (int i = 0; i < activeSystems.Count; i++)
            {
                activeSystems[i].Tick();
            }
        }

        public override void MapComponentUpdate()
        {
            for (int i = 0; i < activeSystems.Count; i++)
            {
                activeSystems[i].Draw();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // We need a way to save which systems are active and their data
            // For now, let's just save all registered systems that have data
            Scribe_Collections.Look(ref activeSystems, "activeSystems", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (activeSystems == null) activeSystems = new List<ParticleSystem>();
                foreach (var system in activeSystems)
                {
                    systemsByType[system.GetType()] = system;
                }
            }
        }
    }
}
