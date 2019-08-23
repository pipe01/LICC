using LICC.API;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Test
{
    public class ShellTest
    {
        private static ICommandRegistryInternal RegistryWithCommand(string name, Action action)
        {
            var cmd = new Command(name, null, action.Method);
            var registryMock = new Mock<ICommandRegistryInternal>();
            registryMock.Setup(o => o.TryGetCommand(name, out cmd, It.IsAny<bool>())).Returns(true);

            return registryMock.Object;
        }

        [Test]
        public void NoParameters()
        {
            var converterMock = new Mock<IValueConverter>();
            converterMock.Setup(o => o.TryConvertValue(It.IsAny<Type>(), It.IsAny<string>())).Returns((true, null));

            var frontendMock = new Mock<Frontend>();
            LConsole.Frontend = frontendMock.Object;

            var shell = new Shell(converterMock.Object, new History(), null, RegistryWithCommand("test", Assert.Pass));

            shell.ExecuteLine("test");

            Assert.Fail("Command not called");
        }
    }
}
