using LiteNetwork.Protocol.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiteNetwork.Server.Abstractions
{
    /// <summary>
    /// Provides a mechanism to manage a TCP server.
    /// </summary>
    /// <typeparam name="TUser">The user type that the server will be use.</typeparam>
    public interface ILiteServer<TUser> : IDisposable
        where TUser : LiteServerUser
    {
        /// <summary>
        /// Gets a value that indicates if the server is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the server configuration.
        /// </summary>
        LiteServerOptions Options { get; }

        /// <summary>
        /// Gets a collection that contains all the connected <typeparamref name="TUser"/>.
        /// </summary>
        IEnumerable<TUser> ConnectedUsers { get; }

        /// <summary>
        /// Gets a connected <typeparamref name="TUser"/> associated with the specified id.
        /// </summary>
        /// <param name="userId">User id to get.</param>
        /// <returns>A <typeparamref name="TUser"/> with the specified id if the id has found;
        /// otherwise, null.</returns>
        TUser? GetUser(Guid userId);

        /// <summary>
        /// Attempts to get the <typeparamref name="TUser"/> associated with the specified id.
        /// </summary>
        /// <param name="userId">User id to get.</param>
        /// <param name="user">If the operation completed returns the user associated with the specified id,
        /// or null if the operaton failed.
        /// </param>
        /// <returns>True if the user id has found; otherwise, false.</returns>
        bool TryGetUser(Guid userId, out TUser? user);

        /// <summary>
        /// Starts to listening and accept users.
        /// </summary>
        void Start();

        /// <summary>
        /// Starts to listening and accept users asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="ILiteServer{TUser}"/> starts.</returns>
        Task StartAsync();

        /// <summary>
        /// Starts to listening and accept users asynchronously with the specified <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be successfully.</param>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="ILiteServer{TUser}"/> starts.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stop listening and disconnect all connectected users.
        /// </summary>
        void Stop();

        /// <summary>
        /// Attempt to stop the server asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="ILiteServer{TUser}"/> stops.</returns>
        Task StopAsync();

        /// <summary>
        /// Attempt to stop the server asynchronously with the specified <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be successfully.</param>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="ILiteServer{TUser}"/> stops.</returns>
        Task StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects an <typeparamref name="TUser"/> with the specified user id.
        /// </summary>
        /// <param name="userId">User id to disconnect.</param>
        void DisconnectUser(Guid userId);

        /// <summary>
        /// Send an <see cref="ILitePacketStream"/> to the given <typeparamref name="TUser"/>.
        /// </summary>
        /// <param name="user">Target user.</param>
        /// <param name="packet">Packet message to send.</param>
        void SendTo(TUser user, ILitePacketStream packet);

        /// <summary>
        /// Send an <see cref="ILitePacketStream"/> to a given collection of <typeparamref name="TUser"/>.
        /// </summary>
        /// <param name="users">Collection of <typeparamref name="TUser"/>.</param>
        /// <param name="packet">Packet message to send.</param>
        void SendTo(IEnumerable<TUser> users, ILitePacketStream packet);

        /// <summary>
        /// Send a <see cref="ILitePacketStream"/> to all connected <typeparamref name="TUser"/>.
        /// </summary>
        /// <param name="packet">Packet message data to send.</param>
        void SendToAll(ILitePacketStream packet);
    }
}
