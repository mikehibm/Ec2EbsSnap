'
' Copyright (c) 2011 Makoto Ishida
' Please see the file MIT-LICENSE.txt for copying permission.
'

Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Configuration

Imports Amazon
Imports Amazon.EC2
Imports Amazon.EC2.Model


Public Class Ec2EbsManager

    Private Shared Function GetServiceURL() As String
        Dim defaultRegion As String = ConfigurationManager.AppSettings("DefaultRegion")
        Dim s As String = ""
        If String.IsNullOrEmpty(defaultRegion) Then
            s = "https://ec2.amazonaws.com"
        Else
            s = "https://ec2." & defaultRegion & ".amazonaws.com"
        End If
        Return s
    End Function

    Private Shared Function GetAmazonEC2() As AmazonEC2
        Dim appConfig As NameValueCollection = ConfigurationManager.AppSettings

        Dim config As AmazonEC2Config = New AmazonEC2Config()
        config.ServiceURL = GetServiceURL()

        Dim accessKey As String = Encrypt.Decrypt(appConfig("EncryptedAWSAccessKey"))
        Dim secretKey As String = Encrypt.Decrypt(appConfig("EncryptedAWSSecretKey"))
        Dim ec2 As AmazonEC2 = AWSClientFactory.CreateAmazonEC2Client(accessKey, secretKey, config)

        Return ec2
    End Function

    Private Shared Sub HandleEC2Error(ByVal ex As AmazonEC2Exception)
        If ex.ErrorCode.Equals("OptInRequired") Then
            Console.WriteLine("Cannot sign in to EC2.")
        Else
            Console.WriteLine("Caught Exception: " & ex.Message)
            Console.WriteLine("Response Status Code: " & ex.StatusCode.ToString())
            Console.WriteLine("Error Code: " & ex.ErrorCode)
            Console.WriteLine("Error Type: " & ex.ErrorType)
            Console.WriteLine("Request ID: " & ex.RequestId)
        End If

    End Sub


    ''' <summary>
    ''' Lists existing snapshots for a volume.
    ''' </summary>
    ''' <param name="vol_id"></param>
    ''' <remarks></remarks>
    Public Shared Sub ListSnapshots(ByVal vol_id As String, ByVal description As String)
        Dim ec2 As AmazonEC2 = GetAmazonEC2()

        Dim request As DescribeSnapshotsRequest = New DescribeSnapshotsRequest()
        request.WithOwner("self")

        If Not String.IsNullOrEmpty(vol_id) Then
            Dim params As New Amazon.EC2.Model.Filter()
            params.Name = "volume-id"
            Dim lst As New List(Of String)
            lst.Add(vol_id)
            params.Value = lst
            request.WithFilter(params)
        End If

        Try
            Dim ec2Response As DescribeSnapshotsResponse = ec2.DescribeSnapshots(request)
            Dim snapshotList As List(Of Snapshot) = ec2Response.DescribeSnapshotsResult.Snapshot

            snapshotList = FilterByDescription(snapshotList, description)

            Console.WriteLine("You have " & snapshotList.Count.ToString() + " EBS Snapshots.")

            For Each itm As Snapshot In snapshotList
                Console.Write(itm.SnapshotId & " (" + itm.Progress + ") ")

                Dim dt As DateTime
                If DateTime.TryParse(itm.StartTime, dt) Then
                    Console.Write(dt.ToString("yyyy/MM/dd HH:mm:ss "))
                End If
                Console.Write(itm.Description)

                Console.WriteLine()
            Next

        Catch ex As AmazonEC2Exception
            HandleEC2Error(ex)
        End Try
        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' Creates a snapshot for a volume.
    ''' </summary>
    ''' <param name="vol_id"></param>
    ''' <remarks></remarks>
    Public Shared Function CreateSnapshot(ByVal vol_id As String, description As String) As Boolean
        Dim result As Boolean = False

        If String.IsNullOrEmpty(vol_id) Then
            Console.WriteLine("ERROR: volume id is required to create a snapshot.")
            Return False
        End If


        Dim ec2 As AmazonEC2 = GetAmazonEC2()

        If String.IsNullOrEmpty(description) Then description = ConfigurationManager.AppSettings("SnapshotDescription")
        If String.IsNullOrEmpty(description) Then description = "[AUTO]"

        Dim request As CreateSnapshotRequest = New CreateSnapshotRequest()
        request.WithVolumeId(vol_id)
        request.WithDescription(description)

        Try
            Dim ec2Response As CreateSnapshotResponse = ec2.CreateSnapshot(request)
            Dim snapshotId As String = ec2Response.CreateSnapshotResult.Snapshot.SnapshotId
            Console.WriteLine("Successfully started to create new snapshot " & snapshotId & ".")
            result = True

        Catch ex As AmazonEC2Exception
            result = False
            HandleEC2Error(ex)
        End Try
        Console.WriteLine()

        Return result
    End Function

    ''' <summary>
    ''' Delete snapshots that meets criteria.
    ''' </summary>
    ''' <param name="vol_id"></param>
    ''' <param name="max_generation"></param>
    ''' <param name="max_age"></param>
    ''' <remarks></remarks>
    Public Shared Sub DeleteSnapshot(ByVal vol_id As String, ByVal description As String, ByVal max_generation As Integer, ByVal max_age As String)
        Dim ec2 As AmazonEC2 = GetAmazonEC2()

        Dim request As DescribeSnapshotsRequest = New DescribeSnapshotsRequest()
        request.WithOwner("self")

        If Not String.IsNullOrEmpty(vol_id) Then
            Dim params As New Amazon.EC2.Model.Filter()
            params.Name = "volume-id"
            Dim lst As New List(Of String)
            lst.Add(vol_id)
            params.Value = lst
            request.WithFilter(params)
        End If

        Try
            Dim ec2Response As DescribeSnapshotsResponse = ec2.DescribeSnapshots(request)
            Dim snapshotList As List(Of Snapshot) = ec2Response.DescribeSnapshotsResult.Snapshot
            Dim deleteList As New List(Of Snapshot)

            snapshotList = FilterByDescription(snapshotList, description)
            snapshotList = FilterByAge(snapshotList, max_age, deleteList)
            If Not String.IsNullOrEmpty(vol_id) Then
                snapshotList = FilterByGeneration(snapshotList, max_generation, deleteList)
            End If

            If deleteList.Count = 0 Then
                Console.WriteLine("No snapshots to delete!")
            Else
                For Each itm As Snapshot In deleteList
                    Console.Write("*** DELETING " & itm.SnapshotId & " ")

                    Dim dt As DateTime
                    If DateTime.TryParse(itm.StartTime, dt) Then
                        Console.Write(dt.ToString("yyyy/MM/dd HH:mm:ss "))
                    End If

                    Console.WriteLine()

                    DeleteOneSnapshot(itm.SnapshotId)
                Next
            End If


        Catch ex As AmazonEC2Exception
            HandleEC2Error(ex)
        End Try
        Console.WriteLine()
    End Sub

    Private Shared Sub DeleteOneSnapshot(ByVal snapshot_id As String)

        '#If DEBUG Then
        '        Console.WriteLine("DEBUG: Delete the snapshot " & snapshot_id & ".")
        '        return  
        '#End If

        Dim ec2 As AmazonEC2 = GetAmazonEC2()

        Dim request As DeleteSnapshotRequest = New DeleteSnapshotRequest()
        request.WithSnapshotId(snapshot_id)

        Try
            Dim ec2Response As DeleteSnapshotResponse = ec2.DeleteSnapshot(request)
            Dim response As String = ec2Response.ToString
            Console.WriteLine("Successfully requested to delete the snapshot " & snapshot_id & ".")

        Catch ex As AmazonEC2Exception
            HandleEC2Error(ex)
        End Try
        Console.WriteLine()
    End Sub

    Private Shared Function FilterByDescription( _
                                ByVal snapshotList As List(Of Snapshot), _
                                ByVal desc As String) As List(Of Snapshot)

        If String.IsNullOrEmpty(desc) Then Return snapshotList

        Dim newList As New List(Of Snapshot)
        For Each sn As Snapshot In snapshotList
            If sn.Description.Contains(desc) Then
                newList.Add(sn)
            End If
        Next
        Return newList
    End Function

    Private Shared Function FilterByGeneration( _
                                ByVal snapshotList As List(Of Snapshot), _
                                ByVal max_generation As Integer, _
                                ByVal deleteList As List(Of Snapshot)) As List(Of Snapshot)

        If max_generation <= 0 Then Return snapshotList

        Dim newList As New List(Of Snapshot)
        If max_generation > snapshotList.Count Then max_generation = snapshotList.Count
        For i As Integer = 0 To snapshotList.Count - 1
            If (i < snapshotList.Count - max_generation) Then
                deleteList.Add(snapshotList(i))
            Else
                newList.Add(snapshotList(i))
            End If
        Next
        Return newList
    End Function

    Private Shared Function FilterByAge( _
                                ByVal snapshotList As List(Of Snapshot), _
                                ByVal max_age As String, _
                                ByVal deleteList As List(Of Snapshot)) As List(Of Snapshot)

        If String.IsNullOrEmpty(max_age) Then Return snapshotList

        Dim n As Integer = CInt(Val(max_age))
        Dim unit As String = max_age.Substring(max_age.Length - 1)          'Last character is supposed to be 'd', 'h', or 'm'.
        Dim span As TimeSpan
        Select Case unit.ToLower
            Case "h" : span = New TimeSpan(n, 0, 0)                     'Hours
            Case "m" : span = New TimeSpan(0, n, 0)                     'Minutes
            Case Else : span = New TimeSpan(n, 0, 0, 0, 0)              'Days
        End Select

        Dim newList As New List(Of Snapshot)
        Dim dt As DateTime
        For Each sn As Snapshot In snapshotList
            If DateTime.TryParse(sn.StartTime, dt) Then
                If DateTime.Now.Subtract(dt).CompareTo(span) <= 0 Then
                    newList.Add(sn)
                Else
                    deleteList.Add(sn)
                End If
            End If
        Next
        Return newList
    End Function



End Class
