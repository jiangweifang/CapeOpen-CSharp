using System;
using System.Collections.Generic;
using System.Linq;

namespace CapeOpen
{
    /// <summary>
    /// A <see cref="UnitPort"/> specialization for energy ports that provides
    /// convenience properties to access the Work, TemperatureLow, and
    /// TemperatureHigh parameters of the connected energy stream.
    /// </summary>
    /// <remarks>
    /// Energy streams in CAPE-OPEN are represented as <see cref="ICapeCollection"/>
    /// of <see cref="ICapeParameter"/> objects. This port wraps that collection
    /// and exposes the standard energy stream parameters as strongly-typed properties.
    /// </remarks>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public class CapeEnergyPort : UnitPort
    {
        /// <summary>
        /// Creates an energy port with the given name and direction.
        /// </summary>
        /// <param name="name">Name of the port.</param>
        /// <param name="description">Description of the port.</param>
        /// <param name="direction">Direction (inlet, outlet, or both).</param>
        public CapeEnergyPort(string name, string description, CapePortDirection direction)
            : base(name, description, direction, CapePortType.CAPE_ENERGY)
        {
        }

        /// <summary>
        /// Returns true if this port has a connected object.
        /// </summary>
        public bool IsConnected => this.connectedObject != null;

        /// <summary>
        /// Gets the connected object as an <see cref="ICapeCollection"/>.
        /// Returns null if nothing is connected.
        /// </summary>
        private ICapeCollection Collection
        {
            get
            {
                object obj = this.connectedObject;
                return obj as ICapeCollection;
            }
        }

        private ICapeParameter FindParameter(string name)
        {
            ICapeCollection col = Collection;
            if (col == null) return null;
            int count = col.Count();
            for (int i = 1; i <= count; i++)
            {
                object item = col.Item((object)i);
                if (item is ICapeParameter param)
                {
                    if (item is ICapeIdentification id &&
                        string.Equals(id.ComponentName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return param;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the Work parameter value of the connected energy stream.
        /// Throws if the parameter is not found.
        /// </summary>
        public double Work
        {
            get
            {
                ICapeParameter p = FindParameter("work");
                if (p != null) return Convert.ToDouble(p.value);
                return 0;
            }
            set
            {
                ICapeParameter p = FindParameter("work");
                if (p == null)
                    throw new CapeInvalidOperationException("Energy port does not contain a 'work' parameter.");
                p.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the TemperatureLow parameter value of the connected energy stream.
        /// Throws if the parameter is not found.
        /// </summary>
        public double TemperatureLow
        {
            get
            {
                ICapeParameter p = FindParameter("temperaturelow");
                if (p != null) return Convert.ToDouble(p.value);
                return 0;
            }
            set
            {
                ICapeParameter p = FindParameter("temperaturelow");
                if (p == null)
                    throw new CapeInvalidOperationException("Energy port does not contain a 'temperaturelow' parameter.");
                p.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the TemperatureHigh parameter value of the connected energy stream.
        /// Throws if the parameter is not found.
        /// </summary>
        public double TemperatureHigh
        {
            get
            {
                ICapeParameter p = FindParameter("temperaturehigh");
                if (p != null) return Convert.ToDouble(p.value);
                return 0;
            }
            set
            {
                ICapeParameter p = FindParameter("temperaturehigh");
                if (p == null)
                    throw new CapeInvalidOperationException("Energy port does not contain a 'temperaturehigh' parameter.");
                p.value = value;
            }
        }
    }
}
