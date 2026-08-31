using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay.Models;

public class MultiplayerDiagnostics : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int internetTimeoutSeconds = 5;

    [Header("Opcional")]
    [Tooltip("Executa automaticamente quando der Play.")]
    [SerializeField] private bool runOnStart = false;

    private async void Start()
    {
        if (runOnStart)
        {
            await RunAllTests();
        }
    }

    // Botão pelo menu de contexto do componente no Inspector
    [ContextMenu("Run Multiplayer Diagnostics")]
    private void RunFromContextMenu()
    {
        RunAllTests();
    }

    public async Task RunAllTests()
    {
        bool internetOk = await TestInternet();
        bool unityServicesOk = await TestUnityServices();
        bool authenticationOk = false;

        if (unityServicesOk)
        {
            authenticationOk = TestAuthentication();
        }
        else
        {
            Skip("AUTHENTICATION", "Unity Services não está inicializado.");
        }

        bool lobbyOk = false;

        if (internetOk && unityServicesOk && authenticationOk)
        {
            lobbyOk = await TestLobby();
        }
        else
        {
            Skip("LOBBY", "Internet, Unity Services ou Authentication falhou.");
        }

        bool relayOk = false;

        if (internetOk && unityServicesOk && authenticationOk)
        {
            relayOk = await TestRelay();
        }
        else
        {
            Skip("RELAY", "Internet, Unity Services ou Authentication falhou.");
        }

        bool ngoOk = TestNGO();

        Debug.Log("\n" +
                  "========================================\n" +
                  " MULTIPLAYER DIAGNOSTICS - RESULTADO\n" +
                  "========================================\n" +
                  $"Internet:          {Result(internetOk)}\n" +
                  $"Unity Services:    {Result(unityServicesOk)}\n" +
                  $"Authentication:    {Result(authenticationOk)}\n" +
                  $"Lobby Service:     {Result(lobbyOk)}\n" +
                  $"Relay Service:     {Result(relayOk)}\n" +
                  $"NGO / Transport:   {Result(ngoOk)}\n" +
                  "========================================"
        );
    }

    public async Task<bool> TestInternet()
    {
        Header("1 - INTERNET");

        Debug.Log($"[INTERNET] Reachability informado pela Unity: " + $"{Application.internetReachability}");

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Fail("INTERNET", "Application.internetReachability = NotReachable");

            return false;
        }

        try
        {
            using UnityWebRequest request = UnityWebRequest.Head("https://unity.com");

            request.timeout = internetTimeoutSeconds;

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Fail("INTERNET", $"Falha de conexão real: {request.error}");

                return false;
            }

            Pass("INTERNET", $"Conexão HTTP funcionando. HTTP {request.responseCode}");

            return true;
        }
        catch (Exception exception)
        {
            Fail("INTERNET", exception);

            return false;
        }
    }

    public async Task<bool> TestUnityServices()
    {
        Header("2 - UNITY GAMING SERVICES");

        try
        {
            Debug.Log($"[UGS] Estado antes do teste: {UnityServices.State}");

            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.Log("[UGS] Unity Services ainda não está inicializado. " + "Tentando InitializeAsync()...");

                await UnityServices.InitializeAsync();
            }

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Pass("UGS", $"Unity Services inicializado. State = {UnityServices.State}");

                return true;
            }

            Fail("UGS", $"Estado inesperado: {UnityServices.State}");

            return false;
        }
        catch (Exception exception)
        {
            Fail("UGS", exception);

            return false;
        }
    }

    public bool TestAuthentication()
    {
        Header("3 - AUTHENTICATION");

        try
        {
            bool signedIn = AuthenticationService.Instance.IsSignedIn;
            bool authorized = AuthenticationService.Instance.IsAuthorized;
            bool expired = AuthenticationService.Instance.IsExpired;

            Debug.Log($"[AUTH] IsSignedIn  = {signedIn}\n" + $"[AUTH] IsAuthorized = {authorized}\n" + $"[AUTH] IsExpired   = {expired}");

            if (!signedIn)
            {
                Fail("AUTH", "Não existe usuário autenticado.");

                return false;
            }

            if (expired)
            {
                Fail("AUTH", "O Access Token está expirado.");

                return false;
            }

            if (!authorized)
            {
                Fail("AUTH", "Existe sessão, mas o usuário NÃO está autorizado.");

                return false;
            }

            Pass("AUTH", $"Autenticado e autorizado. " + $"PlayerId = {AuthenticationService.Instance.PlayerId}");

            return true;
        }
        catch (Exception exception)
        {
            Fail("AUTH", exception);

            return false;
        }
    }

    public async Task<bool> TestLobby()
    {
        Header("4 - LOBBY SERVICE");

        try
        {
            Debug.Log("[LOBBY] Fazendo QueryLobbiesAsync...");

            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 1
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

            Pass("LOBBY", $"Lobby Service respondeu corretamente. " + $"Lobbies retornados: {response.Results.Count}");

            return true;
        }
        catch (Exception exception)
        {
            Fail("LOBBY", exception);

            return false;
        }
    }

    public async Task<bool> TestRelay()
    {
        Header("5 - RELAY SERVICE");

        try
        {
            Debug.Log("[RELAY] Solicitando lista de regiões...");

            List<Region> regions = await RelayService.Instance.ListRegionsAsync();

            if (regions == null)
            {
                Fail("RELAY", "ListRegionsAsync retornou null.");

                return false;
            }

            Pass("RELAY", $"Relay respondeu corretamente. " + $"Regiões disponíveis: {regions.Count}");

            for (int i = 0; i < regions.Count; i++)
            {
                Debug.Log($"[RELAY] Region {i}: " + $"{regions[i].Id} - {regions[i].Description}");
            }

            return true;
        }
        catch (Exception exception)
        {
            Fail("RELAY", exception);

            return false;
        }
    }

    public bool TestNGO()
    {
        Header("6 - NETCODE FOR GAMEOBJECTS");

        try
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
            {
                Fail("NGO", "NetworkManager.Singleton == null");

                return false;
            }

            Debug.Log("[NGO] NetworkManager encontrado.");

            if (networkManager.NetworkConfig == null)
            {
                Fail("NGO", "NetworkManager.NetworkConfig == null");

                return false;
            }

            if (networkManager.NetworkConfig.NetworkTransport == null)
            {
                Fail("NGO", "Nenhum NetworkTransport configurado.");

                return false;
            }

            Debug.Log($"[NGO] Transport configurado: " + $"{networkManager.NetworkConfig.NetworkTransport.GetType().Name}");

            UnityTransport unityTransport = networkManager.GetComponent<UnityTransport>();

            if (unityTransport == null)
            {
                Fail("NGO", "UnityTransport não encontrado no NetworkManager.");

                return false;
            }

            Debug.Log("[NGO] UnityTransport encontrado.");

            Debug.Log(
                $"[NGO] IsListening       = {networkManager.IsListening}\n" +
                $"[NGO] IsHost            = {networkManager.IsHost}\n" +
                $"[NGO] IsServer          = {networkManager.IsServer}\n" +
                $"[NGO] IsClient          = {networkManager.IsClient}\n" +
                $"[NGO] IsConnectedClient = {networkManager.IsConnectedClient}"
            );

            if (!string.IsNullOrEmpty(networkManager.DisconnectReason))
            {
                Debug.LogWarning($"[NGO] Último DisconnectReason: " + $"{networkManager.DisconnectReason}");
            }

            if (!networkManager.IsListening)
            {
                Debug.LogWarning("[NGO] NetworkManager está configurado, " + "mas NÃO está Listening.\n" + "Isso é NORMAL se você ainda está na tela " + "de Lobby/Join e StartHost/StartClient ainda não foi chamado.");
            }

            Pass("NGO", "NetworkManager + NetworkConfig + UnityTransport encontrados.");

            return true;
        }
        catch (Exception exception)
        {
            Fail("NGO", exception);

            return false;
        }
    }

    private static void Header(string title)
    {
        Debug.Log(
            "\n----------------------------------------\n" +
            $" {title}\n" +
            "----------------------------------------"
        );
    }

    private static void Pass(string system, string message)
    {
        Debug.Log($"<color=#55FF55>[PASS] [{system}]</color> {message}");
    }

    private static void Fail(string system, string message)
    {
        Debug.LogError($"[FAIL] [{system}] {message}");
    }

    private static void Fail(string system, Exception exception)
    {
        Debug.LogError($"[FAIL] [{system}]\n" + $"Type: {exception.GetType().Name}\n" + $"Message: {exception.Message}\n" + $"StackTrace:\n{exception.StackTrace}");
    }

    private static void Skip(string system, string reason)
    {
        Debug.LogWarning($"[SKIP] [{system}] {reason}");
    }

    private static string Result(bool value)
    {
        return value ? "PASS" : "FAIL";
    }
}
