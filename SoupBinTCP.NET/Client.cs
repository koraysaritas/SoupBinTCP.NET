﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging.Console;
using SoupBinTCP.NET.Messages;

namespace SoupBinTCP.NET
{
    public class Client
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private IChannel _clientChannel;
        private readonly IClientListener _listener;
        private readonly LoginDetails _loginDetails;

        public Client(IPAddress ipAddress, int port, LoginDetails loginDetails, IClientListener listener)
        {
            if(port < 0) throw new ArgumentException("Invalid port number", nameof(port));
            _ipAddress = ipAddress;
            _port = port;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _listener = listener;
            InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => true, false));
        }
        
        public void Start()
        {
            Task.Run(RunClientAsync);
        }
        
        private async Task RunClientAsync()
        {
            var group = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;
                        pipeline.AddLast(new LoggingHandler(LogLevel.DEBUG));
                        //pipeline.AddLast(new IdleStateHandler(15, 1, 0));
                        pipeline.AddLast(new LengthFieldBasedFrameDecoder(ByteOrder.BigEndian, ushort.MaxValue, 0, 2,
                            0, 0, true));
                        pipeline.AddLast(new LengthFieldPrepender(ByteOrder.BigEndian, 2, 0, false));
                        pipeline.AddLast(new SoupBinTcpMessageDecoder());
                        //pipeline.AddLast(new ClientTimeoutHandler());
                        pipeline.AddLast(new ClientHandler());
                    }));
                _clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(_ipAddress, _port));

                _cancellationToken.WaitHandle.WaitOne();

                await _clientChannel.CloseAsync();
            }
            finally
            {
                await group.ShutdownGracefullyAsync();
            }
        }

        public async Task Send(Message message)
        {
            // TODO guard against sending when client isn't connected
            await _clientChannel.WriteAndFlushAsync(message);
        }

        public async Task Shutdown()
        {
            await _clientChannel.WriteAndFlushAsync(new LogoutRequest());
            _cancellationTokenSource.Cancel();
        }
        
    }
}