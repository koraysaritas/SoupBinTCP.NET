﻿using System;
using System.Threading.Tasks;
using SoupBinTCP.NET;

namespace Server
{
    public class ServerListener : IServerListener
    {
        public LoginStatus OnLoginRequest(string username, string password, string requestedSession = "",
            ulong requestedSequenceNumber = 0)
        {
            // TODO fix this
            //return new LoginStatus(false, RejectionReason.NotAuthorised);
            Console.WriteLine($"OnLoginRequest {requestedSequenceNumber}");
            return new LoginStatus(true);
        }

        public async Task OnServerListening()
        {
            Console.WriteLine($"OnServerListening ");
            await Task.FromResult(false);
        }

        public LoginStatus OnLoginRequest(string username, string password, string requestedSession, ulong requestedSequenceNumber,
            string channelId)
        {
            Console.WriteLine($"OnServerListening {channelId}");
            return new LoginStatus(true);
        }

        public async Task OnLogoutRequest(string channelId)
        {
            Console.WriteLine($"OnServerListening {channelId}");
            await Task.FromResult(false);
        }

        public async Task OnMessage(byte[] message, string channelId)
        {
            Console.WriteLine($"OnMessage {message.Length}");
            await Task.FromResult(false);

        }

        public async Task OnSessionEnd(string channelId)
        {
            Console.WriteLine($"OnSessionEnd {channelId}");
            await Task.FromResult(false);
        }

        public async Task OnServerDisconnect()
        {
            Console.WriteLine($"OnServerDisconnect");
            await Task.FromResult(false);
        }

        public Task OnLogout(string clientId)
        {
            Console.WriteLine($"OnLogout {clientId}");
            return Task.FromResult(false);
        }

        public Task OnDebug(string message, string clientId)
        {
            Console.WriteLine($"OnDebug {clientId}, message {message}");
            return Task.FromResult(false);
        }

        public Task OnSessionStart(string sessionId)
        {
            Console.WriteLine($"OnSessionStart {sessionId}");
            return Task.FromResult(false);
        }
    }
}
