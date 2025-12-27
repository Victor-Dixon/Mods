using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace CitiesRegional.Bootstrap
{
    /// <summary>
    /// Robust bootstrap that waits for the DefaultGameObjectInjectionWorld,
    /// then registers SystemDiscoverySystem exactly once.
    /// Prefer this over guessing a Harmony hook point.
    /// </summary>
    internal sealed class SystemDiscoveryBootstrap
    {
        private readonly MonoBehaviour _host;
        private bool _started;

        public SystemDiscoveryBootstrap(MonoBehaviour host)
        {
            _host = host;
        }

        public void Start()
        {
            if (_started) return;
            _started = true;
            _host.StartCoroutine(WaitForWorldAndInstall());
        }

        private IEnumerator WaitForWorldAndInstall()
        {
            const float timeoutSeconds = 30f;
            float start = Time.realtimeSinceStartup;

            while (World.DefaultGameObjectInjectionWorld == null)
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds)
                {
                    CitiesRegional.Logging.LogWarn("[Discovery] Timeout waiting for DefaultGameObjectInjectionWorld.");
                    yield break;
                }
                yield return null;
            }

            try
            {
                var world = World.DefaultGameObjectInjectionWorld;
                CitiesRegional.Logging.LogInfo($"[Discovery] World ready: {world.Name}");

                // Create + enable system
                var sys = world.GetOrCreateSystemManaged<CitiesRegional.Systems.SystemDiscoverySystem>();
                sys.Enabled = true;

                CitiesRegional.Logging.LogInfo("[Discovery] SystemDiscoverySystem installed + enabled.");
            }
            catch (Exception ex)
            {
                CitiesRegional.Logging.LogError($"[Discovery] Failed to install discovery system: {ex}");
            }
        }
    }
}

