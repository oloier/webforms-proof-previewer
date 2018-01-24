Imports System.Data
Imports System.Data.SqlClient
'Imports System.Collections.Generic


Public Class dba
    Implements IDisposable

    Private _connectionString As String
    Private _connection As New SqlConnection()
    Private _prefix As String
    Private _sqlQuery As String

    Public Sub New(connectionString As String, sqlQuery As String)
        Me._connectionString = connectionString
        Me._connection.ConnectionString = Me._connectionString
        Me._connection.Open()
        _sqlQuery = sqlQuery
    End Sub

    Protected Overridable Sub Dispose() Implements IDisposable.Dispose
        Me._connection.Close()
    End Sub

    Private Function createSqlCommand(params As Dictionary(Of String, Object)) As SqlCommand
        Dim cmd = New SqlCommand(Me._sqlQuery)
        cmd.Connection = Me._connection
        cmd.Parameters.AddRange(addParams(params))
        Return cmd
    End Function

    Private Function addParams(params As Dictionary(Of String, Object)) As Array
        Dim paramCollection As New List(Of SqlParameter)
        For Each param In params
            paramCollection.Add(New SqlParameter(param.Key, param.Value))
        Next
        Return paramCollection.ToArray
    End Function

    Public Function executeReader(params As Dictionary(Of String, Object)) As SqlDataReader
        Dim sdr As SqlDataReader = Nothing
        Using cmd As SqlCommand = Me.createSqlCommand(params)
            Try
                sdr = cmd.ExecuteReader()
            Catch ex As Exception
                dev.log(ex.Message)
            End Try
        End Using
        Return sdr
    End Function

    Public Function readerDatatable(params As Dictionary(Of String, Object)) As DataTable
        Dim dt As New DataTable
        Try
            Dim sqlDr As SqlDataReader
            sqlDr = Me.executeReader(params)
            dt.Load(sqlDr)
            dt.TableName = "sqlResults"
        Catch ex As Exception
            dev.log(ex.Message)
        End Try
        Return dt
    End Function

    Public Function executeDataAdapter(params As Dictionary(Of String, Object)) As DataTable
        Dim sda = New SqlDataAdapter(Me._sqlQuery, Me._connection)
        For Each param As KeyValuePair(Of String, Object) In params
            sda.SelectCommand.Parameters.Add(param.Key, SqlDbType.VarChar).Value = param.Value
        Next
        Dim dt As New DataTable
        sda.Fill(dt)
        dt.TableName = "sqlResults"
        Return dt
    End Function

    Public Sub executeNonQuery(params As Dictionary(Of String, Object))
        Using cmd As SqlCommand = Me.createSqlCommand(params)
            Try
                cmd.ExecuteNonQuery()
            Catch ex As Exception
                dev.log(ex.Message)
            End Try
        End Using
    End Sub

    Public Function executeScalar(params As Dictionary(Of String, Object)) As Object
        Dim scl As New Object
        Using cmd As SqlCommand = Me.createSqlCommand(params)
            Try
                scl = cmd.ExecuteScalar()
            Catch ex As Exception
                dev.log(ex.Message)
            End Try
        End Using
        Return scl
    End Function


End Class



'Public Class dbSP
'    PrivateMe. _connectionString As String
'    Private con As New SqlConnection()

'    Public Sub New(connectionString As String)
'       Me. _connectionString = connectionString
'        con.ConnectionString =Me. _connectionString
'    End Sub

'    Public Sub ExecuteNonQuery(cmdTxt As String, params As Dictionary(Of String, Object))
'        Using cmd As SqlCommand = buildCommand(cmdTxt, params)
'            cmd.ExecuteNonQuery()
'        End Using
'    End Sub

'    Public Function ExecuteReader(cmdTxt As String, params As Dictionary(Of String, Object)) As SqlDataReader
'        con.Open()
'        Using cmd As SqlCommand = buildCommand(cmdTxt, params)
'            Return cmd.ExecuteReader()
'        End Using
'        con.Close()
'    End Function

'    Public Function ExecuteScalar(cmdTxt As String, params As Dictionary(Of String, Object)) As Object
'        Using cmd As SqlCommand = buildCommand(cmdTxt, params)
'            Return cmd.ExecuteScalar()
'        End Using
'    End Function

'    Private Function buildCommand(cmdTxt As String, params As Dictionary(Of String, Object)) As SqlCommand
'        Using con As New SqlConnection(_connectionString)
'            Using cmd As SqlCommand = con.CreateCommand()
'                cmd.CommandType = CommandType.StoredProcedure
'                cmd.CommandText = cmdTxt
'                AddParameters(cmd, params)
'                con.Open()
'                Return cmd
'            End Using
'        End Using
'    End Function

'    Private Sub AddParameters(ByRef cmd As SqlCommand, params As Dictionary(Of String, Object))
'        If params IsNot Nothing Then
'            For Each kvp As KeyValuePair(Of String, Object) In params
'                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value)
'            Next
'        End If
'    End Sub

'End Class