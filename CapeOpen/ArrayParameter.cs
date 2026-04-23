using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapeOpen
{
    /// <summary>
    /// Array-Valued parameter for use in the CAPE-OPEN parameter collection.
    /// </summary>
    /// <remarks>
    /// Array-Valued parameter for use in the CAPE-OPEN parameter collection.
    /// The array contains elements that conform to a specified element parameter type.
    /// </remarks>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.InteropServices.Guid("3A5F7B2E-9C14-4D8A-B6E1-7F2A3D4C5E6B")]
    [System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.None)]
    public class ArrayParameter : CapeParameter,
        ICapeParameter,
        ICapeParameterSpec,
        ICapeArrayParameterSpec,
        System.ComponentModel.INotifyPropertyChanged
    {
        private object[] m_value;
        private object[] m_DefaultValue;
        private int[] m_size;
        private object[] m_itemsSpecifications;

        /// <summary>
        /// Constructor for the array-valued parameter.
        /// </summary>
        /// <param name="name">Sets the ComponentName of the parameter.</param>
        /// <param name="description">Sets the ComponentDescription of the parameter.</param>
        /// <param name="value">Sets the initial value of the parameter (an array of values).</param>
        /// <param name="defaultValue">Sets the default value of the parameter.</param>
        /// <param name="size">An integer array containing the size of each dimension.</param>
        /// <param name="itemsSpecifications">An array of parameter specifications for each element.</param>
        /// <param name="mode">Sets the CapeParamMode mode of the parameter.</param>
        public ArrayParameter(String name, String description, object[] value, object[] defaultValue, int[] size, object[] itemsSpecifications, CapeParamMode mode)
            : base(name, description, mode)
        {
            m_value = value;
            m_DefaultValue = defaultValue;
            m_size = size;
            m_itemsSpecifications = itemsSpecifications;
            this.Mode = mode;
            m_ValStatus = CapeValidationStatus.CAPE_VALID;
        }

        /// <summary>
        /// Constructor for a simple one-dimensional array parameter.
        /// </summary>
        /// <param name="name">Sets the ComponentName of the parameter.</param>
        /// <param name="description">Sets the ComponentDescription of the parameter.</param>
        /// <param name="value">Sets the initial value of the parameter.</param>
        /// <param name="defaultValue">Sets the default value of the parameter.</param>
        /// <param name="mode">Sets the CapeParamMode mode of the parameter.</param>
        public ArrayParameter(String name, String description, object[] value, object[] defaultValue, CapeParamMode mode)
            : this(name, description, value, defaultValue, new int[] { value.Length }, null, mode)
        {
        }

        /// <summary>
        /// Gets and sets the value for this Parameter.
        /// </summary>
        [System.ComponentModel.BrowsableAttribute(false)]
        override public Object value
        {
            get { return m_value; }
            set
            {
                ParameterValueChangedEventArgs args = new ParameterValueChangedEventArgs(this.ComponentName, m_value, value);
                m_value = (object[])value;
                OnParameterValueChanged(args);
            }
        }

        /// <summary>
        /// Gets and sets the array value for this Parameter.
        /// </summary>
        [System.ComponentModel.CategoryAttribute("ICapeParameter")]
        public object[] Value
        {
            get { return m_value; }
            set
            {
                ParameterValueChangedEventArgs args = new ParameterValueChangedEventArgs(this.ComponentName, m_value, value);
                m_value = value;
                OnParameterValueChanged(args);
            }
        }

        /// <inheritdoc/>
        override public Object Clone()
        {
            return new ArrayParameter(this.ComponentName, this.ComponentDescription, (object[])m_value.Clone(), (object[])m_DefaultValue.Clone(), (int[])m_size.Clone(), m_itemsSpecifications, this.Mode);
        }

        /// <inheritdoc/>
        public override bool Validate(ref String message)
        {
            string[] messages = null;
            object result = ((ICapeArrayParameterSpec)this).Validate(m_value, ref messages);
            bool valid = result is bool && (bool)result;
            if (valid)
            {
                message = "Value is valid.";
                m_ValStatus = CapeValidationStatus.CAPE_VALID;
            }
            else
            {
                message = messages != null && messages.Length > 0 ? String.Join("; ", messages) : "Value is invalid.";
                m_ValStatus = CapeValidationStatus.CAPE_INVALID;
            }
            ParameterValidatedEventArgs args = new ParameterValidatedEventArgs(this.ComponentName, message, m_ValStatus, m_ValStatus);
            OnParameterValidated(args);
            return valid;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            ParameterResetEventArgs args = new ParameterResetEventArgs(this.ComponentName);
            m_value = (object[])m_DefaultValue.Clone();
            OnParameterReset(args);
        }

        /// <inheritdoc/>
        [System.ComponentModel.CategoryAttribute("ICapeParameterSpec")]
        public override CapeParamType Type
        {
            get { return CapeParamType.CAPE_ARRAY; }
        }

        /// <inheritdoc/>
        [System.ComponentModel.Category("Parameter Specification")]
        int ICapeArrayParameterSpec.NumDimensions
        {
            get { return m_size.Length; }
        }

        /// <inheritdoc/>
        [System.ComponentModel.Category("Parameter Specification")]
        int[] ICapeArrayParameterSpec.Size
        {
            get { return m_size; }
        }

        /// <inheritdoc/>
        [System.ComponentModel.BrowsableAttribute(false)]
        object[] ICapeArrayParameterSpec.ItemsSpecifications
        {
            get { return m_itemsSpecifications; }
        }

        /// <inheritdoc/>
        object ICapeArrayParameterSpec.Validate(object inputArray, ref string[] messages)
        {
            if (!(inputArray is object[] arr))
            {
                messages = new string[] { "Value is not an array." };
                return false;
            }
            if (m_itemsSpecifications == null || m_itemsSpecifications.Length == 0)
            {
                messages = new string[] { "Value is valid (no element specifications defined)." };
                return true;
            }
            var msgList = new System.Collections.Generic.List<string>();
            bool allValid = true;
            for (int i = 0; i < arr.Length; i++)
            {
                // Use per-element spec if available, otherwise use the first spec for all elements.
                object spec = i < m_itemsSpecifications.Length ? m_itemsSpecifications[i] : m_itemsSpecifications[0];
                if (spec == null)
                {
                    msgList.Add("Element [" + i + "]: valid (no spec).");
                    continue;
                }
                string elemMsg = ValidateElement(arr[i], spec, i);
                if (elemMsg != null)
                {
                    allValid = false;
                    msgList.Add(elemMsg);
                }
                else
                {
                    msgList.Add("Element [" + i + "]: valid.");
                }
            }
            messages = msgList.ToArray();
            return allValid;
        }

        /// <summary>
        /// Validates a single element against its specification.
        /// </summary>
        private static string ValidateElement(object element, object spec, int index)
        {
            string prefix = "Element [" + index + "]: ";
            if (spec is ICapeRealParameterSpec realSpec)
            {
                if (!(element is double dVal))
                    return prefix + "expected double, got " + (element?.GetType().Name ?? "null") + ".";
                string msg = null;
                if (!realSpec.SIValidate(dVal, ref msg))
                    return prefix + (msg ?? "value out of range.");
                return null;
            }
            if (spec is ICapeIntegerParameterSpec intSpec)
            {
                if (!(element is int iVal))
                    return prefix + "expected int, got " + (element?.GetType().Name ?? "null") + ".";
                string msg = null;
                if (!intSpec.Validate(iVal, ref msg))
                    return prefix + (msg ?? "value out of range.");
                return null;
            }
            if (spec is ICapeBooleanParameterSpec)
            {
                if (!(element is bool))
                    return prefix + "expected bool, got " + (element?.GetType().Name ?? "null") + ".";
                return null;
            }
            if (spec is ICapeOptionParameterSpec optSpec)
            {
                if (!(element is string sVal))
                    return prefix + "expected string, got " + (element?.GetType().Name ?? "null") + ".";
                string msg = null;
                if (!optSpec.Validate(sVal, ref msg))
                    return prefix + (msg ?? "value not in option list.");
                return null;
            }
            if (spec is ICapeArrayParameterSpec arrSpec)
            {
                string[] msgs = null;
                object result = arrSpec.Validate(element, ref msgs);
                if (result is bool b && !b)
                    return prefix + "nested array invalid: " + (msgs != null ? String.Join("; ", msgs) : "unknown error.");
                return null;
            }
            return null;
        }

        /// <summary>
        /// Gets and sets the default value of the parameter.
        /// </summary>
        [System.ComponentModel.CategoryAttribute("ICapeArrayParameterSpec")]
        public object[] DefaultValue
        {
            get { return m_DefaultValue; }
            set
            {
                ParameterDefaultValueChangedEventArgs args = new ParameterDefaultValueChangedEventArgs(this.ComponentName, m_DefaultValue, value);
                m_DefaultValue = value;
                OnParameterDefaultValueChanged(args);
            }
        }
    }


    /// <summary>
    /// Wrapper for an array-valued parameter for use in a CAPE-OPEN <see cref="ParameterCollection">parameter collection</see>.
    /// </summary>
    /// <remarks>
    /// Wraps a CAPE-OPEN array-valued parameter for use in a CAPE-OPEN <see cref="ParameterCollection">parameter collection</see>.
    /// </remarks>
    [Serializable]
    [System.Runtime.InteropServices.ComSourceInterfaces(typeof(IRealParameterSpecEvents))]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.InteropServices.Guid("277E2E39-70E7-4FBA-89C9-2A19B9D5E576")]//ICapeThermoMaterialObject_IID)
    [System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.None)]
    class ArrayParameterWrapper : CapeParameter,
        ICapeParameter,
        ICapeParameterSpec,
        ICapeArrayParameterSpec
    {

        [NonSerialized]
        private ICapeParameter m_parameter = null;

        /// <summary>
        /// Creates a new instance of a wrapper class for COM-based array-valued parameter class. 
        /// </summary>
        /// <remarks>
        /// The COM-based array parameter is wrapped and exposed to .NET-based PME and PMCs.
        /// </remarks>
        /// <param name = "parameter">The COM-based array parameter to be wrapped.</param>
        public ArrayParameterWrapper(ICapeParameter parameter)
            : base(((ICapeIdentification)parameter).ComponentName, ((ICapeIdentification)parameter).ComponentDescription, parameter.Mode)
        {
            m_parameter = parameter;
        }

        // ICloneable
        /// <summary>
        /// Creates a copy of the parameter. Both copies refer to the same COM-based array parameter.
        /// </summary>
        /// <remarks><para>The clone method is used to create a copy of the parameter. Both the original object and 
        /// the clone wrap the same instance of the wrapped parameter.</para>
        /// </remarks>
        /// <returns>A copy of the current parameter.</returns>
        override public Object Clone()
        {
            return new ArrayParameterWrapper(m_parameter);
        }

        /// <summary>
        /// Validates the current value of the parameter against the specification of the parameter. 
        /// </summary>        
        /// <remarks>
        /// The wrapped parameter validates iteself against its internal valication criteria.
        /// </remarks>
        /// <returns>
        /// True if the parameter is valid, false if not valid.
        /// </returns>
        /// <param name = "message">Reference to a string that will conain a message regarding the validation of the parameter.</param>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed, for example, an unrecognised Compound identifier or UNDEFINED for the props argument.</exception>
        public override bool Validate(ref String message)
        {
            ParameterValidatedEventArgs args;
            CapeValidationStatus valStatus = m_parameter.ValStatus;
            bool retval = m_parameter.Validate(message);
            args = new ParameterValidatedEventArgs(this.ComponentName, message, ValStatus, m_parameter.ValStatus);
            OnParameterValidated(args);
            this.NotifyPropertyChanged("ValStatus");
            return retval;
        }

        /// <summary>
        /// Sets the value of the parameter to its default value.
        /// </summary>
        /// <remarks>
        ///  This method sets the parameter's value to the default value.
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        public override void Reset()
        {
            ParameterResetEventArgs args = new ParameterResetEventArgs(this.ComponentName);
            m_parameter.Reset();
            this.NotifyPropertyChanged("Value");
            OnParameterReset(args);
        }

        // ICapeParameterSpec
        // ICapeParameterSpec
        /// <summary>
        /// Gets the type of the parameter. 
        /// </summary>
        /// <remarks>
        /// Gets the <see cref = "CapeParamType"/> of the parameter.
        /// </remarks>
        /// <value>The parameter type. </value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed.</exception>
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.Category("ICapeParameterSpec")]
        public override CapeParamType Type
        {
            get
            {
                return CapeParamType.CAPE_ARRAY;
            }
        }

        //ICapeArrayParameterSpec

        /// <summary>
        /// Gets an array of parameter specifications.
        /// </summary>
        /// <remarks>
        /// Gets an array of the specifications of each of the items in the value of a parameter. The Get method 
        /// returns an array of interfaces to the correct specification type (<see cref=" ICapeRealParameterSpec"/>, 
        /// <see cref=" ICapeOptionParameterSpec"/>, <see cref="ICapeIntegerParameterSpec"/>, or <see cref="ICapeBooleanParameterSpec"/>).
        /// Note that it is also possible, for example, to configure an array of arrays, which would a 
        /// similar but not identical concept to a two-dimensional matrix.
        /// </remarks>
        /// <value>An array of parameter specifications. </value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed.</exception>
        [System.ComponentModel.BrowsableAttribute(false)]
        object[] ICapeArrayParameterSpec.ItemsSpecifications
        {
            get
            {
                return (object[])((ICapeArrayParameterSpec)m_parameter.Specification).ItemsSpecifications;
            }
        }

        /// <summary>
        /// Gets the number of dimensions of the array value in the parameter.
        /// </summary>
        /// <remarks>
        /// The number of dimensions of the array value in the parameter.
        /// </remarks>
        /// <value>The number of dimensions of the array value in the parameter. </value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed.</exception>
        [System.ComponentModel.Category("Parameter Specification")]
        int ICapeArrayParameterSpec.NumDimensions
        {
            get
            {
                return ((ICapeArrayParameterSpec)m_parameter.Specification).NumDimensions;
            }
        }

        /// <summary>
        /// Gets the size of each one of the dimensions of the array.
        /// </summary>
        /// <remarks>
        /// Gets the size of each one of the dimensions of the array.
        /// </remarks>			
        /// <value>An integer array containing the size of each one of the dimensions of the array.</value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed.</exception>
        [System.ComponentModel.Category("Parameter Specification")]
        int[] ICapeArrayParameterSpec.Size
        {
            get
            {
                return (int[])((ICapeArrayParameterSpec)m_parameter.Specification).Size;
            }
        }

        /// <summary>
        /// Determines whether a value is valid for the wrapped parameter.
        /// </summary>
        /// <remarks>
        /// <para>Validates an array against the parameter's specification. It returns a flag to indicate the success or 
        /// failure of the validation together with a text message which can be used to convey the reasoning to 
        /// the client/user.</para>
        /// <para>The wrapped parameter validates the value against its internal validation criteria.</para>
        /// </remarks>
        /// <returns>
        /// True if the parameter is valid, false if not valid.
        /// </returns>
        /// <param name = "value">The value to be checked.</param>
        /// <param name = "messages">Reference to a string that will conain a message regarding the validation of the parameter.</param>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed, for example, an unrecognised Compound identifier or UNDEFINED for the props argument.</exception>
        object ICapeArrayParameterSpec.Validate(object value, ref string[] messages)
        {
            return ((ICapeArrayParameterSpec)m_parameter.Specification).Validate(value, ref messages);
        }
    };
}
