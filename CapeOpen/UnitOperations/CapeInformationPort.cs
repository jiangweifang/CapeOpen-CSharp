using System;
using System.Collections;
using System.Collections.Generic;

namespace CapeOpen
{
    /// <summary>
    /// A <see cref="UnitPort"/> specialization for information ports that provides
    /// indexed and named access to the parameters in the connected 
    /// <see cref="ICapeCollection"/>.
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public class CapeInformationPort : UnitPort, IEnumerable<ICapeParameter>
    {
        /// <summary>
        /// Creates an information port with the given name and direction.
        /// </summary>
        /// <param name="name">Name of the port.</param>
        /// <param name="description">Description of the port.</param>
        /// <param name="direction">Direction (inlet, outlet, or both).</param>
        public CapeInformationPort(string name, string description, CapePortDirection direction)
            : base(name, description, direction, CapePortType.CAPE_INFORMATION)
        {
        }

        /// <summary>
        /// Returns true if this port has a connected object.
        /// </summary>
        public bool IsConnected => this.connectedObject != null;

        /// <summary>
        /// Gets the connected object as an <see cref="ICapeCollection"/>.
        /// </summary>
        private ICapeCollection Collection
        {
            get
            {
                object obj = this.connectedObject;
                return obj as ICapeCollection;
            }
        }

        /// <summary>
        /// Gets a parameter by name from the connected collection.
        /// </summary>
        /// <param name="id">The name identifier of the parameter.</param>
        public ICapeParameter this[string id]
        {
            get
            {
                ICapeCollection col = Collection;
                if (col == null)
                    throw new CapeInvalidOperationException("No connected information stream.");
                return (ICapeParameter)col.Item((object)id);
            }
        }

        /// <summary>
        /// Gets a parameter by zero-based index from the connected collection.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        public ICapeParameter this[int index]
        {
            get
            {
                ICapeCollection col = Collection;
                if (col == null)
                    throw new CapeInvalidOperationException("No connected information stream.");
                // CAPE-OPEN collections are 1-based
                return (ICapeParameter)col.Item((object)(index + 1));
            }
        }

        /// <summary>
        /// Gets the number of parameters in the connected collection.
        /// </summary>
        public int Count
        {
            get
            {
                ICapeCollection col = Collection;
                if (col == null) return 0;
                return col.Count();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<ICapeParameter> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                yield return this[i];
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
