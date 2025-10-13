using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ShowRichTextAttribute : PropertyAttribute { }
}