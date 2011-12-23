Imports System.Security.Cryptography
Imports System.Text

Public Class Encrypt

    Private Const ENCRYPT_KEY As String = "YOUR_ENCRYPTION_KEY_SHOULD_GO_HERE"

    Public Shared Function Encrypt(ByVal original As String) As String
        Dim des As New TripleDESCryptoServiceProvider()
        Dim hashmd5 As New MD5CryptoServiceProvider()
        Dim buff As Byte()

        des.Key = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPT_KEY))
        des.Mode = CipherMode.ECB

        buff = Encoding.UTF8.GetBytes(original)
        Return Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(buff, 0, buff.Length))
    End Function

    Public Shared Function Decrypt(ByVal encrypted As String) As String
        If String.IsNullOrEmpty(encrypted) Then Return String.Empty

        Dim des As New TripleDESCryptoServiceProvider()
        Dim hashmd5 As New MD5CryptoServiceProvider()
        Dim buff As Byte()

        des.Key = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPT_KEY))
        des.Mode = CipherMode.ECB

        buff = Convert.FromBase64String(encrypted)
        Return Encoding.UTF8.GetString(des.CreateDecryptor().TransformFinalBlock(buff, 0, buff.Length))
    End Function

End Class
