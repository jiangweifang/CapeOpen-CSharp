using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CapeOpen
{
    /// <summary>
    /// Abstract base class that implements ICapeIdentification and ICapeUtilities. 
    /// </summary>
    /// <remarks>
    /// This abstract class contains all required functionality for ICapeIdentification and ICapeUtilities
    /// It can be inherited and used as any generalized PMC. The derived class will register itself as a 
    /// CAPE-OPEN Component (Category GUID of 678c09a1-7d66-11d2-a67d-00105a42887f) and a Flowsheet
    /// monitoring Object (Category GUID of 7BA1AF89-B2E4-493d-BD80-2970BF4CBE99).
    /// </remarks>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.InteropServices.Guid("2f8fdc51-b6c4-3df1-b286-c54ffc4b2b7f")]
    [System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.None)]
    public abstract class CapeObjectBase : CapeIdentification,
        ICapeUtilities,
        ICapeUtilitiesCOM,
        CapeOpen.ECapeUser,
        CapeOpen.ECapeRoot,
        IPersistStream,
        IPersistStreamInit
    {
        /// <summary>
        /// The message returned during the last validation of the unit operation.
        /// </summary>
        protected string m_ValidationMessage;
        private ParameterCollection m_Parameters;
        [NonSerialized]
        private System.Exception p_Exception;

        // Track whether Dispose has been called.
        private bool _disposed;
        [NonSerialized]
        private bool m_dirty;
        [NonSerialized]
        private bool m_Loaded;
        /// <summary>
        ///	The simulation context that can be used by the PMC.
        /// </summary>
        /// <remarks>
        /// The simulation cotext provides access to the PME, enabling the PMC to access the PME's COSE interfaces <see cref ="ICapeDiagnostic"/>, 
        /// <see cref ="ICapeMaterialTemplateSystem"/> and <see cref ="ICapeCOSEUtilities"/>.
        /// </remarks>
        [NonSerialized]
        private ICapeSimulationContext m_SimulationContext;

        /// <summary>
        /// Event triggered when a new parameter is being added to the collection.
        /// </summary>
        public event AddingNewEventHandler Parameters_AddingNew;

        /// <summary>
        /// Occurs when the list of parameters changes.
        /// </summary>
        /// <remarks>This event is triggered whenever there is a change in the parameters list, such as
        /// adding, removing, or updating an item. Subscribers can handle this event to respond to changes in the
        /// list.</remarks>
        public event ListChangedEventHandler Parameters_ListChanged;

        private void m_Parameters_AddingNew(object sender, AddingNewEventArgs e)
        {
            Parameters_AddingNew?.Invoke(sender, e);
        }

        private void m_Parameters_ListChanged(object sender, ListChangedEventArgs e)
        {
            Parameters_ListChanged?.Invoke(sender, e);
        }

        /// <summary>
        ///	Displays the PMC graphic interface, if available.
        /// </summary>
        /// <remarks>
        /// <para>By default, this method throws a <see cref="CapeNoImplException">CapeNoImplException</see>
        /// that according to the CAPE-OPEN specification, is interpreted by the process
        /// modeling environment as indicating that the PMC does not have a editor 
        /// GUI, and the PME must perform editing steps.</para>
        /// <para>In order for a PMC to provide its own editor, the Edit method will
        /// need to be overridden to create a graphical editor. When the user requests the flowheet
        /// to show the editor, this method will be called to edit the unit. Overriden classes should
        /// not return a failure (throw and exception) as this will be interpreted by the flowsheeting 
        /// tool as the unit not providing its own editor.</para>
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        int ICapeUtilitiesCOM.Edit()
        {
            try
            {
                CrashLogger.Logger?.Info("ICapeUtilitiesCOM.Edit() invoked on {0} (Name={1})", this.GetType().FullName, this.ComponentName);
                System.Windows.Forms.DialogResult result = this.Edit();
                if (result == System.Windows.Forms.DialogResult.OK)
                    return 0;
                return 1;
            }
            catch (CapeNoImplException)
            {
                // The PMC explicitly opted out of providing an editor. Propagate as-is.
                throw;
            }
            catch (System.Exception p_Ex)
            {
                // Persist full diagnostic information BEFORE replacing the exception,
                // otherwise the original cause is lost when this method returns to the host.
                CrashLogger.LogException(p_Ex,
                    string.Format("Edit() crashed for {0} (Name={1})",
                        this.GetType().FullName, this.ComponentName));
                throw new CapeNoImplException("No editor available");
            }
        }
        
        /// <summary>
        ///	Gets the component's collection of parameters.
        /// </summary>
        /// <value>
        /// Return type is System.Object and this method is simply here for classic 
        /// COM-based CAPE-OPEN interop.
        /// </value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeNoImpl">ECapeNoImpl</exception>
        [System.ComponentModel.BrowsableAttribute(false)]
        Object ICapeUtilitiesCOM.parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        /// <summary>
        ///	Sets the component's simulation context.
        /// </summary>
        /// <remarks>
        /// This method provides access to the COSE's interfaces <see cref ="ICapeDiagnostic"/>, 
        /// <see cref ="ICapeMaterialTemplateSystem"/> and <see cref ="ICapeCOSEUtilities"/>.
        /// </remarks>
        /// <value>The simulation context assigned by the Flowsheeting Environment.</value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed, for example, an unrecognised Compound identifier or UNDEFINED for the props argument.</exception>
        /// <exception cref = "ECapeNoImpl">ECapeNoImpl</exception>
        [System.ComponentModel.BrowsableAttribute(false)]
        Object ICapeUtilitiesCOM.simulationContext
        {
            set
            {
                if (value is ICapeSimulationContext)
                    m_SimulationContext = (ICapeSimulationContext)value;
            }
        }


        /// <summary>
        ///	Clean-up tasks can be performed here. 
        /// </summary>
        /// <remarks>
        /// <para>The CAPE-OPEN object should releases all of its allocated resources during this call. This is 
        /// called before the object destructor by the PME. Terminate may check if the data has been 
        /// saved and return an error if not.</para>
        /// <para>There are no input or output arguments for this method.</para>
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeOutOfResources">ECapeOutOfResources</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        void ICapeUtilitiesCOM.Terminate()
        {
            this.Terminate();
        }

        /// <summary>
        ///	Initialization can be performed here. 
        /// </summary>
        /// <remarks>
        /// <para>The CAPE_OPEN object can allocated resources during this method. This is 
        /// called after the object constructor by the PME. .</para>
        /// <para>There are no input or output arguments for this method.</para>
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s), specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeOutOfResources">ECapeOutOfResources</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        void ICapeUtilitiesCOM.Initialize()
        {
            this.Initialize();
        }

        /// <summary>
        ///	Constructor for the unit operation.
        /// </summary>
        /// <remarks>
        /// This method is creates the parameter collections for the object. As a result, 
        /// parameters can be added in the constructor
        /// for the derived object or during the <c>Initialize()</c> call. 
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeOutOfResources">ECapeOutOfResources</exception>
        /// <exception cref = "ECapeLicenceError">ECapeLicenceError</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        static CapeObjectBase()
        {
            // Initialize crash logging as soon as any PMC type is touched by the host process.
            CrashLogger.Initialize();
        }

        public CapeObjectBase()
            : base()
        {
            m_Parameters = new ParameterCollection();
            this.m_SimulationContext = null;
            this.m_ValidationMessage = "This object has not been validated.";
            _disposed = false;
            m_dirty = false;
            m_Loaded = false;
        }

        /// <summary>
        /// Finalizer for the <see cref = "CapeObjectBase"/> class.
        /// </summary>
        /// <remarks>
        /// This will finalize the current instance of the class.
        /// </remarks>
        ~CapeObjectBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///	Constructor for the unit operation.
        /// </summary>
        /// <remarks>
        /// This method is creates the parameter collections for the object. As a result, 
        /// parameters can be added in the constructor
        /// for the derived object or during the <c>Initialize()</c> call. 
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeOutOfResources">ECapeOutOfResources</exception>
        /// <exception cref = "ECapeLicenceError">ECapeLicenceError</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        /// <param name = "name">The name of the PMC.</param>
        public CapeObjectBase(String name)
            : base(name)
        {
            m_Parameters = new ParameterCollection();
            m_Parameters.AddingNew += new AddingNewEventHandler(m_Parameters_AddingNew);
            m_Parameters.ListChanged += new ListChangedEventHandler(m_Parameters_ListChanged);
            this.m_SimulationContext = null;
            this.m_ValidationMessage = "This object has not been validated.";
            _disposed = false;
            m_dirty = false;
            m_Loaded = false;
        }

        /// <summary>
        ///	Constructor for the unit operation.
        /// </summary>
        /// <remarks>
        /// This method is creates the parameter collections for the object. As a result, 
        /// parameters can be added in the constructor
        /// for the derived object or during the <c>Initialize()</c> call. 
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeOutOfResources">ECapeOutOfResources</exception>
        /// <exception cref = "ECapeLicenceError">ECapeLicenceError</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        /// <param name = "name">The name of the PMC.</param>
        /// <param name = "description">The description of the PMC.</param>
        public CapeObjectBase(String name, String description)
            : base(name, description)
        {
            m_Parameters = new ParameterCollection();
            m_Parameters.AddingNew += new AddingNewEventHandler(m_Parameters_AddingNew);
            m_Parameters.ListChanged += new ListChangedEventHandler(m_Parameters_ListChanged);
            this.m_SimulationContext = null;
            this.m_ValidationMessage = "This object has not been validated.";
            _disposed = false;
            m_dirty = false;
            m_Loaded = false;
        }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <remarks>
        /// <para>
        /// Clone can be implemented either as a deep copy or a shallow copy. In a deep copy, all objects are duplicated; 
        /// in a shallow copy, only the top-level objects are duplicated and the lower levels contain references.
        /// </para>
        /// <para>
        /// The resulting clone must be of the same type as, or compatible with, the original instance.
        /// </para>
        /// <para>
        /// See <see cref="Object.MemberwiseClone"/> for more information on cloning, deep versus shallow copies, and examples.
        /// </para>
        /// </remarks>
        /// <param name = "objectToBeCopied">The object being copied.</param>
        public CapeObjectBase(CapeObjectBase objectToBeCopied)
            : base((CapeIdentification)objectToBeCopied)
        {
            m_SimulationContext = objectToBeCopied.m_SimulationContext;
            // 这里我不确定这样做对不对
            m_Parameters.AddingNew -= new AddingNewEventHandler(m_Parameters_AddingNew);
            m_Parameters.ListChanged -= new ListChangedEventHandler(m_Parameters_ListChanged);
            m_Parameters.Clear();
            foreach (CapeParameter parameter in objectToBeCopied.Parameters)
            {
                m_Parameters.Add((CapeParameter)parameter.Clone());
            }
            m_Parameters.AddingNew += new AddingNewEventHandler(m_Parameters_AddingNew);
            m_Parameters.ListChanged += new ListChangedEventHandler(m_Parameters_ListChanged);
            this.m_ValidationMessage = "This object has not been validated.";
            _disposed = false;
            m_dirty = false;
            m_Loaded = false;
        }


        /// <summary>
        /// Creates a new object that is a copy of the current instance.</summary>
        /// <remarks>
        /// <para>
        /// Clone can be implemented either as a deep copy or a shallow copy. In a deep copy, all objects are duplicated; 
        /// in a shallow copy, only the top-level objects are duplicated and the lower levels contain references.
        /// </para>
        /// <para>
        /// The resulting clone must be of the same type as, or compatible with, the original instance.
        /// </para>
        /// <para>
        /// See <see cref="Object.MemberwiseClone"/> for more information on cloning, deep versus shallow copies, and examples.
        /// </para>
        /// </remarks>
        /// <returns>A new object that is a copy of this instance.</returns>
        override public object Clone()
        {
            CapeObjectBase retVal = (CapeObjectBase)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(this.GetType().AssemblyQualifiedName, this.GetType().FullName);
            retVal.Parameters.Clear();
            foreach (CapeParameter param in this.Parameters)
            {
                retVal.Parameters.Add((CapeParameter)param.Clone());
            }
            retVal.SimulationContext = null;
            if (retVal.GetType().IsAssignableFrom(typeof(ICapeSimulationContext)))
                retVal.SimulationContext = (ICapeSimulationContext)this.m_SimulationContext;
            return retVal;
        }
                        
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        /// <summary>
        /// Releases the unmanaged resources used by the CapeIdentification object and optionally releases 
        /// the managed resources.
        /// </summary>
        /// <remarks><para>This method is called by the public <see href="http://msdn.microsoft.com/en-us/library/system.componentmodel.component.dispose.aspx">Dispose</see>see> 
        /// method and the <see href="http://msdn.microsoft.com/en-us/library/system.object.finalize.aspx">Finalize</see> method. 
        /// <bold>Dispose()</bold> invokes the protected <bold>Dispose(Boolean)</bold> method with the disposing
        /// parameter set to <bold>true</bold>. <see href="http://msdn.microsoft.com/en-us/library/system.object.finalize.aspx">Finalize</see> 
        /// invokes <bold>Dispose</bold> with disposing set to <bold>false</bold>.</para>
        /// <para>When the <italic>disposing</italic> parameter is <bold>true</bold>, this method releases all 
        /// resources held by any managed objects that this Component references. This method invokes the 
        /// <bold>Dispose()</bold> method of each referenced object.</para>
        /// <para><bold>Notes to Inheritors</bold></para>
        /// <para><bold>Dispose</bold> can be called multiple times by other objects. When overriding 
        /// <bold>Dispose(Boolean)</bold>, be careful not to reference objects that have been previously 
        /// disposed of in an earlier call to <bold>Dispose</bold>. For more information about how to 
        /// implement <bold>Dispose(Boolean)</bold>, see <see href="http://msdn.microsoft.com/en-us/library/fs2xkftw.aspx">Implementing a Dispose Method</see>.</para>
        /// <para>For more information about <bold>Dispose</bold> and <see href="http://msdn.microsoft.com/en-us/library/system.object.finalize.aspx">Finalize</see>, 
        /// see <see href="http://msdn.microsoft.com/en-us/library/498928w2.aspx">Cleaning Up Unmanaged Resources</see> 
        /// and <see href="http://msdn.microsoft.com/en-us/library/ddae83kx.aspx">Overriding the Finalize Method</see>.</para>
        /// </remarks> 
        /// <param name = "disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (m_SimulationContext != null)
                    {
                        if (m_SimulationContext.GetType().IsCOMObject)
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(m_SimulationContext);
                    }
                    m_SimulationContext = null;
                    m_Parameters.Clear();
                    _disposed = true;
                }
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///	The function that controls COM registration.  
        /// </summary>
        /// <remarks>
        ///	This function adds the registration keys specified in the CAPE-OPEN Method and
        /// Tools specifications. In particular, it indicates that this unit operation implements
        /// the CAPE-OPEN Unit Operation Category Identification. It also adds the CapeDescription
        /// registry keys using the <see cref ="CapeNameAttribute"/>, <see cref ="CapeDescriptionAttribute"/>, <see cref ="CapeVersionAttribute"/>
        /// <see cref ="CapeVendorURLAttribute"/>, <see cref ="CapeHelpURLAttribute"/>, 
        /// and <see cref ="CapeAboutAttribute"/> attributes.
        /// </remarks>
        /// <param name = "t">The type of the class being registered.</param> 
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        [System.Runtime.InteropServices.ComRegisterFunction()]
        public static void RegisterFunction(Type t)
        {
            // No-op under .NET 8 ComHost. The original .NET Framework implementation
            // wrote CAPE-OPEN-specific registry keys (Implemented Categories + CapeDescription),
            // but it relied on APIs that are unavailable or behave differently on .NET 8
            // (e.g. Assembly.CodeBase throws NotSupportedException; ClassesRoot.OpenSubKey for
            // a not-yet-existing CLSID subkey returns null causing NREs that abort the entire
            // DllRegisterServer call). Use tools/CapeOpenRegistrar to write those keys.
        }

        /// <summary>
        ///	This function controls the removal of the class from the COM registry when the class is unistalled.  
        /// </summary>
        /// <remarks>
        ///	The method will remove all subkeys added to the class' regristration, including the CAPE-OPEN
        /// specific keys added in the <see cref ="RegisterFunction"/> method.
        /// </remarks>
        /// <param name = "t">The type of the class being unregistered.</param> 
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        [System.Runtime.InteropServices.ComUnregisterFunction()]
        public static void UnregisterFunction(Type t)
        {
            // No-op under .NET 8 ComHost. See RegisterFunction for rationale.
        }

        /// <summary>
        ///	Initialization can be performed here. 
        /// </summary>
        /// <remarks>
        /// <para>The CAPE_OPEN object can allocated resources during this method. This is 
        /// called after the object constructor by the PME. .</para>
        /// <para>There are no input or output arguments for this method.</para>
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s), specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeOutOfResources">ECapeOutOfResources</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        virtual public void Initialize()
        {
        }


        /// <summary>
        ///	Clean-up tasks can be performed here. 
        /// </summary>
        /// <remarks>
        /// <para>The CAPE-OPEN object should releases all of its allocated resources during this call. This is 
        /// called before the object destructor by the PME. Terminate may check if the data has been 
        /// saved and return an error if not.</para>
        /// <para>There are no input or output arguments for this method.</para>
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeOutOfResources">ECapeOutOfResources</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        virtual public void Terminate()
        {
            this.Dispose();
        }

        /// <summary>
        ///	Gets the component's collection of parameters. 
        /// </summary>
        /// <remarks>
        /// <para>Return the collection of Public Parameters (i.e. 
        /// <see cref = "ICapeCollection"/>.</para>
        /// <para>These are delivered as a collection of elements exposing the interface 
        /// <see cref = "ICapeParameter"/>. From there, the client could extract the 
        /// <see cref = "ICapeParameterSpec"/> interface or any of the typed
        /// interfaces such as <see cref = "ICapeRealParameterSpec"/>, once the client 
        /// establishes that the Parameter is of type double.</para>
        /// </remarks>
        /// <value>The parameter collection of the unit operation.</value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeNoImpl">ECapeNoImpl</exception>
        [System.ComponentModel.EditorAttribute(typeof(ParameterCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [System.ComponentModel.CategoryAttribute("Parameter Collection")]
        [System.ComponentModel.TypeConverter(typeof(ParameterCollectionTypeConverter))]
        public ParameterCollection Parameters
        {
            get
            {
                return m_Parameters;
            }
        }

        /// <summary>
        /// Validates the PMC. 
        /// </summary>
        /// <remarks>
        /// <para>Validates the parameter collection. This base-class implementation of this method 
        /// traverses the parameter collections and calls the  <see cref = "Validate"/> method of each 
        /// member parameter. The PMC is valid if all parameters are valid, which is 
        /// signified by the Validate method returning <c>true</c>.</para>
        /// </remarks>
        /// <returns>
        /// <para>true, if the unit is valid.</para>
        /// <para>false, if the unit is not valid.</para>
        /// </returns>
        /// <param name = "message">Reference to a string that will conain a message regarding the validation of the parameter.</param>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeBadCOParameter">ECapeBadCOParameter</exception>
        /// <exception cref = "ECapeBadInvOrder">ECapeBadInvOrder</exception>
        public virtual bool Validate(ref String message)
        {
            message = "Object is valid.";
            this.m_ValidationMessage = message;
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                String testString = String.Empty;
                if (!m_Parameters[i].Validate(ref testString))
                {
                    message = testString;
                    this.m_ValidationMessage = message;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///	Displays the PMC graphic interface, if available.
        /// </summary>
        /// <remarks>
        /// <para>By default, this method throws a <see cref="CapeNoImplException">CapeNoImplException</see>
        /// that according to the CAPE-OPEN specification, is interpreted by the process
        /// modeling environment as indicating that the PMC does not have a editor 
        /// GUI, and the PME must perform editing steps.</para>
        /// <para>In order for a PMC to provide its own editor, the Edit method will
        /// need to be overridden to create a graphical editor. When the user requests the flowheet
        /// to show the editor, this method will be called to edit the unit. Overriden classes should
        /// not return a failure (throw and exception) as this will be interpreted by the flowsheeting 
        /// tool as the unit not providing its own editor.</para>
        /// </remarks>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        public virtual System.Windows.Forms.DialogResult Edit()
        {
            throw new CapeNoImplException("No Object Editor");
        }


        /// <summary>
        ///	Gets and sets the component's simulation context.
        /// </summary>
        /// <remarks>
        /// This method provides access to the COSE's interfaces <see cref ="ICapeDiagnostic"/>, 
        /// <see cref ="ICapeMaterialTemplateSystem"/> and <see cref ="ICapeCOSEUtilities"/>.
        /// </remarks>
        /// <value>The simulation context assigned by the Flowsheeting Environment.</value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed, for example, an unrecognised Compound identifier or UNDEFINED for the props argument.</exception>
        /// <exception cref = "ECapeNoImpl">ECapeNoImpl</exception>
        [System.ComponentModel.BrowsableAttribute(false)]
        public ICapeSimulationContext SimulationContext
        {
            get
            {
                return m_SimulationContext;
            }
            set
            {
                m_SimulationContext = value;
            }
        }
        /// <summary>
        ///	Gets the component's flowsheet monitoring object.
        /// </summary>
        /// <remarks>
        /// This method provides access to the COSE's interfaces <see cref ="ICapeDiagnostic"/>, 
        /// <see cref ="ICapeMaterialTemplateSystem"/> and <see cref ="ICapeCOSEUtilities"/>.
        /// </remarks>
        /// <value>The simulation context assigned by the Flowsheeting Environment.</value>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeFailedInitialisation">ECapeFailedInitialisation</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed, for example, an unrecognised Compound identifier or UNDEFINED for the props argument.</exception>
        /// <exception cref = "ECapeNoImpl">ECapeNoImpl</exception>
        [System.ComponentModel.BrowsableAttribute(false)]
        public ICapeFlowsheetMonitoring FlowsheetMonitoring
        {
            get
            {
                if (m_SimulationContext is ICapeFlowsheetMonitoring)
                {
                    return (ICapeFlowsheetMonitoring)m_SimulationContext;
                }
                return null;
            }
        }

        /// <summary>
        /// Throws an exception and exposes the exception object.
        /// </summary>
        /// <remarks>
        /// This method allows the derived class to conform to the CAPE-OPEN error handling standards and still use .Net 
        /// exception handling. In order to use this class, create an exception object that derives from <see cref ="ECapeUser"/>.
        /// Use the exception object as the argument to this function. As a result, the information in the expcetion will be exposed using the CAPE-OPEN 
        /// exception handing and will be thrown to .Net clients.
        /// </remarks>
        /// <param name="exception">The exception that will the throw.</param>
        public void throwException(System.Exception exception)
        {
            p_Exception = exception;
            throw this.p_Exception;
        }

        // ECapeRoot method
        // returns the message string in the System.ApplicationException.
        /// <summary>
        /// The name of the exception being thrown.
        /// </summary>
        /// <remarks>
        /// The name of the exception being thrown.
        /// </remarks>
        /// <value>
        /// The name of the exception being thrown.
        /// </value>
        [System.ComponentModel.BrowsableAttribute(false)]
        String ECapeRoot.Name
        {
            get
            {
                if (p_Exception is ECapeRoot) return ((ECapeRoot)p_Exception).Name;
                return "";
            }
        }

        /// <summary>
        /// Code to designate the subcategory of the error. 
        /// </summary>
        /// <remarks>
        /// The assignment of values is left to each implementation. So that is a 
        /// proprietary code specific to the CO component provider. By default, set to 
        /// the CAPE-OPEN error HRESULT <see cref = "CapeErrorInterfaceHR"/>.
        /// </remarks>
        /// <value>
        /// The HRESULT value for the exception.
        /// </value>
        [System.ComponentModel.BrowsableAttribute(false)]
        int ECapeUser.code
        {
            get
            {
                return ((ECapeUser)p_Exception).code;
            }
        }

        /// <summary>
        /// The description of the error.
        /// </summary>
        /// <remarks>
        /// The error description can include a more verbose description of the condition that
        /// caused the error.
        /// </remarks>
        /// <value>
        /// A string description of the exception.
        /// </value>
        [System.ComponentModel.BrowsableAttribute(false)]
        String ECapeUser.description
        {
            get
            {
                return ((ECapeUser)p_Exception).description;
            }
        }

        /// <summary>
        /// The scope of the error.
        /// </summary>
        /// <remarks>
        /// This property provides a list of packages where the error occurred. 
        /// For example <see cref = "ICapeIdentification"/>.
        /// </remarks>
        /// <value>The source of the error.</value>
        [System.ComponentModel.BrowsableAttribute(false)]
        String ECapeUser.scope
        {
            get
            {
                return ((ECapeUser)p_Exception).scope;
            }
        }

        /// <summary>
        /// The name of the interface where the error is thrown. This is a mandatory field."
        /// </summary>
        /// <remarks>
        /// The interface that the error was thrown.
        /// </remarks>
        /// <value>The name of the interface.</value>
        [System.ComponentModel.BrowsableAttribute(false)]
        String ECapeUser.interfaceName
        {
            get
            {
                return ((ECapeUser)p_Exception).interfaceName;
            }
        }

        /// <summary>
        /// The name of the operation where the error is thrown. This is a mandatory field.
        /// </summary>
        /// <remarks>
        /// This field provides the name of the operation being perfomed when the exception was raised.
        /// </remarks>
        /// <value>The operation name.</value>
        [System.ComponentModel.BrowsableAttribute(false)]
        String ECapeUser.operation
        {
            get
            {
                return ((ECapeUser)p_Exception).operation;
            }
        }

        /// <summary>
        /// An URL to a page, document, web site,  where more information on the error can be found. The content of this information is obviously implementation dependent.
        /// </summary>
        /// <remarks>
        /// This field provides an internet URL where more information about the error can be found.
        /// </remarks>
        /// <value>The URL.</value>
        [System.ComponentModel.BrowsableAttribute(false)]
        String ECapeUser.moreInfo
        {
            get
            {
                return ((ECapeUser)p_Exception).moreInfo;
            }
        }

        /// <summary>
        /// Writes a message to the terminal.
        /// </summary>
        /// <remarks>
        /// <para>Write a string to the terminal.</para>
        /// <para>This method is called when a message needs to be brought to the user’s attention.
        /// The implementation should ensure that the string is written out to a dialogue box or 
        /// to a message list that the user can easily see.</para>
        /// <para>A priori this message has to be displayed as soon as possible to the user.</para>
        /// </remarks>
        /// <param name = "message">The text to be displayed.</param>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed, for example, an unrecognised Compound identifier or UNDEFINED for the props argument.</exception>
        public void PopUpMessage(System.String message)
        {
            if (m_SimulationContext != null)
            {
                if (m_SimulationContext is ICapeDiagnostic)
                {
                    ((ICapeDiagnostic)this.m_SimulationContext).PopUpMessage(message);
                }
            }
        }
        /// <summary>
        /// Writes a string to the PME's log file.
        /// </summary>
        /// <remarks>
        /// <para>Write a string to a log.</para>
        /// <para>This method is called when a message needs to be recorded for logging purposes. 
        /// The implementation is expected to write the string to a log file or other journaling 
        /// device.</para>
        /// </remarks>
        /// <param name = "message">The text to be logged.</param>
        /// <exception cref ="ECapeUnknown">The error to be raised when other error(s),  specified for this operation, are not suitable.</exception>
        /// <exception cref = "ECapeInvalidArgument">To be used when an invalid argument value is passed, for example, an unrecognised Compound identifier or UNDEFINED for the props argument.</exception>
        public void LogMessage(System.String message)
        {
            if (m_SimulationContext != null)
            {
                if (m_SimulationContext is ICapeDiagnostic)
                {
                    ((ICapeDiagnostic)this.m_SimulationContext).LogMessage(message);
                }
            }
        }

        // IPersistStream

        void IPersistStream.GetClassID(out Guid pClassID)
        {
            pClassID = this.GetType().GUID;
        }

        /// <summary>This method checks the object for changes since it was last saved.</summary>
        /// <returns>S_OK (0) if dirty; S_FALSE (1) otherwise.</returns>
        int IPersistStream.IsDirty()
        {
            if (m_dirty) return 0;
            return 1;
        }

        /// <summary>This method saves an object to the specified stream.</summary>
        /// <param name="pStm">IStream to write to.</param>
        /// <param name="fClearDirty">True to clear the dirty flag after save.</param>
        void IPersistStream.Save(System.Runtime.InteropServices.ComTypes.IStream pStm, bool fClearDirty)
        {
            SaveToStream(pStm);
            if (fClearDirty) m_dirty = false;
        }

        /// <summary>This method initializes an object from the stream where it was previously saved.</summary>
        /// <param name="pStm">IStream to read from.</param>
        void IPersistStream.Load(System.Runtime.InteropServices.ComTypes.IStream pStm)
        {
            LoadFromStream(pStm);
        }

        /// <summary>This method returns the size, in bytes, of the stream needed to save the object.</summary>
        /// <param name="pcbSize">Pointer to size in bytes.</param>
        void IPersistStream.GetSizeMax(out long pcbSize)
        {
            pcbSize = 0;
        }

        // IPersistStreamInit

        void IPersistStreamInit.GetClassID(out Guid pClassID)
        {
            pClassID = this.GetType().GUID;
        }

        /// <summary>This method checks the object for changes since it was last saved.</summary>
        /// <returns>S_OK (0) if dirty; S_FALSE (1) otherwise.</returns>
        int IPersistStreamInit.IsDirty()
        {
            if (m_dirty) return 0;
            return 1;
        }

        /// <summary>This method saves an object to the specified stream.</summary>
        /// <param name="pStm">IStream to write to.</param>
        /// <param name="fClearDirty">True to clear the dirty flag after save.</param>
        void IPersistStreamInit.Save(System.Runtime.InteropServices.ComTypes.IStream pStm, bool fClearDirty)
        {
            SaveToStream(pStm);
            if (fClearDirty) m_dirty = false;
        }

        /// <summary>This method initializes an object from the stream where it was previously saved.</summary>
        /// <param name="pStm">IStream to read from.</param>
        void IPersistStreamInit.Load(System.Runtime.InteropServices.ComTypes.IStream pStm)
        {
            LoadFromStream(pStm);
        }

        /// <summary>This method returns the size, in bytes, of the stream needed to save the object.</summary>
        /// <param name="pcbSize">Pointer to size in bytes.</param>
        void IPersistStreamInit.GetSizeMax(out long pcbSize)
        {
            pcbSize = 0;
        }

        /// <summary>Initializes an object to a default state. This method is to be called instead of IPersistStreamInit.Load.</summary>
        void IPersistStreamInit.InitNew()
        {
            if (m_Loaded)
            {
                throw new CapeUnexpectedException("The object has already been initialized with IPersistStreamInit.Load.");
            }
        }

        /// <summary>
        /// Saves the PMC state to a COM IStream using typed binary serialization with GZip compression.
        /// </summary>
        /// <remarks>
        /// Serializes ComponentName, ComponentDescription, and all parameter values using
        /// BinaryWriter with explicit types, following the FlowExchange storage pattern.
        /// V2 format saves full parameter metadata to support reconstruction of dynamically added parameters.
        /// Derived classes may override to persist additional state.
        /// </remarks>
        /// <param name="pStm">The COM IStream to write to.</param>
        protected virtual void SaveToStream(System.Runtime.InteropServices.ComTypes.IStream pStm)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            using (var gz = new System.IO.Compression.GZipStream(memoryStream, System.IO.Compression.CompressionMode.Compress))
            using (var writer = new System.IO.BinaryWriter(gz))
            {
                // V2 format marker
                writer.Write(STREAM_MAGIC_V2);
                writer.Write(this.ComponentName ?? "");
                writer.Write(this.ComponentDescription ?? "");
                writer.Write(this.Parameters.Count);
                for (int i = 0; i < this.Parameters.Count; i++)
                {
                    ICapeParameter param = (ICapeParameter)this.Parameters[i];
                    ICapeParameterSpec spec = (ICapeParameterSpec)param.Specification;
                    CapeIdentification paramId = (CapeIdentification)param;
                    writer.Write(paramId.ComponentName ?? "");
                    writer.Write(paramId.ComponentDescription ?? "");
                    writer.Write((int)param.Mode);
                    writer.Write((int)spec.Type);
                    switch (spec.Type)
                    {
                        case CapeParamType.CAPE_REAL:
                            {
                                RealParameter rp = (RealParameter)param;
                                writer.Write(rp.SIValue);
                                writer.Write(((ICapeRealParameterSpec)rp).SIDefaultValue);
                                writer.Write(((ICapeRealParameterSpec)rp).SILowerBound);
                                writer.Write(((ICapeRealParameterSpec)rp).SIUpperBound);
                                writer.Write(rp.Unit ?? "");
                            }
                            break;
                        case CapeParamType.CAPE_INT:
                            {
                                IntegerParameter ip = (IntegerParameter)param;
                                writer.Write(ip.Value);
                                writer.Write(((ICapeIntegerParameterSpec)ip).DefaultValue);
                                writer.Write(((ICapeIntegerParameterSpec)ip).LowerBound);
                                writer.Write(((ICapeIntegerParameterSpec)ip).UpperBound);
                            }
                            break;
                        case CapeParamType.CAPE_BOOLEAN:
                            {
                                BooleanParameter bp = (BooleanParameter)param;
                                writer.Write(bp.Value);
                                writer.Write(bp.DefaultValue);
                            }
                            break;
                        case CapeParamType.CAPE_OPTION:
                            {
                                OptionParameter op = (OptionParameter)param;
                                writer.Write(param.value is string ? (string)param.value : "");
                                writer.Write(((ICapeOptionParameterSpec)op).DefaultValue ?? "");
                                string[] optList = ((ICapeOptionParameterSpec)op).OptionList ?? new string[0];
                                writer.Write(optList.Length);
                                for (int k = 0; k < optList.Length; k++)
                                    writer.Write(optList[k] ?? "");
                                writer.Write(((ICapeOptionParameterSpec)op).RestrictedToList);
                            }
                            break;
                        case CapeParamType.CAPE_ARRAY:
                            WriteObject(writer, param.value);
                            WriteObject(writer, ((ArrayParameter)param).DefaultValue);
                            break;
                        default:
                            WriteObject(writer, param.value);
                            break;
                    }
                }
            }
            byte[] data = memoryStream.ToArray();
            byte[] lenBytes = BitConverter.GetBytes(data.Length);
            pStm.Write(lenBytes, 4, IntPtr.Zero);
            pStm.Write(data, data.Length, IntPtr.Zero);
        }

        /// <summary>
        /// Loads the PMC state from a COM IStream using typed binary deserialization with GZip decompression.
        /// </summary>
        /// <remarks>
        /// Restores ComponentName, ComponentDescription, and parameter values that were
        /// previously saved by <see cref="SaveToStream"/>. Supports both V1 (value-only)
        /// and V2 (full metadata) formats. For V2 streams, parameters not found in the
        /// constructor-created collection are recreated from saved metadata and appended.
        /// </remarks>
        /// <param name="pStm">The COM IStream to read from.</param>
        protected virtual void LoadFromStream(System.Runtime.InteropServices.ComTypes.IStream pStm)
        {
            try
            {
                m_Loaded = true;
                byte[] lenBytes = new byte[4];
                pStm.Read(lenBytes, 4, IntPtr.Zero);
                int len = BitConverter.ToInt32(lenBytes, 0);
                byte[] data = new byte[len];
                pStm.Read(data, len, IntPtr.Zero);
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(data);
                using (var gz = new System.IO.Compression.GZipStream(memoryStream, System.IO.Compression.CompressionMode.Decompress))
                using (var reader = new System.IO.BinaryReader(gz))
                {
                    // Detect format version: V2 starts with 4-byte magic, V1 starts with a string.
                    byte[] magic = reader.ReadBytes(4);
                    bool isV2 = magic.Length == 4
                        && magic[0] == STREAM_MAGIC_V2[0]
                        && magic[1] == STREAM_MAGIC_V2[1]
                        && magic[2] == STREAM_MAGIC_V2[2]
                        && magic[3] == STREAM_MAGIC_V2[3];
                    CrashLogger.Logger?.Info("LoadFromStream {0} (Name={1}) format={2} payloadBytes={3}",
                        this.GetType().FullName, this.ComponentName, isV2 ? "V2" : "V1", len);
                    if (isV2)
                    {
                        LoadFromStreamV2(reader);
                    }
                    else
                    {
                        // V1: re-decompress from the beginning since GZipStream is forward-only.
                        var memoryStream2 = new System.IO.MemoryStream(data);
                        using (var gz2 = new System.IO.Compression.GZipStream(memoryStream2, System.IO.Compression.CompressionMode.Decompress))
                        using (var reader2 = new System.IO.BinaryReader(gz2))
                        {
                            LoadFromStreamV1(reader2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException(ex,
                    string.Format("LoadFromStream failed for {0} (Name={1})",
                        this.GetType().FullName, this.ComponentName));
                throw;
            }
        }

        // Stream format version: V1 = original (no version byte), V2 = with full metadata.
        // We write a 4-byte magic marker to distinguish V2 from V1.
        // V1 starts with a BinaryWriter string (7-bit length prefix), so this 4-byte
        // sequence (0xCA, 0xFE, 0x00, 0x02) will never appear as a valid V1 start.
        private static readonly byte[] STREAM_MAGIC_V2 = { 0xCA, 0xFE, 0x00, 0x02 };

        /// <summary>
        /// V1 load: restores only parameter values by name match (original format).
        /// </summary>
        private void LoadFromStreamV1(System.IO.BinaryReader reader)
        {
            this.ComponentName = reader.ReadString();
            this.ComponentDescription = reader.ReadString();
            int paramCount = reader.ReadInt32();
            for (int i = 0; i < paramCount; i++)
            {
                string paramName = reader.ReadString();
                CapeParamType paramType = (CapeParamType)reader.ReadInt32();
                object paramValue = null;
                switch (paramType)
                {
                    case CapeParamType.CAPE_REAL:
                        paramValue = reader.ReadDouble();
                        break;
                    case CapeParamType.CAPE_INT:
                        paramValue = reader.ReadInt32();
                        break;
                    case CapeParamType.CAPE_BOOLEAN:
                        paramValue = reader.ReadBoolean();
                        break;
                    case CapeParamType.CAPE_OPTION:
                        paramValue = reader.ReadString();
                        break;
                    case CapeParamType.CAPE_ARRAY:
                        paramValue = ReadObject(reader);
                        break;
                    default:
                        paramValue = ReadObject(reader);
                        break;
                }
                for (int j = 0; j < this.Parameters.Count; j++)
                {
                    CapeIdentification existingParam = (CapeIdentification)this.Parameters[j];
                    if (existingParam.ComponentName == paramName)
                    {
                        ((ICapeParameter)this.Parameters[j]).value = paramValue;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// V2 load: restores parameter values by name match and recreates missing parameters from saved metadata.
        /// </summary>
        private void LoadFromStreamV2(System.IO.BinaryReader reader)
        {
            this.ComponentName = reader.ReadString();
            this.ComponentDescription = reader.ReadString();
            int paramCount = reader.ReadInt32();
            for (int i = 0; i < paramCount; i++)
            {
                string paramName = reader.ReadString();
                string paramDesc = reader.ReadString();
                CapeParamMode paramMode = (CapeParamMode)reader.ReadInt32();
                CapeParamType paramType = (CapeParamType)reader.ReadInt32();

                ICapeParameter existingParam = null;
                for (int j = 0; j < this.Parameters.Count; j++)
                {
                    CapeIdentification ep = (CapeIdentification)this.Parameters[j];
                    if (ep.ComponentName == paramName)
                    {
                        existingParam = (ICapeParameter)this.Parameters[j];
                        break;
                    }
                }

                switch (paramType)
                {
                    case CapeParamType.CAPE_REAL:
                        {
                            double val = reader.ReadDouble();
                            double defVal = reader.ReadDouble();
                            double lower = reader.ReadDouble();
                            double upper = reader.ReadDouble();
                            string unit = reader.ReadString();
                            if (existingParam != null)
                                existingParam.value = val;
                            else
                                this.Parameters.Add(new RealParameter(paramName, paramDesc, val, defVal, lower, upper, paramMode, unit));
                        }
                        break;
                    case CapeParamType.CAPE_INT:
                        {
                            int val = reader.ReadInt32();
                            int defVal = reader.ReadInt32();
                            int lower = reader.ReadInt32();
                            int upper = reader.ReadInt32();
                            if (existingParam != null)
                                existingParam.value = val;
                            else
                                this.Parameters.Add(new IntegerParameter(paramName, paramDesc, val, defVal, lower, upper, paramMode));
                        }
                        break;
                    case CapeParamType.CAPE_BOOLEAN:
                        {
                            bool val = reader.ReadBoolean();
                            bool defVal = reader.ReadBoolean();
                            if (existingParam != null)
                                existingParam.value = val;
                            else
                                this.Parameters.Add(new BooleanParameter(paramName, paramDesc, val, defVal, paramMode));
                        }
                        break;
                    case CapeParamType.CAPE_OPTION:
                        {
                            string val = reader.ReadString();
                            string defVal = reader.ReadString();
                            int optCount = reader.ReadInt32();
                            string[] optList = new string[optCount];
                            for (int k = 0; k < optCount; k++)
                                optList[k] = reader.ReadString();
                            bool restricted = reader.ReadBoolean();
                            if (existingParam != null)
                                existingParam.value = val;
                            else
                                this.Parameters.Add(new OptionParameter(paramName, paramDesc, val, defVal, optList, restricted, paramMode));
                        }
                        break;
                    case CapeParamType.CAPE_ARRAY:
                        {
                            object val = ReadObject(reader);
                            object defVal = ReadObject(reader);
                            if (existingParam != null)
                                existingParam.value = val;
                            else
                                this.Parameters.Add(new ArrayParameter(paramName, paramDesc, (object[])val, (object[])defVal, paramMode));
                        }
                        break;
                    default:
                        {
                            object val = ReadObject(reader);
                            if (existingParam != null)
                                existingParam.value = val;
                        }
                        break;
                }
            }
        }

        // Type tags for recursive typed object serialization (used for CAPE_ARRAY values).
        private const byte TAG_NULL = 0;
        private const byte TAG_DOUBLE = 1;
        private const byte TAG_INT = 2;
        private const byte TAG_BOOL = 3;
        private const byte TAG_STRING = 4;
        private const byte TAG_ARRAY = 5;

        /// <summary>
        /// Recursively writes an arbitrary parameter value (scalar, string, nested array,
        /// or <see cref="ICapeParameter"/> wrapper) with a leading type tag byte so that it
        /// can be faithfully reconstructed by <see cref="ReadObject"/>.
        /// </summary>
        private static void WriteObject(System.IO.BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write(TAG_NULL);
                return;
            }
            if (value is ICapeParameter)
            {
                // Unwrap nested parameter to its underlying value.
                WriteObject(writer, ((ICapeParameter)value).value);
                return;
            }
            if (value is double)
            {
                writer.Write(TAG_DOUBLE);
                writer.Write((double)value);
                return;
            }
            if (value is int)
            {
                writer.Write(TAG_INT);
                writer.Write((int)value);
                return;
            }
            if (value is bool)
            {
                writer.Write(TAG_BOOL);
                writer.Write((bool)value);
                return;
            }
            if (value is string)
            {
                writer.Write(TAG_STRING);
                writer.Write((string)value);
                return;
            }
            if (value is System.Array)
            {
                System.Array arr = (System.Array)value;
                writer.Write(TAG_ARRAY);
                writer.Write(arr.Length);
                for (int k = 0; k < arr.Length; k++)
                {
                    WriteObject(writer, arr.GetValue(k));
                }
                return;
            }
            // Fallback: persist as string representation.
            writer.Write(TAG_STRING);
            writer.Write(value.ToString());
        }

        /// <summary>
        /// Recursively reads a value previously written by <see cref="WriteObject"/>.
        /// Arrays are materialized as <c>object[]</c>.
        /// </summary>
        private static object ReadObject(System.IO.BinaryReader reader)
        {
            byte tag = reader.ReadByte();
            switch (tag)
            {
                case TAG_NULL:
                    return null;
                case TAG_DOUBLE:
                    return reader.ReadDouble();
                case TAG_INT:
                    return reader.ReadInt32();
                case TAG_BOOL:
                    return reader.ReadBoolean();
                case TAG_STRING:
                    return reader.ReadString();
                case TAG_ARRAY:
                    {
                        int n = reader.ReadInt32();
                        object[] result = new object[n];
                        for (int k = 0; k < n; k++)
                        {
                            result[k] = ReadObject(reader);
                        }
                        return result;
                    }
                default:
                    throw new System.IO.InvalidDataException("Unknown persistence type tag: " + tag);
            }
        }
    };
}
