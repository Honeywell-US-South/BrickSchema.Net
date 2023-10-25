using BrickSchema.Net.Alerts;
using BrickSchema.Net.Behaviors;
using BrickSchema.Net.EntityProperties;
using BrickSchema.Net.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Keep namespace as BirckSchema.Net
namespace BrickSchema.Net
{
    /// <summary>
    /// This class is part of BrickEnity. Placing in this folder for organization purpose only.
    /// </summary>
    public partial class BrickEntity
    {
        public AlertValue GetAlert()
        {
            var alert = GetProperty<AlertValue>(PropertiesEnum.AlertValue);
            if (alert == null)
            {
                alert = SetAlert(new());

            }
            return alert;
        }

        public AlertStatuses GetAlertStatus()
        {
            var alert = GetAlert();
            return alert.Status;
        }

        public AlertValue SetAlert(AlertValue alert)
        {
            var a = GetAlert();
            if (a == null)
            {
                a = alert;
                SetProperty(PropertiesEnum.AlertValue, a);
            } else
            {
                a.Set(alert);
            }
            return a;
        }

        public List<BehaviorValue> GetBehaviorFaultValues()
        {
            List<BehaviorValue> results = new();
            var bv = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            results = bv?.Where(x => x.FaultType == BehaviorFaultTypes.Fault).ToList() ?? new();
            return results;
        }

        public List<BehaviorValue> GetBehaviorAlarmValues()
        {
            List<BehaviorValue> results = new();
            var bv = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            results = bv?.Where(x => x.FaultType == BehaviorFaultTypes.Alarm).ToList() ?? new();
            return results;
        }

        public void SetBehaviorValue<T>(BrickBehavior behavior, string valueName, T value)
        {
            var results = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            if (results == null) results = new();
            var myValue = results?.FirstOrDefault(x=>x.Name== valueName && x.BehaviorId == behavior.Id);
            if (myValue == null)
            {
                myValue = new(valueName, behavior);
                myValue.SetValue(value);
                results?.Add(myValue);
            }  else
            {
                myValue.SetValue(value);
            }
            CleanUpDuplicatedProperties();
            SetProperty(PropertiesEnum.BehaviorValues, results);
            LastUpdate = DateTime.Now;
        }

        public void SetBehaviorValue(BehaviorValue behaviorValue)
        {
            var results = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            if (results == null) results = new();
            var myValue = results?.FirstOrDefault(x => x.Name == behaviorValue.Name && x.BehaviorId == behaviorValue.BehaviorId);
            if (myValue == null)
            {
                myValue = behaviorValue.Clone(false);
                results?.Add(myValue);
            }
            else
            {
                myValue.UpdateValue(behaviorValue);
            }
            CleanUpDuplicatedProperties();
            SetProperty(PropertiesEnum.BehaviorValues, results);
            LastUpdate = DateTime.Now;
        }

        public void SetBehaviorValue(List<BehaviorValue> values)
        {
            var results = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            if (results == null) results = new();
            foreach (var value in values)
            {
                var myValue = results?.FirstOrDefault(x => x.Name == value.Name && x.BehaviorId == value.BehaviorId);
                if (myValue == null)
                {
                    myValue = value.Clone(false);
                    results?.Add(myValue);
                }
                else
                {
                    myValue.UpdateValue(value);
                }
            }
            CleanUpDuplicatedProperties();
            SetProperty(PropertiesEnum.BehaviorValues, results);
            LastUpdate = DateTime.Now;
        }

        public List<BehaviorValue> GetBehaviorValues(BehaviorFunction.Types behaviorFunction, string labelName)
        {

            List<BehaviorValue> results = new();
            var bv = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            results = bv?.Where(x=> x.BehaviorFunction == behaviorFunction.ToString() && x.Name == labelName).ToList()??new();
            return results;
        }

        public BehaviorValue? GetBehaviorValue(string  behaviorType, string lableName)
        {
            var results = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            if (results == null) results = new();
            var myValue = results?.FirstOrDefault(x => x.Name == lableName && x.BehaviorEntityTypeName == behaviorType);
            return myValue;
        }

        public T? GetBehaviorValue<T>(string behaviorEntityTypeName, string lableName)
        {
            var results = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            if (results == null) results = new();
            var myValue = results?.FirstOrDefault(x => x.Name == lableName && x.BehaviorEntityTypeName == behaviorEntityTypeName);
            T? returnValue = default(T);
            if (myValue != null)
            {
                returnValue = myValue.GetValue<T>();
            }

            return returnValue;
        }

        public (T? Value, U? Weight)  GetBehaviorValue<T, U>(string behaviorType, string labelName)
        {
            var results = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            if (results == null) results = new();
            var myValue = results?.FirstOrDefault(x => x.Name == labelName && x.BehaviorEntityTypeName == behaviorType);
            (T?, U?) returnValue = (default(T), default(U));
            if (myValue != null)
            {
                returnValue = myValue.GetValue<T, U>();
            }

            return returnValue;
        }

        public List<BehaviorValue> GetBehaviorValues(string behaviorId)
        {
            var results = GetProperty<List<BehaviorValue>>(PropertiesEnum.BehaviorValues);
            if (results == null) results = new();
            var myValues = results?.Where(x => x.BehaviorId == behaviorId).ToList()??new();
            return myValues;
        }


        public void CleanUpDuplicatedProperties()
        {
            bool hasDuplidated = true;
            while (hasDuplidated)
            {
                bool found = false;
                for (int i = 0; i < Properties.Count; i++)
                {
                    found = false;
                    var p = Properties[i];
                    
                    for (int j = i + 1; j < Properties.Count; j++)
                    {
                        if (p.DataTypeName == Properties[j].DataTypeName && p.Name == Properties[j].Name)
                        {
                            found = true;
                            Properties.RemoveAt(j);
                            break;
                        }
                    }
                    if (found) break;

                }
                hasDuplidated = found;
            }
        }

        private readonly object propertyLock = new object();
        public void SetProperty<T>(string propertyName, T propertyValue)
        {
            lock (propertyLock)
            {
                var properties = Properties.Where(x => x.Name.Equals(propertyName)).ToList();
                EntityProperty? property = null;
                if (properties.Count > 1)
                {
                    for (int i = 1; i < properties.Count; i++)
                    { //keep first property[0];=
                        Properties.Remove(properties[i]);

                    }
                    property = Properties.FirstOrDefault(x => x.Name.Equals(propertyName));
                }
                else
                {
                    property = properties.FirstOrDefault();
                }

                if (property == null)
                {
                    property = new EntityProperty();
                    property.SetValue(propertyName, propertyValue);
                    Properties.Add(property);
                }
                else
                {
                    property.SetValue(propertyName, propertyValue);
                }
                LastUpdate = DateTime.Now;
            }
        }

        public void SetProperty<T>(PropertiesEnum property, T propertyValue)
        {
            
                SetProperty(property.ToString(), propertyValue);
            LastUpdate = DateTime.Now;
        }
        public T? GetProperty<T>(string propertyName)
        {
            lock (propertyLock)
            {
                var properties = Properties.Where(x => x.Name.Equals(propertyName)).ToList();
                EntityProperty? property = null;
                if (properties.Count > 1)
                {
                    for (int i = 1; i < properties.Count; i++)
                    { //keep first property[0];=
                        Properties.Remove(properties[i]);

                    }
                    property = Properties.FirstOrDefault(x => x.Name.Equals(propertyName));
                }
                else
                {
                    property = properties.FirstOrDefault();
                }

                if (property != null)
                {
                    return property.GetValue<T>();
                }
            }
            return default(T?);
        }

        public T? GetProperty<T>(PropertiesEnum property)
        {
            return GetProperty<T>(property.ToString());
        }


        public T? GetProperty<T>(BehaviorValue behaviorValue)
        {
            if (!string.IsNullOrEmpty(behaviorValue.BehaviorEntityTypeName) && !string.IsNullOrEmpty(behaviorValue.Name))
            {
                return GetBehaviorValue<T>(behaviorValue.BehaviorEntityTypeName, behaviorValue.Name);
            }
            
            return default(T);
        }
        public bool IsProperty(string propertyName)
        {
            lock (propertyLock)
            {
                bool b = Properties.Any(x => x.Name.Equals(propertyName));
                return b;
            }

        }

        public bool IsProperty(PropertiesEnum property)
        {
            return IsProperty(property.ToString());
        }


    }
}
