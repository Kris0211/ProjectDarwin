using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Darwin
{
    public class FusionSessionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] 
        private NetworkPrefabRef _spectatorPlayerPrefab;

        private NetworkRunner _runner;
        private readonly Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

        async void StartGame(GameMode mode, string sessionName)
        {
            Debug.Log($"Starting game as {(mode == GameMode.Client ? "CLIENT" : "HOST")}");
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            SceneRef scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            NetworkSceneInfo sceneInfo = new();
            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            StartGameArgs startArgs = new()
            {
                GameMode = mode,
                SessionName = sessionName,
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            };

            await _runner.StartGame(startArgs);
        }

        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(0, 10, 200, 40), "Host"))
                {
                    StartGame(GameMode.Host, "DevRoom");
                }

                if (GUI.Button(new Rect(0, 50, 200, 40), "Join"))
                {
                    StartGame(GameMode.Client, "DevRoom");
                }
            }
        }

    #region INetworkRunnerCallbacks
        public void OnConnectedToServer(NetworkRunner runner)
        {
            throw new NotImplementedException();
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            throw new NotImplementedException();
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            throw new NotImplementedException();
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            throw new NotImplementedException();
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            throw new NotImplementedException();
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData data = new();

            Keyboard currentKeyboard = Keyboard.current;

            if (currentKeyboard.wKey.isPressed)
                data.direction += Vector3.forward;

            if (currentKeyboard.sKey.isPressed)
                data.direction += Vector3.back;

            if (currentKeyboard.aKey.isPressed)
                data.direction += Vector3.left;

            if (currentKeyboard.dKey.isPressed)
                data.direction += Vector3.right;

            input.Set(data);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            throw new NotImplementedException();
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            throw new NotImplementedException();
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Only the host can spawn player avatars
            if (runner.IsServer)
            {
                Vector3 spawnPosition = new(player.RawEncoded % runner.Config.Simulation.PlayerCount * 3, 1, 0);
                NetworkObject networkPlayerObject = runner.Spawn(
                    _spectatorPlayerPrefab, 
                    spawnPosition, 
                    Quaternion.identity, 
                    player
                );

                _spawnedCharacters.Add(player, networkPlayerObject);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            throw new NotImplementedException();
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public void OnSceneLoadDone(NetworkRunner runner) {}

        public void OnSceneLoadStart(NetworkRunner runner) {}

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            throw new NotImplementedException();
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _spawnedCharacters.Clear();
        }
    #endregion
    }
}
