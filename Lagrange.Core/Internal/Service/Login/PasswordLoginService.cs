﻿using Lagrange.Core.Common;
using Lagrange.Core.Internal.Event;
using Lagrange.Core.Internal.Event.Login;
using Lagrange.Core.Internal.Packets.Login.NTLogin;
using Lagrange.Core.Internal.Packets.Login.NTLogin.Plain;
using Lagrange.Core.Internal.Packets.Login.NTLogin.Plain.Body;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Crypto;
using Lagrange.Core.Utility.Generator;
using ProtoBuf;

namespace Lagrange.Core.Internal.Service.Login;

[EventSubscribe(typeof(PasswordLoginEvent))]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin", 12, 2)]
internal class PasswordLoginService : BaseService<PasswordLoginEvent>
{
    protected override bool Build(PasswordLoginEvent input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device,
        out BinaryPacket output, out List<BinaryPacket>? extraPackets)
    {
        var plainBody = new SsoNTLoginPasswordLogin
        {
            Random = (uint)Random.Shared.Next(),
            AppId = appInfo.AppId,
            Uin = keystore.Uin,
            Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            PasswordMd5 = keystore.PasswordMd5!,
            RandomBytes = ByteGen.GenRandomBytes(16),
            Guid = device.System.Guid.ToByteArray(),
            UinString = keystore.Uin.ToString()
        };
        var plainBytes = BinarySerializer.Serialize(plainBody);
        var encryptedPlain = keystore.TeaImpl.Encrypt(plainBytes.ToArray(), keystore.PasswordWithSalt!);
        
        output = SsoNTLoginCommon.BuildNTLoginPacket(keystore, appInfo, device, encryptedPlain);
        extraPackets = null;
        return true;
    }

    protected override bool Parse(Span<byte> input, BotKeystore keystore, BotAppInfo appInfo, BotDeviceInfo device,
        out PasswordLoginEvent output, out List<ProtocolEvent>? extraEvents)
    {
        if (keystore.Session.ExchangeKey == null) throw new InvalidOperationException("ExchangeKey is null");
        var encrypted = Serializer.Deserialize<SsoNTLoginEncryptedData>(input);
        
        if (encrypted.GcmCalc != null)
        {
            var decrypted = AesGcmImpl.Decrypt(encrypted.GcmCalc, keystore.Session.ExchangeKey);
            var response = Serializer.Deserialize<SsoNTLoginBase<SsoNTLoginResponse>>(decrypted.AsSpan());
            var body = response.Body;
            
            if (response.Header?.Error != null || body is not { Credentials: not null, Uid: not null })
            {
                keystore.Session.UnusualCookies = response.Header?.Cookie?.Cookie;
                keystore.Session.NewDeviceVerifyUrl = response.Header?.Error?.NewDeviceVerifyUrl;
                keystore.Session.CaptchaUrl = body?.Captcha?.Url;
                
                string? tag = response.Header?.Error?.Tag;
                string? message = response.Header?.Error?.Message;
                output = PasswordLoginEvent.Result((int)(response.Header?.Error?.ErrorCode ?? 1), tag, message);
            }
            else
            {
                keystore.Uid = body.Uid.Uid;
                keystore.Session.Tgt = body.Credentials.Tgt;
                keystore.Session.D2 = body.Credentials.D2;
                keystore.Session.D2Key = body.Credentials.D2Key;
                keystore.Session.A2 = body.Credentials.A2;
                keystore.Session.SessionDate = DateTime.Now;

                output = PasswordLoginEvent.Result(0);
            }
        }
        else
        {
            output = PasswordLoginEvent.Result(1);
        }

        extraEvents = null;
        return true;
    }
}