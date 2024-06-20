using NSubstitute;

using Stahp.Core.HostTypes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stahp.Core.Tests
{
    internal static class Mocks
    {
        private static IHostFactory? _hostFactory;

        internal static IHostFactory HostFactory
        {
            get
            {
                if (_hostFactory is null)
                {
                    _hostFactory = Substitute.For<IHostFactory>();
                    _hostFactory.GetHost(Arg.Any<Uri>())
                        .Returns(callInfo =>
                        {
                            var host = Substitute.For<IHost>();

                            host.HostName.Returns(callInfo.Arg<Uri>().Host);

                            host.HostUrl.Returns(callInfo.Arg<Uri>().ToString());

                            return Task.FromResult(host);
                        });
                }
                return _hostFactory;
            }
        }
    }
}
