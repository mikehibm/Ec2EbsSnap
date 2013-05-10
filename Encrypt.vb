'
' Copyright (c) 2011 Makoto Ishida
' Please see the file MIT-LICENSE.txt for copying permission.
'

Imports System.Security.Cryptography
Imports System.Text
Imports System.Configuration

Public Class Encrypt

    Private Shared Function GetEncryptionKey() As String
        Dim EncryptKey As String = ConfigurationManager.AppSettings("EncryptionKey")

        If String.IsNullOrEmpty(EncryptKey) Then
            Throw New Exception("EncryptionKey is not configured in application config file.")
        End If

        Return EncryptKey
    End Function

    Private Shared Function GetEncryptionKeyAsBytes() As Byte()
        Dim key As String = GetEncryptionKey()
        Dim hashmd5 As New MD5CryptoServiceProvider()
        Return hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key))
    End Function

    Private Shared Function GetIV() As Byte()
        Dim key As String = GetEncryptionKey()
        key &= key
        Dim hashmd5 As New MD5CryptoServiceProvider()
        Return hashmd5.ComputeHash(hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key)))
    End Function

    Public Shared Function Encrypt(ByVal original As String) As String
        Dim aes As New AesCryptoServiceProvider()
        aes.Key = GetEncryptionKeyAsBytes()
        aes.IV = GetIV()
        aes.Mode = CipherMode.CBC

        Dim buff As Byte() = Encoding.UTF8.GetBytes(original)
        Return Convert.ToBase64String(aes.CreateEncryptor().TransformFinalBlock(buff, 0, buff.Length))
    End Function

    Public Shared Function Decrypt(ByVal encrypted As String) As String
        If String.IsNullOrEmpty(encrypted) Then Return String.Empty

        Dim aes As New AesCryptoServiceProvider()
        aes.Key = GetEncryptionKeyAsBytes()
        aes.IV = GetIV()
        aes.Mode = CipherMode.CBC

        Dim buff As Byte() = Convert.FromBase64String(encrypted)
        Return Encoding.UTF8.GetString(aes.CreateDecryptor().TransformFinalBlock(buff, 0, buff.Length))
    End Function

End Class
