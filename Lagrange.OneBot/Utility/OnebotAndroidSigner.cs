using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Internal.Packets.System;
using Lagrange.Core.Utility.Generator;
using Lagrange.Core.Utility.Extension;
using Lagrange.Core.Utility.Network;
using Lagrange.Core.Common;
using Microsoft.Extensions.Configuration;
using ProtoBuf;

namespace Lagrange.Core.Utility.Sign;

internal class OnebotAndroidSigner : SignProvider
{
    private readonly string Url;
    private readonly string SignUrl;
    private readonly string EnergyUrl;
    private readonly string GetXwDebugIdUrl;

    private readonly HttpClient _client = new();

    public OnebotAndroidSigner(IConfiguration config)
    {
        Url = config["AndroidSignServerUrl"] ?? "";
        SignUrl = $"{Url}/sign";
        EnergyUrl = $"{Url}/custom_energy";
        GetXwDebugIdUrl = $"{Url}/get_xw_debug_id";
    }

    public override byte[] Sign(BotDeviceInfo device, BotKeystore keystore, string cmd, int seq, byte[] body)
    {
        var signature = new ReserveFields
        {
            Flag = 1,
            LocaleId = 2052,
            Qimei = keystore.Session.QImei?.Q36,
            NewconnFlag = 0,
            TraceParent = StringGen.GenerateTrace(),
            Uid = keystore.Uid,
            Imsi = 0,
            NetworkType = 1,
            IpStackType = 1,
            MsgType = 0,
            TransInfo = new Dictionary<string, string>{
                { "client_conn_seq", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
            },
            NtCoreVersion = 100,
            SsoIpOrigin = 3
        };
        if (WhiteListCommand.Contains(cmd) && !string.IsNullOrEmpty(Url))
        {
            try
            {
                var payload = new JsonObject
                {
                    { "uin", keystore.Uin },
                    { "cmd", cmd },
                    { "seq", seq },
                    { "buffer", body.Hex() },
                    { "android_id", device.System.AndroidId },
                    { "guid", device.System.Guid.ToByteArray().Hex() }
                };
                var message = _client.PostAsJsonAsync(SignUrl, payload).Result;
                string response = message.Content.ReadAsStringAsync().Result;
                var json = JsonSerializer.Deserialize<JsonObject>(response);

                var secSig = json?["data"]?["sign"]?.ToString().UnHex();
                var secDeviceToken = json?["data"]?["token"]?.ToString().UnHex();
                var secExtra = json?["data"]?["extra"]?.ToString().UnHex();

                signature.SecInfo = new SsoSecureInfo
                {
                    SecSig = secSig,
                    SecDeviceToken = secDeviceToken is null ? null : Encoding.UTF8.GetString(secDeviceToken),
                    SecExtra = secExtra,
                };
            }
            catch (Exception) { }
        }
        var stream = new MemoryStream();
        Serializer.Serialize(stream, signature);
        return stream.ToArray();
    }

    public override byte[] Energy(string salt, string data)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                { "salt", salt },
                { "data", data }
            };
            string response = Http.GetAsync(EnergyUrl, payload).GetAwaiter().GetResult();
            var json = JsonSerializer.Deserialize<JsonObject>(response);

            return json?["data"]?["data"]?.ToString().UnHex() ?? Array.Empty<byte>();
        }
        catch (Exception)
        {
            return Array.Empty<byte>();
        }
    }

    public override byte[] GetXwDebugId(uint uin, string cmd, string subCmd)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                { "uin", uin.ToString() },
                { "cmd", cmd },
                { "subCmd", subCmd },
                { "data", $"{cmd}_{subCmd}" }
            };
            string response = Http.GetAsync(GetXwDebugIdUrl, payload).GetAwaiter().GetResult();
            var json = JsonSerializer.Deserialize<JsonObject>(response);

            return json?["data"]?["data"]?.ToString().UnHex() ?? Array.Empty<byte>();
        }
        catch (Exception)
        {
            return Array.Empty<byte>();
        }
    }
}