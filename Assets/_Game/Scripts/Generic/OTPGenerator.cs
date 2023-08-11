using System.Collections.Generic;

public static class OTPGenerator
{
    static char[] chars = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
    private static List<string> usedCodes = new List<string>();
    private static int codeLength = 4;

    public static string GenerateOTP()
    {
        string otp = "";
        for(int i = 0; i < codeLength; i++)
            otp += chars[UnityEngine.Random.Range(0, chars.Length)];
        if (usedCodes.Contains(otp))
            GenerateOTP();
        usedCodes.Add(otp);
        return otp;
    }
}
