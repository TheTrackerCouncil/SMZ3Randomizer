using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    public interface IMergeableConfig<T>
    {
        public void Merge(IMergeableConfig<T> other)
        {
            if (GetType().IsSubclassOf(typeof(List<T>)))
            {
                MergeLists(this, other);
            }
            else
            {
                MergeProperties(this, other);
            }
        }

        public static void MergeLists(IMergeableConfig<T> first, IMergeableConfig<T>? second)
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

            var listOne = (List<T>)first;
            var listTwo = (List<T>)second;

            // If the first is empty, simply add all the records from the second
            if (listOne.Count == 0)
            {
                listOne.AddRange(listTwo);
                return;
            }

            foreach (var secondObj in listTwo)
            {
                var key = keyProp.GetValue(secondObj);
                var firstObj = listOne.FirstOrDefault(x => keyProp.GetValue(x).Equals(key));
                if (firstObj != null)
                {
                    var firstMergeable = (IMergeableConfig<T>)firstObj;
                    var secondMergeable = (IMergeableConfig<T>)secondObj;
                    firstMergeable.Merge(secondMergeable);
                }
                else
                {
                    listOne.Add(secondObj);
                }
            }

        }

        public static void MergeProperties<T>(IMergeableConfig<T> primary, IMergeableConfig<T> other)
        {
            if (primary.GetType() != other.GetType())
            {
                throw new ArgumentException(primary.GetType().Name + " cannot be merged with " + other.GetType().Name);
            }

            var properties = primary.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(Dictionary<int, SchrodingersString>))
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
                            else if (text != null)
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
                else if (property.PropertyType.GetInterfaces().Any(x => x.Name.StartsWith("IMergeableConfig")))
                {
                    var thisValue = property.GetValue(primary);
                    var otherValue = property.GetValue(other);
                    var mergeMethod = property.PropertyType.GetMethod("Merge");
                    if (mergeMethod == null)
                    {
                        var inter = property.PropertyType.GetInterfaces().First(x => x.Name.StartsWith("IMergeableConfig"));
                        mergeMethod = inter.GetMethod("Merge");
                    }

                    if (thisValue != null && otherValue != null)
                    {
                        var paramList = new object[] { otherValue };
                        mergeMethod.Invoke(thisValue, paramList);
                    }
                    else if (thisValue == null)
                    {
                        property.SetValue(primary, otherValue);
                    }

                    //Console.Write("Mergeable property " + property.Name);
                    /*var thisValue = (IMergeableConfig?)property.GetValue(primary);
                    var otherValue = (IMergeableConfig?)property.GetValue(other);
                    if (thisValue != null && otherValue != null)
                    {
                        thisValue.Merge(otherValue);
                    }
                    else if (otherValue != null)
                    {
                        property.SetValue(primary, otherValue);
                    }*/
                }
            }
        }
    }
}
