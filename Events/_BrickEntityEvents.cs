using BrickSchema.Net.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net
{
    /// <summary>
    /// This class is part of BrickEnity. Placing in this folder for organization purpose only.
    /// </summary>
    public partial class BrickEntity
    {
        public delegate void PropertyChangedEventHandler(string entityId, string propertyName);
        //events
        public event PropertyChangedEventHandler OnPropertyValueChanged;

        internal void NotifyPropertyValueChange(string propertyName)
        {
            OnPropertyValueChanged?.Invoke(Id, propertyName);
        }
        internal void NotifyPropertyValueChange(PropertiesEnum propertyName)
        {
            OnPropertyValueChanged?.Invoke(Id, propertyName.ToString());
        }

        internal virtual async void HandleOnPropertyValueChanged(string entityId, string propertyName)
        {
            await Task.Run(() =>
            {
                if (entityId == Id) { return; }
                OnPropertyValueChanged?.Invoke(entityId, propertyName);
            });


        }
    }
}
