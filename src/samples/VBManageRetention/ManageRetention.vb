'
'Copyright � 2006 EMC Corporation. All Rights Reserved
'
'This file is part of .NET wrapper for the Centera SDK.
'
'.NET wrapper is free software; you can redistribute it and/or modify it under
'the terms of the GNU General Public License as published by the Free Software
'Foundation version 2.
'
'In addition to the permissions granted in the GNU General Public License
'version 2, EMC Corporation gives you unlimited permission to link the compiled
'version of this file into combinations with other programs, and to distribute
'those combinations without any restriction coming from the use of this file.
'(The General Public License restrictions do apply in other respects; for
'example, they cover modification of the file, and distribution when not linked
'into a combined executable.)
'
'.NET wrapper is distributed in the hope that it will be useful, but WITHOUT ANY
'WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
'PARTICULAR PURPOSE. See the GNU General Public License version 2 for more
'details.
'
'You should have received a copy of the GNU General Public License version 2
'along with .NET wrapper; see the file COPYING. If not, write to:
'
' EMC Corporation 
' Centera Open Source Intiative (COSI) 
' 80 South Street
' 1/W-1
' Hopkinton, MA 01748 
' USA
'

Imports System
Imports EMC.Centera.SDK
Imports EMC.Centera.FPTypes


Namespace ManageRetention
    _
    '/ <summary>
    '/ Summary description for Class1.
    '/ </summary>
    Class ManageRetention
        Private Shared clusterAddress As [String] = "128.221.200.64"

        Public Shared Sub Main(ByVal args() As String)
            Try
                FPLogger.ConsoleMessage(("Cluster to connect to [" + clusterAddress + "]: "))
                Dim answer As [String] = Console.ReadLine()

                If answer <> "" Then
                    clusterAddress = answer
                End If
                Dim myPool As New FPPool(clusterAddress)

                FPLogger.ConsoleMessage("\nEnter the content address of the clip to manage: ")
                Dim clipID As [String] = Console.ReadLine()

                Dim clipRef As FPClip = myPool.ClipOpen(clipID, FPMisc.OPEN_ASTREE)

                FPLogger.ConsoleMessage(ControlChars.Lf + "The following RetentionClasses are configured in the pool:" + ControlChars.Lf)

                Dim rc As FPRetentionClass
                For Each rc In myPool.RetentionClasses
                    FPLogger.ConsoleMessage(rc)
                Next rc

                FPLogger.ConsoleMessage(ControlChars.Lf + "Enter RetentionClass or Period (in seconds) to be set: ")
                Dim newValue As [String] = Console.ReadLine()


                If myPool.RetentionClasses.ValidateClass(newValue) Then
                    clipRef.FPRetentionClass = myPool.RetentionClasses.GetClass(newValue)
                Else
                    clipRef.RetentionPeriod = New TimeSpan(0, 0, Integer.Parse(newValue))
                End If

                clipID = clipRef.Write()

                FPLogger.ConsoleMessage((ControlChars.Lf + "The new content address for the clip is " + clipID))
                FPLogger.ConsoleMessage(("Cluster time " + myPool.ClusterTime))
                FPLogger.ConsoleMessage(("Clip created " + clipRef.CreationDate))
                FPLogger.ConsoleMessage(("The retention period " + clipRef.RetentionPeriod.ToString()))
                FPLogger.ConsoleMessage(("The retention will expire on " + clipRef.RetentionExpiry))

                clipRef.Close()

            Catch e As FPLibraryException
                Dim err As ErrorInfo = e.errorInfo
                FPLogger.ConsoleMessage(("Exception thrown in FP Library: Error " + err.error + " " + err.message))
            Catch e As Exception
                FPLogger.ConsoleMessage(("Exception thrown! " + e.Message))
            End Try
        End Sub 'Main

    End Class 'ManageRetention
End Namespace 'ManageRetention

