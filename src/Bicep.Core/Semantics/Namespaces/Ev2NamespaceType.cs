// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Immutable;
using Bicep.Core.TypeSystem;
using Bicep.Core.TypeSystem.Ev2;

namespace Bicep.Core.Semantics.Namespaces
{
    public class Ev2NamespaceType
    {
        public const string BuiltInName = "ev2Extension";

        private static readonly IResourceTypeProvider TypeProvider = new Ev2ResourceTypeProvider(new Ev2ResourceTypeLoader());

        public static NamespaceSettings Settings { get; } = new(
            IsSingleton: false,
            BicepProviderName: BuiltInName,
            ConfigurationType: GetConfigurationType(),
            ArmTemplateProviderName: "ev2Extension",
            ArmTemplateProviderVersion: "1.0");

        private static ObjectType GetConfigurationType()
        {
            return new ObjectType("configuration", TypeSymbolValidationFlags.Default, new[]
            {
                new TypeProperty("namespace", LanguageConstants.String, TypePropertyFlags.Required),
                new TypeProperty("ev2Config", LanguageConstants.String, TypePropertyFlags.Required),
                new TypeProperty("context", LanguageConstants.String),
            }, null);
        }

        public static NamespaceType Create(string aliasName)
        {
            return new NamespaceType(
                aliasName,
                Settings,
                ImmutableArray<TypeProperty>.Empty,
                ImmutableArray<FunctionOverload>.Empty,
                ImmutableArray<BannedFunction>.Empty,
                ImmutableArray<Decorator>.Empty,
                TypeProvider);
        }
    }
}
