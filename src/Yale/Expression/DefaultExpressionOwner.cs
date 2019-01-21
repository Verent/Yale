using System;

namespace Yale.Expression
{
    /// <summary>
    //  The dynamic method inherits its security from the type to which it's attached.
    //  Methods that you don't attach to a type are attached to a security transparent assembly.
    //  You can't add an attribute to a DynamicMethod overriding the security -- you have to attach it to an appropriate type.
    //  https://stackoverflow.com/questions/3247471/how-can-i-make-my-dynamicmethod-security-critical
    /// </summary>
    internal class DefaultExpressionOwner
    {
        public static object Instance { get; } = new DefaultExpressionOwner();
        public static Type Type => typeof(DefaultExpressionOwner);
    }
}