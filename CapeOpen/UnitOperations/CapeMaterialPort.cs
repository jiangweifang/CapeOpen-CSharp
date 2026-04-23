using System;

namespace CapeOpen
{
    /// <summary>
    /// A <see cref="UnitPort"/> specialization for material ports that provides
    /// direct access to the connected material through the <see cref="Material"/>
    /// property as a <see cref="CapeMaterialObject"/>.
    /// </summary>
    /// <remarks>
    /// This class is fully backward-compatible with the base <see cref="UnitPort"/> class.
    /// The simulator still sees a standard <see cref="ICapeUnitPort"/>. The
    /// <see cref="Material"/> property is a convenience for Calculator implementations.
    /// </remarks>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public class CapeMaterialPort : UnitPort
    {
        /// <summary>
        /// Creates a material port with the given name and direction.
        /// </summary>
        /// <param name="name">Name of the port.</param>
        /// <param name="description">Description of the port.</param>
        /// <param name="direction">Direction (inlet, outlet, or both).</param>
        public CapeMaterialPort(string name, string description, CapePortDirection direction)
            : base(name, description, direction, CapePortType.CAPE_MATERIAL)
        {
        }

        /// <summary>
        /// Gets the connected material as a <see cref="CapeMaterialObject"/> convenience wrapper.
        /// Returns null if nothing is connected.
        /// </summary>
        [System.ComponentModel.BrowsableAttribute(false)]
        public CapeMaterialObject Material
        {
            get
            {
                object obj = this.connectedObject;
                if (obj == null) return null;
                return new CapeMaterialObject(obj);
            }
        }

        /// <summary>
        /// Gets a duplicated <see cref="CapeMaterialObject"/> from the connected material.
        /// For input ports, always work with a duplicate to avoid modifying the original.
        /// Returns null if nothing is connected.
        /// </summary>
        [System.ComponentModel.BrowsableAttribute(false)]
        public CapeMaterialObject DuplicateMaterial()
        {
            CapeMaterialObject mat = Material;
            return mat?.Duplicate();
        }

        /// <summary>
        /// Returns true if this port has a connected material object.
        /// </summary>
        public bool IsConnected => this.connectedObject != null;
    }
}
