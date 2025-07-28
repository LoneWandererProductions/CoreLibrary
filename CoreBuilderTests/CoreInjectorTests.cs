/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/CoreInjectorTests.cs
 * PURPOSE:     Some basic tests for our Injector
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Reflection;
using CoreInject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreBuilderTests;

/// <summary>
///     Part of my Dependency Injection Journey
/// </summary>
[TestClass]
public class CoreInjectorTests
{
    /// <summary>
    ///     The injector
    /// </summary>
    private CoreInjector _injector;

    /// <summary>
    ///     Setups this instance.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _injector = new CoreInjector();
    }

    /// <summary>
    ///     Registers the singleton should create single instance.
    /// </summary>
    [TestMethod]
    public void RegisterSingletonShouldCreateSingleInstance()
    {
        _injector.RegisterSingleton<IService, Service>();

        var firstInstance = _injector.Resolve<IService>();
        var secondInstance = _injector.Resolve<IService>();

        Assert.AreSame(firstInstance, secondInstance, "Singleton instances should be the same.");
    }

    /// <summary>
    ///     Registers the transient should create different instances.
    /// </summary>
    [TestMethod]
    public void RegisterTransientShouldCreateDifferentInstances()
    {
        _injector.RegisterTransient<IService, Service>();

        var firstInstance = _injector.Resolve<IService>();
        var secondInstance = _injector.Resolve<IService>();

        Assert.AreNotSame(firstInstance, secondInstance, "Transient instances should be different.");
    }

    /// <summary>
    ///     Registers the instance should use registered instance.
    /// </summary>
    [TestMethod]
    public void RegisterInstanceShouldUseRegisteredInstance()
    {
        var mockService = new MockService();
        _injector.RegisterInstance<IService>(mockService);

        var resolvedService = _injector.Resolve<IService>();

        Assert.AreSame(mockService, resolvedService, "Instance should be the same as the registered instance.");
    }

    /// <summary>
    ///     Registers the scoped should maintain scope within scope.
    /// </summary>
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

    /// <summary>
    ///     Resolves the unregistered service should throw exception.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ResolveUnregisteredServiceShouldThrowException()
    {
        _injector.Resolve<IService>(); // Should throw Exception as IService is not registered
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RegisterScopedWithoutScopeShouldThrowException()
    {
        _injector.RegisterScoped<IService, Service>(); // No active scope
        _injector.Resolve<IService>(); // Should throw InvalidOperationException
    }

    // --- New Tests for RegisterFromAssembly and TryResolve ---

    /// <summary>
    ///     Registers from assembly should register services correctly.
    /// </summary>
    [TestMethod]
    public void RegisterFromAssemblyShouldRegisterServicesCorrectly()
    {
        // Arrange: Create a dummy assembly with implementations
        var assembly = Assembly.GetExecutingAssembly(); // Use current assembly for testing

        // Register from assembly
        _injector.RegisterFromAssembly(assembly);

        // Act: Resolve the service
        var service = _injector.Resolve<IService>();

        // Assert: Ensure the registered service was correctly resolved
        Assert.IsNotNull(service, "Service should be resolved.");
    }

    [TestMethod]
    public void TryResolveShouldReturnNullForUnregisteredService()
    {
        // Act: Try to resolve an unregistered service
        var service = _injector.TryResolve<IService>();

        // Assert: Ensure that null is returned for an unregistered service
        Assert.IsNull(service, "TryResolve should return null for unregistered services.");
    }

    [TestMethod]
    public void TryResolveShouldReturnResolvedService()
    {
        // Arrange: Register a service
        _injector.RegisterTransient<IService, Service>();

        // Act: Try to resolve the registered service
        var service = _injector.TryResolve<IService>();

        // Assert: Ensure that the service is resolved successfully
        Assert.IsNotNull(service, "TryResolve should return the resolved service.");
    }
}

/// <summary>
///     Mock classes for testing
/// </summary>
public interface IService
{
    void DoStuff();
}

/// <inheritdoc />
/// <summary>
///     Concrete service implementation for testing
/// </summary>
public sealed class Service : IService
{
    public void DoStuff() { }
}

/// <summary>
///     Mock service for testing
/// </summary>
public sealed class MockService : IService
{
    public void DoStuff() { }
}
