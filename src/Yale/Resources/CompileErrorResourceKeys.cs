namespace Yale.Resources
{
    /// <summary>
    /// Resource keys for compile error messages
    /// </summary>
    /// <remarks></remarks>
    internal static class CompileErrorResourceKeys
    {
        public const string CouldNotResolveType = "CouldNotResolveType";
        public const string CannotConvertType = "CannotConvertType";
        public const string FirstArgNotBoolean = "FirstArgNotBoolean";
        public const string NeitherArgIsConvertibleToTheOther = "NeitherArgIsConvertibleToTheOther";
        public const string ValueNotRepresentableInType = "ValueNotRepresentableInType";
        public const string SearchArgIsNotKnownCollectionType = "SearchArgIsNotKnownCollectionType";
        public const string OperandNotConvertibleToCollectionType = "OperandNotConvertibleToCollectionType";
        public const string TypeNotArrayAndHasNoIndexerOfType = "TypeNotArrayAndHasNoIndexerOfType";
        public const string ArrayIndexersMustBeOfType = "ArrayIndexersMustBeOfType";
        public const string AmbiguousCallOfFunction = "AmbiguousCallOfFunction";
        public const string NamespaceCannotBeUsedAsType = "NamespaceCannotBeUsedAsType";
        public const string TypeCannotBeUsedAsAnExpression = "TypeCannotBeUsedAsAnExpression";
        public const string StaticMemberCannotBeAccessedWithInstanceReference = "StaticMemberCannotBeAccessedWithInstanceReference";
        public const string ReferenceToNonSharedMemberRequiresObjectReference = "ReferenceToNonSharedMemberRequiresObjectReference";
        public const string FunctionHasNoReturnValue = "FunctionHasNoReturnValue";
        public const string OperationNotDefinedForType = "OperationNotDefinedForType";
        public const string OperationNotDefinedForTypes = "OperationNotDefinedForTypes";
        public const string CannotConvertTypeToExpressionResult = "CannotConvertTypeToExpressionResult";
        public const string AmbiguousOverloadedOperator = "AmbiguousOverloadedOperator";
        public const string NoIdentifierWithName = "NoIdentifierWithName";
        public const string NoIdentifierWithNameOnType = "NoIdentifierWithNameOnType";
        public const string IdentifierIsAmbiguous = "IdentifierIsAmbiguous";
        public const string IdentifierIsAmbiguousOnType = "IdentifierIsAmbiguousOnType";
        public const string CannotReferenceCalcEngineAtomWithoutCalcEngine = "CannotReferenceCalcEngineAtomWithoutCalcEngine";
        public const string CalcEngineDoesNotContainAtom = "CalcEngineDoesNotContainAtom";
        public const string UndefinedFunction = "UndefinedFunction";
        public const string UndefinedFunctionOnType = "UndefinedFunctionOnType";
        public const string NoAccessibleMatches = "NoAccessibleMatches";
        public const string NoAccessibleMatchesOnType = "NoAccessibleMatchesOnType";
        public const string CannotParseType = "CannotParseType";
        public const string MultiArrayIndexNotSupported = "MultiArrayIndexNotSupported";

        // Grammatica
        public const string UnexpectedToken = "UNEXPECTED_TOKEN";

        public const string IO = "IO";
        public const string UnexpectedEof = "UNEXPECTED_EOF";
        public const string UnexpectedChar = "UNEXPECTED_CHAR";
        public const string InvalidToken = "INVALID_TOKEN";
        public const string Analysis = "ANALYSIS";
        public const string LineColumn = "LineColumn";
        public const string SyntaxError = "SyntaxError";
    }
}