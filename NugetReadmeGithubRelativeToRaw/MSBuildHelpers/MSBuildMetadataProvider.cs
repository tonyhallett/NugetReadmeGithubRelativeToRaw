using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;

namespace NugetReadmeGithubRelativeToRaw.MSBuildHelpers
{
    internal class MSBuildMetadataProvider : IMSBuildMetadataProvider
    {
        internal class RequiredPropertyInfo
        {
            private readonly PropertyInfo propertyInfo;

            public RequiredPropertyInfo(PropertyInfo propertyInfo, bool isRequired = true)
            {
                this.propertyInfo = propertyInfo;
                IsRequired = isRequired;
            }

            public string Name => propertyInfo.Name;

            public bool IsRequired { get; }

            public void SetValue(object obj, object value) => propertyInfo.SetValue(obj, value);
        }

        private static readonly Dictionary<Type, List<RequiredPropertyInfo>> properties = new Dictionary<Type, List<RequiredPropertyInfo>>();

        private static List<RequiredPropertyInfo> GetRequiredProperties(Type type)
        {
            if (!properties.TryGetValue(type, out var requiredProps))
            {
                var properties = type.GetProperties().Where(p => p.GetCustomAttribute<IgnoreMetadataAttribute>() == null);
                if(typeof(IRequiredMetadata).IsAssignableFrom(type))
                {
                    requiredProps = properties.Select(p =>
                        {
                            var isRequired = p.GetCustomAttribute<RequiredMetadataAttribute>() != null;
                            return new RequiredPropertyInfo(p, isRequired);
                        }).ToList();
                }
                else
                {
                    requiredProps = properties.Select(p => new RequiredPropertyInfo(p, false)).ToList();
                }
            }

            return requiredProps;
        }

        public T GetCustomMetadata<T>(ITaskItem item) where T : new()
        {
            T metadata = new T();

            foreach (var requiredPropertyInfo in GetRequiredProperties(typeof(T)))
            {
                var metadataValue = item.GetMetadata(requiredPropertyInfo.Name);

                requiredPropertyInfo.SetValue(metadata, metadataValue);

                if (requiredPropertyInfo.IsRequired && string.IsNullOrEmpty(metadataValue) && metadata is IRequiredMetadata requiredMetadata)
                {
                    requiredMetadata.AddMissingMetadataName(requiredPropertyInfo.Name);
                }
            }
            return metadata;
        }
    }
}
