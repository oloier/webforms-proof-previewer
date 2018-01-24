Imports System.IO

Partial Class proof_imgview
    Inherits System.Web.UI.Page

    Dim _proofid As String = ""
    Protected Property proofid As String
        Get
            Return _proofid
        End Get
        Set(value As String)
            If value <> "" Then
                _proofid = sanitizeParam(value)
            End If
        End Set
    End Property

    Dim _layout As Integer = 0
    Protected Property layout As Integer
        Get
            Return _layout
        End Get
        Set(value As Integer)
            If value > 0 Then
                _layout = sanitizeParam(value)
            End If
        End Set
    End Property

    Dim _filename As String = ""
    Protected Property fileName As String
        Get
            Return _filename
        End Get
        Set(value As String)
            Dim file As String = ""
            If value <> "" AndAlso value.IndexOf(".") <> -1 Then
                _filename = sanitizeParam(value)
            End If
        End Set
    End Property

    Protected ReadOnly Property filePath As String
        Get
            If dev.isLocal Then
                Return Server.MapPath(String.Format(".\proofs\{0}\layout{1}\", proofid, layout))
            Else
                Return String.Format("X:\inetpub\webapps\proofs\{0}\layout{1}\", proofid, layout)
            End If
        End Get
    End Property

    Dim _download As Boolean = False
    Protected Property download As Boolean
        Get
            Return _download
        End Get
        Set(value As Boolean)
            _download = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim layoutID As String = Request.QueryString("layout")
        Dim layoutInt As Integer = 0
        Int32.TryParse(layoutID, layoutInt)
        layout = layoutInt

        Dim proof As String = Request.QueryString("proof")
        proofid = proof

        Dim file As String = Request.QueryString("file")
        fileName = file

        Dim downloadCheck As String = Request.QueryString("download")
        download = If(downloadCheck = "1", True, False)

        If proofid <> "" AndAlso layout > 0 AndAlso fileName <> "" Then
            Try
                ' Read the file and convert it to byte array
                Dim mimeType As String = "image/" & Path.GetExtension(fileName).Replace(".", "")

                ' Check for PDF or DAE and set MIME type
                If fileName.IndexOf(".pdf") <> -1 Then
                    mimeType = "application/pdf"
                ElseIf fileName.IndexOf(".dae") <> -1 Then
                    mimeType = "text/xml"
                End If

                ' Setup the filestream and read the file from our single location
                Dim truePath As String = filePath & fileName
                Dim fs As FileStream = New FileStream(truePath, FileMode.Open, FileAccess.Read)
                Dim br As BinaryReader = New BinaryReader(fs)
                Dim bytes As Byte() = br.ReadBytes(Convert.ToInt32(fs.Length))
                br.Close()
                fs.Close()

                ' Write the file to reponse
                Response.Buffer = True
                Response.Charset = ""
                Response.Cache.SetCacheability(HttpCacheability.NoCache)
                Response.ContentType = mimeType
                If download Then
                    Response.AddHeader("content-disposition", "attachment;filename=" & fileName)
                End If
                Response.BinaryWrite(bytes)
                Response.Flush()
                Response.End()
            Catch

            End Try
        End If
    End Sub

    Protected Function sanitizeParam(ByVal inputString As String) As String
        If inputString <> "" Then
            Return System.Text.RegularExpressions.Regex.Replace(inputString, "[^a-zA-Z0-9-_.]", "").ToLower
        Else
            Return ""
        End If
    End Function

End Class
