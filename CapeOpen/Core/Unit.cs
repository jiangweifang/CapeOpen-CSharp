using System;
using Newtonsoft.Json;

namespace CapeOpen
{

    /// <summary>
    /// Provides information related to a unit of measure associated with a parameter.
    /// </summary>
    /// <remarks>
    /// <para>The unit of maesure can be either an System Internation (SI) or customary unit. Each unit is assigned to a 
    /// <see cref = "unitCategory"/> that has information related to the dimensionality of the unit.</para>
    /// </remarks>
    struct unit
    {
        /// <summary>
        /// The name of the unit of measure
        /// </summary>
        /// <remarks>The common name of the unit of measure. Typically, this field represents the abbreviation for the unit.</remarks>
        [JsonProperty("Unit")]
        public String Name;
        /// <summary>
        /// A description of the unit of measure
        /// </summary>
        /// <remarks>The description of the unit of measure.</remarks>
        public String Description;
        /// <summary>
        /// The category  of the unit of measure
        /// </summary>
        /// <remarks><para>The category for a unit of measure defines the dimensionality of the unit.</para>
        /// <para>The dimensionality of the parameter represents the physical dimensional axes of this parameter. It 
        /// is expected that the dimensionality must cover at least 6 fundamental axes (length, mass, time, angle, 
        /// temperature and charge). A possible implementation could consist in being a constant length array vector 
        /// that contains the exponents of each basic SI unit, following directives of SI-brochure (from 
        /// http://www.bipm.fr/). So if we agree on order &lt;m kg s A K,&gt; ... velocity would be &lt;1,0,-1,0,0,0&gt;: 
        /// that is m1 * s-1 =m/s. We have suggested to the  CO Scientific Committee to use the SI base units plus the 
        /// SI derived units with special symbols (for a better usability and for allowing the definition of angles).
        /// </para>
        /// </remarks>
        [JsonProperty("Category")]
        public String Category;
        /// <summary>
        /// A conversion factor used to multiply the value of the measurement by to convert the unit to its SI
        /// equivalent.
        /// </summary>
        /// <remarks>
        /// <para>Units are converted to and from the SI equivalent for the unit category. Unit conversions are 
        /// accomplished by first adding any offset, stored in <see cref = "ConversionPlus"/> to the value of the unit. 
        /// The sum is then multiplied by the value of the <see cref = "ConversionTimes"/> for the unit to get the 
        /// measured value in SI units.</para>
        /// </remarks>
        [JsonProperty("ConversionTimes")]
        public double ConversionTimes;
        /// <summary>
        /// An offset factor used in converting the value of the measurement to its SI equivalent.
        /// </summary>
        /// <remarks>
        /// <para>Units are converted to and from the SI equivalent for the unit category. Unit conversions are 
        /// accomplished by first adding any offset, stored in <see cref = "ConversionPlus"/> to the value of the unit. 
        /// The sum is then multiplied by the value of the <see cref = "ConversionTimes"/> for the unit to get the 
        /// measured value in SI units.</para>
        /// </remarks>
        [JsonProperty("ConversionPlus")]
        public double ConversionPlus;
    };
}
