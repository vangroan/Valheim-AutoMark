using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;

namespace VanGroan.Valheim
{
    [BepInPlugin("com.vangroan.valheim.auto-mark", "AutoMark Minimap", "1.0.0")]
    // [BepInDependency("com.bepinex.plugin.somedependency", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("valheim.exe")]
    public class AutoMark : BaseUnityPlugin
    {
        private float m_timer = 0f;
        private float m_queryInterval = 5f;
        private readonly HashSet<int> m_seen = new HashSet<int>();

        /// <summary>
        ///     Retrieve list of pins from the <c>Minimap</c> singleton.
        /// </summary>
        /// <remarks>
        ///     Reference retrieved is private to <c>Minimap</c>, so treat it with respect.
        ///     Don't mutate, add or remove elements.
        /// </remarks>
        private List<Minimap.PinData> m_pins
        {
            get
            {
                var field = typeof(Minimap).GetField("m_pins", BindingFlags.Instance | BindingFlags.NonPublic);
                return field.GetValue(Minimap.instance) as List<Minimap.PinData>;
            }
        }

        #region Unity Lifecycle
        void Awake()
        {
            UnityEngine.Debug.Log("Hello from AutoMark!");

            var map = Minimap.instance;
        }

        void Update()
        {
            m_timer += UnityEngine.Time.deltaTime;
            if (m_timer >= m_queryInterval)
            {
                m_timer -= m_queryInterval;

                // Mod will be loaded on main menu, but
                // minimap instance will not be ready.
                if (Minimap.instance)
                {

                }
            }
        }
        #endregion

        void QueryWorld()
        {
            // UnityEngine.RaycastHit hit;
        }

        void ClearTest()
        {
            if (m_pins != null)
            {
                foreach (var pinData in m_pins)
                {
                    if (pinData.m_name == "Custom Bed")
                        Minimap.instance.RemovePin(pinData);
                }
            }
        }

        Bed[] FindBeds()
        {
            return UnityEngine.GameObject.FindObjectsOfType<Bed>();
        }

        void PinBeds()
        {
            foreach (var bed in FindBeds())
            {
                if (m_seen.Contains(bed.GetInstanceID())) continue;

                // Bed must be in range to be marked.
                var delta = Player.m_localPlayer.transform.position - bed.transform.position;
                var distance = delta.magnitude;
                if (distance <= Minimap.instance.m_exploreRadius)
                {
                    // Minimap.instance.DiscoverLocation(bed.transform.position, Minimap.PinType.Icon1, "Custom Bed");
                    // If private field cannot be retrieved, don't add pin.
                    if (m_pins != null && !HaveSimilarPin(bed.transform.position, Minimap.PinType.Icon1, "Custom Bed", false))
                    {
                        Minimap.instance.AddPin(bed.transform.position, Minimap.PinType.Icon1, "Custom Bed", false, false);
                        m_seen.Add(bed.GetInstanceID());
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if a similar pin already exists.
        /// </summary>
        /// <remarks>
        ///     Function exists on <c>Minimap</c> but is private.
        /// </remarks>
        bool HaveSimilarPin(UnityEngine.Vector3 pos, Minimap.PinType type, string name, bool save)
        {
            foreach (var pinData in m_pins)
            {
                if (pinData.m_name == name && pinData.m_type == type && pinData.m_save == save && Utils.DistanceXZ(pos, pinData.m_pos) < 1f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
