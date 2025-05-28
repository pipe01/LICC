using LICC.API;
using LICC.Internal;
using Moq;
using NUnit.Framework;
using System;
using System.Reflection;

namespace LICC.Test
{
    public class ShellTest
    {
        private static ICommandRegistryInternal RegistryWithCommand(string name, string methodName)
            => RegistryWithCommand(name, typeof(ShellTest).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static));

        private static ICommandRegistryInternal RegistryWithCommand(string name, Action action)
            => RegistryWithCommand(name, action.Method);

        private static ICommandRegistryInternal RegistryWithCommand(string name, MethodInfo method)
        {
            var cmd = new Command(name, null, method);
            var registryMock = new Mock<ICommandRegistryInternal>();
            registryMock.Setup(o => o.TryGetCommand(name, method.GetParameters().Length, out cmd, It.IsAny<bool>())).Returns(true);

            return registryMock.Object;
        }

        
        [OneTimeSetUp]
        public static void Setup()
        {
            LConsole.Frontend = new Mock<Frontend>().Object;
        }

        [Test]
        public void NoParameters()
        {
            var converterMock = new Mock<IValueConverter>();
            converterMock.Setup(o => o.TryConvertValue(It.IsAny<Type>(), It.IsAny<string>())).Returns((true, null));

            var shell = new Shell(converterMock.Object, new History(), null, RegistryWithCommand("test", Assert.Pass), null, new CommandExecutor(null));

            shell.ExecuteLine("test");

            Assert.Fail("Command not called");
        }

        [Test]
        public void OneIntParameter()
        {
            var converterMock = new Mock<IValueConverter>();
            converterMock.Setup(o => o.TryConvertValue(typeof(int), "123")).Returns((true, 123));

            var shell = new Shell(converterMock.Object, new History(), null, RegistryWithCommand("test", nameof(OneIntParameter_Callback)), null, new CommandExecutor(null));

            shell.ExecuteLine("test 123");

            Assert.Fail("Command not called");
        }

        private static void OneIntParameter_Callback(int number)
        {
            Assert.AreEqual(123, number);
            Assert.Pass();
        }

        [Test]
        public void OneUnquotedStringParameter()
        {
            var converterMock = new Mock<IValueConverter>();
            converterMock.Setup(o => o.TryConvertValue(typeof(string), It.IsAny<string>())).Returns((Type _, string str) => (true, str));

            var shell = new Shell(converterMock.Object, new History(), null, RegistryWithCommand("test", nameof(OneUnquotedStringParameter_Callback)), null, new CommandExecutor(null));

            shell.ExecuteLine("test testing string");

            Assert.Fail("Command not called");
        }

        private static void OneUnquotedStringParameter_Callback(string text)
        {
            Assert.AreEqual("testing string", text);
            Assert.Pass();
        }
    }
}
