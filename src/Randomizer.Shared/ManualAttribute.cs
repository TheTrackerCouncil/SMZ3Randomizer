using System;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared;

[AttributeUsage(AttributeTargets.Class,
    Inherited = false, AllowMultiple = false)]
public class ManualAttribute : Attribute
{
}
