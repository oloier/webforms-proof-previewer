Imports System.Data
Imports System.Data.SqlClient



Public Class settings
    Private Shared appSettings As NameValueCollection = System.Configuration.ConfigurationManager.AppSettings

    Public Shared ReadOnly Property siteDomain As String
        Get
            Return appSettings("siteDomain")
        End Get
    End Property

    Public Shared ReadOnly Property proofEmailAccount As String
        Get
            Return appSettings("proofEmailAccount")
        End Get
    End Property

    Public Shared ReadOnly Property proofAppPath As String
        Get
            Return appSettings("proofAppPath")
        End Get
    End Property

    ' Log successful reports/events on pages in google analytics?
    Public Shared ReadOnly Property trackSuccessfulReports As Boolean
        Get
            Return appSettings("trackSuccessfulReports")
        End Get
    End Property

End Class


Public Class dev

    Public Shared ReadOnly Property dbcon As String
        Get
            Return ConfigurationManager.ConnectionStrings("db").ConnectionString
        End Get
    End Property

    Public Shared ReadOnly Property isLocal As Boolean
        Get
            Dim local As Boolean = False
            Dim serverName As String = HttpContext.Current.Request.ServerVariables("SERVER_NAME")
            If HttpContext.Current.Request.IsLocal Or InStr(serverName, "localhost") Or InStr(serverName, "172.16.1") Then
                local = True
            End If
            Return local
        End Get
    End Property

    Public Shared Sub log(ByVal ex As String)
        If isLocal Then
            System.Diagnostics.Debug.Print(ex)
        End If
    End Sub

    Public Shared Sub throwReport(ByVal reportClass As String, ByVal reportText As String, ByRef control As HtmlGenericControl, ByVal gaAction As String, ByVal gaLabel As String, Optional ByVal displayReport As Boolean = True, Optional ByVal trackEvent As Boolean = True)
        control.InnerHtml = ""
        control.InnerHtml = reportText.ToString
        If trackEvent Then
            Dim category As String = "'page: ' + document.location.pathname + document.location.search" 'document.referrer
            control.InnerHtml += String.Format("<script>ga('send', 'event', {0}, '{1}', '{2} - ' );</script>",
                                               category, gaAction, gaLabel)
        End If
        control.Visible = True
        control.Attributes.Add("class", "report " & reportClass)
        If Not displayReport Then
            control.Attributes.Add("style", "display:none")
        End If
    End Sub

    Public Shared Function stripHtml(ByVal htmlString As String) As String
        'Return Regex.Replace(htmlString, "<.*?>", "")
        Dim matchPattern As String = "<(?:[^>=]|='[^']*'|=""[^""]*""|=[^'""][^\s>]*)*>"
        Return Regex.Replace(htmlString, matchPattern, "", RegexOptions.IgnoreCase Or RegexOptions.IgnorePatternWhitespace Or RegexOptions.Multiline Or RegexOptions.Singleline)
    End Function

    Public Shared Sub reportError(ByVal reportText As String, ByRef control As HtmlGenericControl, ByVal gaAction As String, Optional ByVal displayReport As Boolean = True)
        throwReport("error", reportText, control, gaAction, "report: " & dev.stripHtml(reportText), displayReport, True)
    End Sub

    Public Shared Sub reportSuccess(ByVal reportText As String, ByRef control As HtmlGenericControl, ByVal gaAction As String, Optional ByVal displayReport As Boolean = True)
        Dim trackSuccess As Boolean = settings.trackSuccessfulReports
        throwReport("success", reportText, control, gaAction, "report: " & dev.stripHtml(reportText), displayReport, trackSuccess)
    End Sub

    Public Shared Sub reportInfo(ByVal reportText As String, ByRef control As HtmlGenericControl, ByVal gaAction As String, Optional ByVal displayReport As Boolean = True)
        Dim trackSuccess As Boolean = settings.trackSuccessfulReports
        throwReport("info", reportText, control, gaAction, "report: " & dev.stripHtml(reportText), displayReport, trackSuccess)
    End Sub

End Class


Public Class utils

    Public Shared Function stripNonAscii(ByVal inputString As String) As String
        Dim newString As String = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(Encoding.ASCII.EncodingName, New EncoderReplacementFallback(String.Empty), New DecoderExceptionFallback()), Encoding.UTF8.GetBytes(inputString)))
        Return newString
    End Function

    Public Shared Function fileExists(ByVal FileFullPath As String) As Boolean
        Dim f As New IO.FileInfo(FileFullPath)
        Return f.Exists
    End Function

    Public Shared Function validEmail(ByVal email As String) As Boolean
        Try
            Dim a As New System.Net.Mail.MailAddress(email)
        Catch
            Return False
        End Try
        Return True
    End Function

End Class