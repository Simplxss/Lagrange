namespace Lagrange.Core.Internal.Event.Login;

internal class LoginCommon
{
    public enum Error : uint
    {
        TokenExpired = 140022015,
        UnusualVerify = 140022011,
        NewDeviceVerify = 140022010,
        CaptchaVerify = 140022008,
        Success = 0,
        Unknown = 1,
    }

    /*
     * 0 ��¼�ɹ�
     * 1 �������
     * 2 ��֤��
     * 32 ������
     * 40 ������
     * 160 �豸��
     * 162 ���ŷ���ʧ��
     * 163 ������֤�����
     * 180 �ع� (ecdh����, ...)
     * 204 �豸�� ��֤
     * 235 �汾����
     * 237 ���������쳣
     * 239 �豸��
     * 243 �Ƿ���Դ��ֹ��¼
     */
    public enum State : uint
    {
        Success = 0,
        PasswordError = 1,
        CaptchaVerify = 2,
        Recycle = 32,
        Freeze = 40,
        DeviceLock = 160,
        SmsSendFail = 162,
        SmsVerifyError = 163,
        Rollback = 180,
        DeviceLockVerify = 204,
        VersionLow = 235,
        NetworkAbnormal = 237,
        DeviceLock2 = 239,
        Unknown = 240,
        IllegalSource = 243,
    }
}