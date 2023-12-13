using System;
using System.Diagnostics;
using System.Reflection;
using Yale.Expression.Elements.Base;

namespace Yale.Parser.Internal
{
    /// <summary>
    /// "Helper class to resolve overloads"
    /// </summary>
    internal class CustomMethodInfo : IComparable<CustomMethodInfo>, IEquatable<CustomMethodInfo>
    {
        /// <summary>
        /// The rating of how close the method matches the given arguments (0 is best)
        /// </summary>
        private float score;

        public bool IsParamArray;
        public Type[]? FixedArgTypes;
        public Type[]? ParamArrayArgTypes;
        public MethodInfo Target { get; }

        public Type? ParamArrayElementType;

        public CustomMethodInfo(MethodInfo target)
        {
            Target = target;
        }

        public void ComputeScore(Type[] argTypes)
        {
            ParameterInfo[] parameters = Target.GetParameters();

            if (parameters.Length == 0)
            {
                score = 0.0F;
            }
            else if (IsParamArray)
            {
                score = ComputeScoreForParamArray(parameters, argTypes);
            }
            else
            {
                score = ComputeScoreInternal(parameters, argTypes);
            }
        }

        /// <summary>
        /// Compute a score showing how close our method matches the given argument types
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        private float ComputeScoreInternal(ParameterInfo[] parameters, Type[] argTypes)
        {
            // Our score is the average of the scores of each parameter.  The lower the score, the better the match.
            int sum = ComputeSum(parameters, argTypes);

            return sum / argTypes.Length;
        }

        private static int ComputeSum(ParameterInfo[] parameters, Type[] argTypes)
        {
            Debug.Assert(parameters.Length == argTypes.Length);

            int sum = 0;
            for (int i = 0; i <= parameters.Length - 1; i++)
            {
                sum += ImplicitConverter.GetImplicitConvertScore(
                    argTypes[i],
                    parameters[i].ParameterType
                );
            }

            return sum;
        }

        private float ComputeScoreForParamArray(ParameterInfo[] parameters, Type[] argTypes)
        {
            ParameterInfo paramArrayParameter = parameters[parameters.Length - 1];
            int fixedParameterCount = paramArrayParameter.Position;
            ParameterInfo[] fixedParameters = new ParameterInfo[fixedParameterCount];

            Array.Copy(parameters, fixedParameters, fixedParameterCount);

            int fixedSum = ComputeSum(fixedParameters, FixedArgTypes);
            Type paramArrayElementType = paramArrayParameter.ParameterType.GetElementType();
            int paramArraySum = 0;

            foreach (Type argType in ParamArrayArgTypes)
            {
                paramArraySum += ImplicitConverter.GetImplicitConvertScore(
                    argType,
                    paramArrayElementType
                );
            }

            float score =
                argTypes.Length > 0 ? (fixedSum + paramArraySum) / argTypes.Length : (float)0;

            // The param array score gets a slight penalty so that it scores worse than direct matches
            return score + 1;
        }

        public bool IsAccessible(MemberElement owner)
        {
            return MemberElement.IsMemberAccessible(Target);
        }

        /// <summary>
        /// Is the given MethodInfo usable as an overload?
        /// </summary>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        public bool IsMatch(Type[] argTypes)
        {
            ParameterInfo[] parameters = Target.GetParameters();

            // If there are no parameters and no arguments were passed, then we are a match.
            if (parameters.Length == 0 & argTypes.Length == 0)
            {
                return true;
            }

            // If there are no parameters but there are arguments, we cannot be a match
            if (parameters.Length == 0 & argTypes.Length > 0)
            {
                return false;
            }

            // Is the last parameter a paramArray?
            ParameterInfo lastParam = parameters[parameters.Length - 1];

            if (lastParam.IsDefined(typeof(ParamArrayAttribute), false) == false)
            {
                if (parameters.Length != argTypes.Length)
                {
                    // Not a paramArray and parameter and argument counts don't match
                    return false;
                }

                // Regular method call, do the test
                return AreValidArgumentsForParameters(argTypes, parameters);
            }

            // At this point, we are dealing with a paramArray call

            // If the parameter and argument counts are equal and there is an implicit conversion from one to the other, we are a match.
            if (
                parameters.Length == argTypes.Length
                && AreValidArgumentsForParameters(argTypes, parameters)
            )
            {
                return true;
            }

            if (IsParamArrayMatch(argTypes, parameters, lastParam))
            {
                IsParamArray = true;
                return true;
            }

            return false;
        }

        private bool IsParamArrayMatch(
            Type[] argTypes,
            ParameterInfo[] parameters,
            ParameterInfo paramArrayParameter
        )
        {
            // Get the count of arguments before the paramArray parameter
            int fixedParameterCount = paramArrayParameter.Position;
            Type[] fixedArgTypes = new Type[fixedParameterCount];
            ParameterInfo[] fixedParameters = new ParameterInfo[fixedParameterCount];

            // Get the argument types and parameters before the paramArray
            Array.Copy(argTypes, fixedArgTypes, fixedParameterCount);
            Array.Copy(parameters, fixedParameters, fixedParameterCount);

            // If the fixed arguments don't match, we are not a match
            if (AreValidArgumentsForParameters(fixedArgTypes, fixedParameters) == false)
            {
                return false;
            }

            // Get the type of the paramArray
            ParamArrayElementType = paramArrayParameter.ParameterType.GetElementType();

            // Get the types of the arguments passed to the paramArray
            Type[] paramArrayArgTypes = new Type[argTypes.Length - fixedParameterCount];
            Array.Copy(
                argTypes,
                fixedParameterCount,
                paramArrayArgTypes,
                0,
                paramArrayArgTypes.Length
            );

            // Check each argument
            foreach (Type argType in paramArrayArgTypes)
            {
                if (
                    ImplicitConverter.EmitImplicitConvert(argType, ParamArrayElementType, null)
                    == false
                )
                {
                    return false;
                }
            }

            FixedArgTypes = fixedArgTypes;
            ParamArrayArgTypes = paramArrayArgTypes;

            // They all match, so we are a match
            return true;
        }

        private static bool AreValidArgumentsForParameters(
            Type[] argTypes,
            ParameterInfo[] parameters
        )
        {
            Debug.Assert(argTypes.Length == parameters.Length);
            // Match if every given argument is implicitly convertible to the method's corresponding parameter
            for (int i = 0; i <= argTypes.Length - 1; i++)
            {
                if (
                    ImplicitConverter.EmitImplicitConvert(
                        argTypes[i],
                        parameters[i].ParameterType,
                        null
                    ) == false
                )
                {
                    return false;
                }
            }

            return true;
        }

        public int CompareTo(CustomMethodInfo other)
        {
            return score.CompareTo(other.score);
        }

        private bool Equals1(CustomMethodInfo other)
        {
            return score == other.score;
        }

        bool IEquatable<CustomMethodInfo>.Equals(CustomMethodInfo other)
        {
            return Equals1(other);
        }
    }
}
