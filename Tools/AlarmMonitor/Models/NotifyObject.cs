using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AlarmMonitor.Models
{
    public abstract class NotifyObject:INotifyPropertyChanged
    {
        #region [ INotifyPropertyChanged interface ]
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        #region [ Gestione delle property ]

        private readonly Dictionary<string, object> _Properties = new Dictionary<string, object>();


        protected void SetPropertyValue(object value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException();
            _Properties[propertyName] = value;
            OnPropertyChanged(propertyName);
        }

        protected T GetPropertyValue<T>([CallerMemberName()] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException();
            if (_Properties.ContainsKey(propertyName))
            {
                try
                {
                    return (T)_Properties[propertyName];
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            return default(T);
        }

        #endregion
    }
}
