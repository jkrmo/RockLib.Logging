﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RockLib.Logging.DependencyInjection;
using RockLib.Logging.LogProcessing;
using System;
using Xunit;

namespace RockLib.Logging.Tests.DependencyInjection;

public static class ServiceCollectionExtensionsTests
{
    private static readonly IServiceProvider _emptyServiceProvider = new ServiceCollection().BuildServiceProvider();

    [Fact(DisplayName = "AddLogger method 1 adds the correct service descriptors")]
    public static void AddLoggerMethod()
    {
        var services = new ServiceCollection();
        var logProcessor = new Mock<ILogProcessor>().Object;

        var builder = services.AddLogger(logProcessor,
            configureOptions: options => options.Level = LogLevel.Info,
            lifetime: ServiceLifetime.Scoped);

        services.Should().HaveCount(3);

        services[0].ServiceType.Should().Be<ILogProcessor>();
        services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        services[0].ImplementationInstance.Should().BeSameAs(logProcessor);

        services[1].ServiceType.Should().Be<ILogger>();
        services[1].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[1].ImplementationFactory!.Target.Should().BeSameAs(builder);

        services[2].ServiceType.Should().Be<LoggerLookup>();
        services[2].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[2].ImplementationFactory.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddLogger method 1 throws when services parameter is null")]
    public static void AddLoggerMethodWhenServicesIsNull()
    {
        var logProcessor = new Mock<ILogProcessor>().Object;

        var act = () => (null as IServiceCollection)!.AddLogger(logProcessor);

        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*services*");
    }

    [Fact(DisplayName = "AddLogger method 1 throws when logProcessor parameter is null")]
    public static void AddLoggerMethodWhenProcessorIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddLogger((null as ILogProcessor)!);

        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*logProcessor*");
    }

    [Fact(DisplayName = "AddLogger method 2 adds the correct service descriptors")]
    public static void AddLoggerMethodWithDescriptors()
    {
        var services = new ServiceCollection();
        var logProcessor = new Mock<ILogProcessor>().Object;
        ILogProcessor LogProcessorRegistration(IServiceProvider serviceProvider) => logProcessor;

        var builder = services.AddLogger(LogProcessorRegistration,
            configureOptions: options => options.Level = LogLevel.Info,
            lifetime: ServiceLifetime.Scoped);

        services.Should().HaveCount(3);

        services[0].ServiceType.Should().Be<ILogProcessor>();
        services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        services[0].ImplementationFactory!.Invoke(_emptyServiceProvider).Should().BeSameAs(logProcessor);

        services[1].ServiceType.Should().Be<ILogger>();
        services[1].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[1].ImplementationFactory!.Target.Should().BeSameAs(builder);

        services[2].ServiceType.Should().Be<LoggerLookup>();
        services[2].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[2].ImplementationFactory.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddLogger method 2 throws when services parameter is null")]
    public static void AddLoggerMethodWithDescriptorsWhenServicesIsNull()
    {
        var logProcessor = new Mock<ILogProcessor>().Object;
        ILogProcessor LogProcessorRegistration(IServiceProvider serviceProvider) => logProcessor;

        var act = () => (null as IServiceCollection)!.AddLogger(LogProcessorRegistration);

        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*services*");
    }

    [Fact(DisplayName = "AddLogger method 2 throws when logProcessorRegistration parameter is null")]
    public static void AddLoggerMethodWithDescriptorsWhenRegistrationIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddLogger((null as Func<IServiceProvider, ILogProcessor>)!);

        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*logProcessorRegistration*");
    }

    [Fact(DisplayName = "AddLogger method 3 adds the correct service descriptors for ProcessingMode.Background")]
    public static void AddLoggerMethodWithDescriptorsInBackground()
    {
        var services = new ServiceCollection();

        var builder = services.AddLogger(configureOptions: options => options.Level = LogLevel.Info,
            processingMode: Logger.ProcessingMode.Background,
            lifetime: ServiceLifetime.Scoped);

        services.Should().HaveCount(3);

        services[0].ServiceType.Should().Be<ILogProcessor>();
        services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        services[0].ImplementationType.Should().Be<BackgroundLogProcessor>();

        services[1].ServiceType.Should().Be<ILogger>();
        services[1].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[1].ImplementationFactory!.Target.Should().BeSameAs(builder);

        services[2].ServiceType.Should().Be<LoggerLookup>();
        services[2].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[2].ImplementationFactory.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddLogger method 3 adds the correct service descriptors for ProcessingMode.FireAndForget")]
    public static void AddLoggerMethodWithDescriptorsInFireAndForget()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618 // Type or member is obsolete
        var builder = services.AddLogger(configureOptions: options => options.Level = LogLevel.Info,
            processingMode: Logger.ProcessingMode.FireAndForget,
            lifetime: ServiceLifetime.Scoped);
#pragma warning restore CS0618 // Type or member is obsolete

        services.Should().HaveCount(3);

        services[0].ServiceType.Should().Be<ILogProcessor>();
        services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
#pragma warning disable CS0618 // Type or member is obsolete
        services[0].ImplementationType.Should().Be<FireAndForgetLogProcessor>();
#pragma warning restore CS0618 // Type or member is obsolete

        services[1].ServiceType.Should().Be<ILogger>();
        services[1].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[1].ImplementationFactory!.Target.Should().BeSameAs(builder);

        services[2].ServiceType.Should().Be<LoggerLookup>();
        services[2].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[2].ImplementationFactory.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddLogger method 3 adds the correct service descriptors for ProcessingMode.Synchronous")]
    public static void AddLoggerMethod3HAddLoggerMethodWithDescriptorsInSynchronous()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618 // Type or member is obsolete
        var builder = services.AddLogger(configureOptions: options => options.Level = LogLevel.Info,
            processingMode: Logger.ProcessingMode.Synchronous,
            lifetime: ServiceLifetime.Scoped);
#pragma warning restore CS0618 // Type or member is obsolete

        services.Should().HaveCount(3);

        services[0].ServiceType.Should().Be<ILogProcessor>();
        services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
#pragma warning disable CS0618 // Type or member is obsolete
        services[0].ImplementationType.Should().Be<SynchronousLogProcessor>();
#pragma warning restore CS0618 // Type or member is obsolete

        services[1].ServiceType.Should().Be<ILogger>();
        services[1].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[1].ImplementationFactory!.Target.Should().BeSameAs(builder);

        services[2].ServiceType.Should().Be<LoggerLookup>();
        services[2].Lifetime.Should().Be(ServiceLifetime.Scoped);
        services[2].ImplementationFactory.Should().NotBeNull();
    }

    [Fact(DisplayName = "AddLogger method 3 throws when services parameter is null")]
    public static void AddLoggerMethodWithDescriptorsInBackgroundWhenServicesIsNull()
    {
        var processingMode = Logger.ProcessingMode.Background;

        var act = () => (null as IServiceCollection)!.AddLogger(processingMode: processingMode);

        act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*services*");
    }

    [Fact(DisplayName = "AddLogger method 3 throws when processingMode parameter is out of range")]
    public static void AddLoggerMethodWithDescriptorsWhenProcessingModeIsIncorrect()
    {
        var services = new ServiceCollection();
        var processingMode = (Logger.ProcessingMode)123;
     
        var act = () => services.AddLogger(processingMode: processingMode);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("*processingMode*");
    }
}
