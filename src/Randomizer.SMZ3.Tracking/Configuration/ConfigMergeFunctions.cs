using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    public class ConfigMergeFunctions
    {
        public static void MergeLists<T>(List<T> first, List<T>? second) where T : IMergeableConfig
        {
            if (second == null)
                return;

            var type = typeof(T);

            var keyProp = type.GetProperties()
                .SingleOrDefault(x => x.GetCustomAttributes(false).OfType<ConfigMergeKeyAttribute>().SingleOrDefault() != null);

            if (keyProp == null)
            {
                throw new ArgumentException("Type " + type.Name + " does not have a property with the ConfigMergeKeyAttribute");
            }

            // If the first is empty, simply add all the records from the second
            if (first.Count == 0)
            {
                first.AddRange(second);
                return;
            }

            foreach (var secondObj in second)
            {
                var key = keyProp.GetValue(secondObj);
                var firstObj = first.FirstOrDefault(x => keyProp.GetValue(x).Equals(key));
                if (firstObj != null)
                {
                    firstObj.Merge(secondObj);
                }
                else
                {
                    first.Add(secondObj);
                }
            }

        }

        public static void MergeProperties(IMergeableConfig primary, IMergeableConfig other)
        {
            if (primary.GetType() != other.GetType())
            {
                throw new ArgumentException(primary.GetType().Name + " cannot be merged with " + other.GetType().Name);
            }

            var properties = primary.GetType().GetProperties();

            foreach (var property in properties)
            {
                if(property.PropertyType == typeof(Dictionary<int, SchrodingersString>))
                {
                    var thisValue = (Dictionary<int, SchrodingersString>?)property.GetValue(primary);
                    var otherValue = (Dictionary<int, SchrodingersString>?)property.GetValue(other);

                    if (thisValue != null && otherValue != null)
                    {
                        foreach (var (amount, text) in otherValue)
                        {
                            if (thisValue.ContainsKey(amount) && thisValue[amount] != null)
                            {
                                thisValue[amount].Merge(text);
                            }
                            else if(text != null)
                            {
                                thisValue[amount] = text;
                            }
                        }
                    }
                    else if (thisValue == null)
                    {
                        property.SetValue(primary, otherValue);
                    }
                }
                else if (property.PropertyType == typeof(List<string>))
                {
                    var thisValue = (List<string>?)property.GetValue(primary);
                    var otherValue = (List<string>?)property.GetValue(other);

                    if (thisValue != null && otherValue != null)
                    {
                        thisValue.AddRange(otherValue.Where(x => !thisValue.Contains(x)));
                    }
                    else if (thisValue == null)
                    {
                        property.SetValue(primary, otherValue);
                    }
                }
                else if (typeof(IMergeableConfig).IsAssignableFrom(property.PropertyType))
                {
                    var thisValue = (IMergeableConfig?)property.GetValue(primary);
                    var otherValue = (IMergeableConfig?)property.GetValue(other);
                    if (thisValue != null && otherValue != null)
                    {
                        thisValue.Merge(otherValue);
                    }
                    else if(otherValue != null)
                    {
                        property.SetValue(primary, otherValue);
                    }
                }
            }
        }
    }
}
