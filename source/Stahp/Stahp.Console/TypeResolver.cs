
using Spectre.Console.Cli;

namespace Stahp.Console
{
    internal class TypeResolver : ITypeResolver
    {
        private IServiceProvider _serviceProvider;

        public TypeResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? Resolve(Type? type)
        {
            return type is null 
                ? null
                : _serviceProvider.GetService(type);
        }
    }
}