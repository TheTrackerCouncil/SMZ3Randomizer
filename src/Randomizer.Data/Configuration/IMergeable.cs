using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration
{
    /// <summary>
    /// Interface for classes where the data can be merged
    /// between two instances of the object
    /// </summary>
    /// <typeparam name="T">
    /// The type of data to be merged. By default, if the type
    /// matches the class it will merge the properties. If the
    /// type does not match the class, it will treat it like a
    /// list of that other type.
    /// </typeparam>
    public interface IMergeable<T>
    {
        /// <summary>
        /// Merges the data from the other object into the current instance
        /// </summary>
        /// <param name="other">The object to be merged into this one</param>
        public void MergeFrom(IMergeable<T> other)
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

        private static void MergeLists(IMergeable<T> first, IMergeable<T>? second)
        {
            if (second == null)
                return;

            var classType = first.GetType();
            var type = typeof(T);

            var keyProp = type.GetProperties()
                .SingleOrDefault(x => x.GetCustomAttributes(false).OfType<MergeKeyAttribute>().SingleOrDefault() != null);

            var listOne = (List<T>)first;
            var listTwo = (List<T>)second;

            if (listOne == null)
            {
                throw new ArgumentException($"Type {classType.Name} has a generic type of {type.Name} specified but {classType.Name} is not a list of {type.Name}");
            }

            // If the first is empty or there is no merge key, simply add all the records from the second
            if (keyProp == null || (listOne.Count == 0 && listTwo != null))
            {
                listOne.AddRange(listTwo);
                return;
            }
            else if (listTwo == null || listTwo.Count == 0)
            {
                return;
            }

            foreach (var secondObj in listTwo)
            {
                if (secondObj == null) continue;

                var key = keyProp.GetValue(secondObj);

                if (key == null) continue;

                var firstObj = listOne.FirstOrDefault(x => key.Equals(keyProp.GetValue(x)));
                if (firstObj != null)
                {
                    var firstMergeable = (IMergeable<T>)firstObj;
                    var secondMergeable = (IMergeable<T>)secondObj;
                    firstMergeable.MergeFrom(secondMergeable);
                }
                else
                {
                    listOne.Add(secondObj);
                }
            }

        }

        private static void MergeProperties(IMergeable<T> primary, IMergeable<T> other)
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
                        foreach (var (key, text) in otherValue)
                        {
                            if (thisValue.ContainsKey(key) && thisValue[key] != null)
                            {
                                thisValue[key].MergeFrom(text);
                            }
                            else
                            {
                                thisValue[key] = text;
                            }
                        }
                    }
                    else if (thisValue == null)
                    {
                        property.SetValue(primary, otherValue);
                    }
                }
                else if (property.PropertyType == typeof(Dictionary<string, SchrodingersString>))
                {
                    var thisValue = (Dictionary<string, SchrodingersString>?)property.GetValue(primary);
                    var otherValue = (Dictionary<string, SchrodingersString>?)property.GetValue(other);

                    if (thisValue != null && otherValue != null)
                    {
                        foreach (var (key, text) in otherValue)
                        {
                            if (thisValue.ContainsKey(key) && thisValue[key] != null)
                            {
                                thisValue[key].MergeFrom(text);
                            }
                            else if (text != null)
                            {
                                thisValue[key] = text;
                            }
                        }
                    }
                    else if (thisValue == null)
                    {
                        property.SetValue(primary, otherValue);
                    }
                }
                else if (property.PropertyType == typeof(Dictionary<string, string>))
                {
                    var thisValue = (Dictionary<string, string>?)property.GetValue(primary);
                    var otherValue = (Dictionary<string, string>?)property.GetValue(other);

                    if (thisValue == null)
                    {
                        property.SetValue(primary, otherValue);
                    }
                    else if (otherValue != null)
                    {
                        foreach (var (key, value) in otherValue)
                        {
                            thisValue[key] = value;
                        }
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
                else if (property.PropertyType.GetInterfaces().Any(x => x.Name.StartsWith("IMergeable")))
                {
                    var thisValue = property.GetValue(primary);
                    var otherValue = property.GetValue(other);
                    var mergeMethod = property.PropertyType.GetMethod("MergeFrom");
                    if (mergeMethod == null)
                    {
                        var inter = property.PropertyType.GetInterfaces().First(x => x.Name.StartsWith("IMergeable"));
                        mergeMethod = inter.GetMethod("MergeFrom");
                    }

                    if (thisValue != null && otherValue != null)
                    {
                        var paramList = new object[] { otherValue };
                        mergeMethod?.Invoke(thisValue, paramList);
                    }
                    else if (thisValue == null)
                    {
                        property.SetValue(primary, otherValue);
                    }
                }
            }
        }
    }
}
