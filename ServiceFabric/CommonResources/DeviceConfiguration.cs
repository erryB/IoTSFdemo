using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CommonResources
{
    /// <summary>
    /// Contains the configuration properties for a device.
    /// </summary>
    [DataContract]
    public class DeviceConfiguration
    {

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        [DataMember]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        #region [ Public Methods ]

        /// <summary>
        /// Adds the or update complex object to properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="complexObject">The complex object.</param>
        public void AddOrUpdateComplexObjectToProperties<T>(string key, T complexObject)
        {
            this.Properties[key] = JsonConvert.SerializeObject(complexObject);
        }

        /// <summary>
        /// Gets the complex object from properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns>T.</returns>
        public T GetComplexObjectFromProperties<T>(string key)
            where T : class, new()
        {
            T result = null;

            if (this.Properties.ContainsKey(key))
            {
                result = JsonConvert.DeserializeObject<T>(this.Properties[key].ToString());
            }

            return result;
        }

        /// <summary>
        /// Tries the get complex object from properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TryGetComplexObjectFromProperties<T>(string key, out T value)
            where T : class, new()
        {
            try
            {
                value = GetComplexObjectFromProperties<T>(key);
                return true;
            }
            catch //(Exception ex)
            {
                value = default(T);
                return false;
            }
        }
        #endregion [ Public Methods ]

    }
}
