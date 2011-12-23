'
' Copyright (c) 2011 Makoto Ishida
' Please see the file MIT-LICENSE.txt for copying permission.
'

Module Module1

    Sub Main(ByVal args() As String)
        Console.WriteLine("===========================================")
        Console.WriteLine("EBS Snapshot Utility")
        Console.WriteLine("===========================================")

        If args.Count = 2 Then
            Select Case args(0).ToUpper
                Case "/L" : ListSnapshots(args(1), "")
                Case "/C" : CreateSnapshot(args(1), "")
                Case "/ENC" : ShowEncrypted(args(1))
                    'Case "/DEC" : ShowDecrypted(args(1))
                Case Else : ShowHelp()
            End Select

        ElseIf args.Count = 3 Then
            Select Case args(0).ToUpper
                Case "/L" : ListSnapshots(args(1), args(2))
                Case "/C" : CreateSnapshot(args(1), args(2))
                Case Else : ShowHelp()
            End Select

        ElseIf args.Count = 4 Then
            Select Case args(0).ToUpper
                Case "/D" : DeleteSnapshot(args(1), args(2), args(3), "")
                Case "/CD" : CreateAndDeleteSnapshot(args(1), args(2), args(3), "")
                Case Else : ShowHelp()
            End Select

        ElseIf args.Count = 5 Then
            Select Case args(0).ToUpper
                Case "/D" : DeleteSnapshot(args(1), args(2), args(3), args(4))
                Case "/CD" : CreateAndDeleteSnapshot(args(1), args(2), args(3), args(4))
                Case Else : ShowHelp()
            End Select

        Else
            ShowHelp()
        End If

#If DEBUG Then
        Console.WriteLine("Press any key.")
        Console.ReadKey()
#End If

    End Sub

    Private Sub ShowHelp()
        Console.WriteLine("Usage:")
        Console.WriteLine("  /L  volume_id                          : List snapshots.")
        Console.WriteLine("  /c  volume_id                          : Create a new snapshot.")
        Console.WriteLine("  /d  volume_id description max_generation [max_age] : Delete old snapshots.")
        Console.WriteLine("  /cd volume_id description max_generation [max_age] : Create a new snapshot and delete old ones.")
        Console.WriteLine("  /enc text_to_encrypt                   : Encrypt a text to be used in config file.")

    End Sub

    Private Sub ShowEncrypted(ByVal str As String)
        Console.WriteLine("Encrypted: ")
        Console.WriteLine(Encrypt.Encrypt(str))
    End Sub

    Private Sub ShowDecrypted(ByVal str As String)
        Console.WriteLine("Decrypted: ")
        Console.WriteLine(Encrypt.Decrypt(str))
    End Sub

    Private Sub ListSnapshots(ByVal vol_id As String, ByVal description As String)
        Console.WriteLine("Existing snapshots for the volume '" & vol_id & "':")
        Ec2EbsManager.ListSnapshots(vol_id, description)
    End Sub

    Private Function CreateSnapshot(ByVal vol_id As String, ByVal description As String) As Boolean
        Console.WriteLine("Creating snapshot for the volume '" & vol_id & "':")
        Return Ec2EbsManager.CreateSnapshot(vol_id, description)
    End Function

    Private Sub DeleteSnapshot(ByVal vol_id As String, ByVal description As String, ByVal max_generation As String, ByVal max_age As String)
        ListSnapshots(vol_id, description)

        Console.WriteLine("Deleting snapshots for the volume '" & vol_id & "':")

        Dim int_max_generation As Integer = CInt(Val(max_generation))

        Ec2EbsManager.DeleteSnapshot(vol_id, description, int_max_generation, max_age)
    End Sub

    Private Sub CreateAndDeleteSnapshot(ByVal vol_id As String, ByVal description As String, ByVal max_generation As String, ByVal max_age As String)

        If CreateSnapshot(vol_id, description) Then
            DeleteSnapshot(vol_id, description, max_generation, max_age)
        End If
    End Sub

End Module
