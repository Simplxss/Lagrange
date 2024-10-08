using Lagrange.Core.Common;
using Lagrange.Core.Internal.Event;
using Lagrange.Core.Internal.Event.Login;
using Lagrange.Core.Internal.Packets.Login.NTLogin;
using Lagrange.Core.Internal.Packets.Login.NTLogin.Plain;
using Lagrange.Core.Internal.Packets.Login.NTLogin.Plain.Body;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Crypto;
using ProtoBuf;

namespace Lagrange.Core.Internal.Service.Login;

[EventSubscribe(typeof(NewDeviceLoginEvent))]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginNewDevice", 12, 2)]
internal class NewDeviceLoginService : BaseService<NewDeviceLoginEvent>
{
    protected override bool Build(NewDeviceLoginEvent input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device,
        out BinaryPacket output, out List<BinaryPacket>? extraPackets)
    {
        if (keystore.Session.A2 == null) throw new InvalidOperationException("A2 is null");
        // Field #1  { "type": "down-sms", "sig": "i4rWSeOd2D2il7KiIElXGGO%2BGGlWoTY3kqHqITXtJcNuWVKjLH8dwnqMCyhnYVDs" }

        output = SsoNTLoginCommon.BuildNTLoginPacket(keystore, appInfo, device, keystore.Session.A2);
        extraPackets = null;
        return true;
    }

    protected override bool Parse(Span<byte> input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device, 
        out NewDeviceLoginEvent output, out List<ProtocolEvent>? extraEvents)
    {
        if (keystore.Session.ExchangeKey == null) throw new InvalidOperationException("ExchangeKey is null");
        
        var encrypted = Serializer.Deserialize<SsoNTLoginEncryptedData>(input);

        if (encrypted.GcmCalc != null)
        {
            var decrypted = AesGcmImpl.Decrypt(encrypted.GcmCalc, keystore.Session.ExchangeKey);
            var response = Serializer.Deserialize<SsoNTLoginBase<SsoNTLoginResponse>>(decrypted.AsSpan());
            var body = response.Body;

            if (response.Header?.Error != null || body is not { Credentials: not null, Uid: not null})
            {
                output = NewDeviceLoginEvent.Result((int)(response.Header?.Error?.ErrorCode ?? 1));
            }
            else
            {
                keystore.Uid = body.Uid.Uid;
                keystore.Session.Tgt = body.Credentials.Tgt;
                keystore.Session.D2 = body.Credentials.D2;
                keystore.Session.D2Key = body.Credentials.D2Key;
                keystore.Session.A2 = body.Credentials.A2;
                keystore.Session.SessionDate = DateTime.Now;

                output = NewDeviceLoginEvent.Result(0);
            }
        }
        else
        {
            output = NewDeviceLoginEvent.Result(1);
        }

        extraEvents = null;
        return true;
    }
}