using System.Buffers;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Implementations;

namespace Tiempito.IPC;

/// <summary>
/// Provides extension methods for registering IPC-related services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers IPC-related services into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add IPC services to.</param>
    public static void AddIpc(this IServiceCollection services)
    {
        services.AddSingleton(_ => MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
        services.AddSingleton<IMessageSerializer, MsgPackMessageSerializer>();
        services.AddSingleton(_ => ArrayPool<byte>.Shared);
        services.AddSingleton<MessageHandler>();
        services.AddSingleton<IMessageWriter>(sp => sp.GetRequiredService<MessageHandler>());
        services.AddSingleton<IMessageReader>(sp => sp.GetRequiredService<MessageHandler>());
    }
}