/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/SimpleInjectorTests.cs
 * PURPOSE:     Tests for our small scale Injector Framework
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using CoreInject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class SimpleInjectorTests
    {
        private SimpleInjector _injector;

        [TestInitialize]
        public void Setup()
        {
            _injector = new SimpleInjector();
        }

        [TestMethod]
        public void RegisterSingletonShouldCreateSingleInstance()
        {
            _injector.RegisterSingleton<IService, Service>();

            var firstInstance = _injector.Resolve<IService>();
            var secondInstance = _injector.Resolve<IService>();

            Assert.AreSame(firstInstance, secondInstance, "Singleton instances should be the same.");
        }

        [TestMethod]
        public void RegisterTransientShouldCreateDifferentInstances()
        {
            _injector.RegisterTransient<IService, Service>();

            var firstInstance = _injector.Resolve<IService>();
            var secondInstance = _injector.Resolve<IService>();

            Assert.AreNotSame(firstInstance, secondInstance, "Transient instances should be different.");
        }

        [TestMethod]
        public void RegisterInstanceShouldUseRegisteredInstance()
        {
            var mockService = new MockService();
            _injector.RegisterInstance<IService>(mockService);

            var resolvedService = _injector.Resolve<IService>();

            Assert.AreSame(mockService, resolvedService, "Instance should be the same as the registered instance.");
        }

        [TestMethod]
        public void RegisterScopedShouldMaintainScopeWithinScope()
        {
            _injector.RegisterScoped<IService, Service>();

            _injector.BeginScope();
            var firstInstance = _injector.Resolve<IService>();
            var secondInstance = _injector.Resolve<IService>();

            Assert.AreSame(firstInstance, secondInstance, "Scoped instances should be the same within a scope.");

            _injector.EndScope();

            _injector.BeginScope();
            var thirdInstance = _injector.Resolve<IService>();

            Assert.AreNotSame(firstInstance, thirdInstance, "Scoped instances should be different between scopes.");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ResolveUnregisteredServiceShouldThrowException()
        {
            _injector.Resolve<IService>();  // Should throw Exception as IService is not registered
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RegisterScopedWithoutScopeShouldThrowException()
        {
            _injector.RegisterScoped<IService, Service>(); // No active scope
            _injector.Resolve<IService>();  // Should throw InvalidOperationException
        }
    }

    /// <summary>
    /// Mock classes for testing
    /// </summary>
    public interface IService
    {
        void DoStuff();
    }

    /// <summary>
    /// Mock classes for testing
    /// </summary>
    /// <seealso cref="CommonLibraryTests.IService" />
    public class Service : IService
    {
        public void DoStuff() { }
    }

    /// <summary>
    /// Mock classes for testing
    /// </summary>
    /// <seealso cref="CommonLibraryTests.IService" />
    public class MockService : IService
    {
        public void DoStuff() { }
    }
}
